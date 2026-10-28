using System.Security.Cryptography;
using Google.Protobuf;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.SteamClient.UnitTest;

/// <summary>Offline protocol tests: never read state.json, contact Steam, or approve a real login.</summary>
public sealed class SteamLoginQrTests
{
    const ulong SteamId = 0x1112131415161718UL;
    static readonly SteamLoginQrChallenge Challenge = new(0x1234, 0x0102030405060708UL);

    [TestCase("https://s.team/q/1/1", 1, 1UL)]
    [TestCase("https://S.TEAM/q/65535/18446744073709551615", 65535, ulong.MaxValue)]
    [TestCase("https://s.team:443/q/1/2", 1, 2UL)]
    public void ParsesStrictChallenge(string text, int version, ulong clientId)
    {
        Assert.That(SteamLoginQrChallenge.TryParse(text, out var challenge), Is.True);
        Assert.That(challenge, Is.EqualTo(new SteamLoginQrChallenge(version, clientId)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("http://s.team/q/1/1")]
    [TestCase("https://s.team.evil.example/q/1/1")]
    [TestCase("https://s.team@evil.example/q/1/1")]
    [TestCase("https://evil.example@s.team/q/1/1")]
    [TestCase("https://s.team:8443/q/1/1")]
    [TestCase("https://s.team/q/1/1/")]
    [TestCase("https://s.team/q/1/1?x=1")]
    [TestCase("https://s.team/q/1/1#fragment")]
    [TestCase("https://s.team/q/1/1/extra")]
    [TestCase("https://s.team/q/0/1")]
    [TestCase("https://s.team/q/65536/1")]
    [TestCase("https://s.team/q/1/0")]
    [TestCase("https://s.team/q/1/18446744073709551616")]
    [TestCase("https://s.team/q/+1/1")]
    [TestCase("https://s.team/q/1/%31")]
    [TestCase("https://s.team/q/1/1\n")]
    [TestCase(" https://s.team/q/1/1")]
    public void RejectsUntrustedChallenge(string? text)
    {
        Assert.That(SteamLoginQrChallenge.TryParse(text, out var challenge), Is.False);
        Assert.That(challenge, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ApprovalEncodesLittleEndianHmacAndPersistence(bool persistent)
    {
        var token = Token();
        var secret = Enumerable.Range(1, 20).Select(x => (byte)x).ToArray();
        var handler = new FakeHandler(async (request, cancellationToken) =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(request.RequestUri!.GetLeftPart(UriPartial.Path), Is.EqualTo(
                "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithMobileConfirmation/v1/"));
            Assert.That(request.RequestUri.Query, Is.EqualTo("?access_token=" + Uri.EscapeDataString(token)));
            var input = CAuthentication_UpdateAuthSessionWithMobileConfirmation_Request.Parser.ParseFrom(
                await ReadInputAsync(request, cancellationToken));
            Assert.That(input.Version, Is.EqualTo(Challenge.Version));
            Assert.That(input.ClientId, Is.EqualTo(Challenge.ClientId));
            Assert.That(input.Steamid, Is.EqualTo(SteamId));
            Assert.That(input.Confirm, Is.True);
            Assert.That(input.Persistence, Is.EqualTo(persistent
                ? ESessionPersistence.KEsessionPersistencePersistent : ESessionPersistence.KEsessionPersistenceEphemeral));
            byte[] expectedData = [0x34, 0x12, 8, 7, 6, 5, 4, 3, 2, 1, 0x18, 0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11];
            Assert.That(input.Signature.ToByteArray(), Is.EqualTo(HMACSHA256.HashData(secret, expectedData)));
            return Response(new CAuthentication_UpdateAuthSessionWithMobileConfirmation_Response());
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        await service.ApproveQrAuthSessionAsync(token, Challenge, SteamId, secret, persistent);
        Assert.That(handler.Calls, Is.EqualTo(1));
    }

    [TestCase("5")]
    [TestCase("15")]
    [TestCase("27")]
    [TestCase("84")]
    [TestCase("not-a-result")]
    [TestCase(null)]
    public void RejectedOrMissingResultCannotApprove(string? result)
    {
        var handler = new FakeHandler((_, _) => Task.FromResult(Response(
            new CAuthentication_UpdateAuthSessionWithMobileConfirmation_Response(), result)));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        Assert.CatchAsync<InvalidOperationException>(() => service.ApproveQrAuthSessionAsync(
            Token(), Challenge, SteamId, new byte[20], true));
        Assert.That(handler.Calls, Is.EqualTo(1));
    }

    [TestCase("web", false, false, SteamId)]
    [TestCase("mobile", true, false, SteamId)]
    [TestCase("mobile", false, true, SteamId)]
    [TestCase("mobile", false, false, SteamId + 1)]
    public void UnsuitableTokensCannotSendApproval(string audience, bool refresh, bool expired, ulong subject)
    {
        var handler = new FakeHandler((_, _) => throw new AssertionException("No request should be sent."));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        Assert.ThrowsAsync<ArgumentException>(() => service.ApproveQrAuthSessionAsync(
            Token(audience, refresh, expired, subject), Challenge, SteamId, new byte[20], true));
        Assert.That(handler.Calls, Is.Zero);
    }

    [Test]
    public void CancelledApprovalDoesNotSend()
    {
        var handler = new FakeHandler((_, _) => throw new AssertionException("No request should be sent."));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(() => service.ApproveQrAuthSessionAsync(
            Token(), Challenge, SteamId, new byte[20], true, cancellation.Token));
        Assert.That(handler.Calls, Is.Zero);
    }

    [Test]
    public async Task CancellingMobileLoginStopsOutstandingRequest()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new FakeHandler(async (_, token) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new AssertionException("A cancelled login must not complete its request.");
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        using var cancellation = new CancellationTokenSource();
        var state = new SteamLoginState { Username = "offline", Password = "offline-password" };
        var login = service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        Assert.ThrowsAsync<TaskCanceledException>(async () => await login.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.That(state.Success, Is.False);
    }

    static string Token(string audience = "mobile", bool refresh = false, bool expired = false, ulong subject = SteamId)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = subject.ToString(CultureInfo.InvariantCulture),
            aud = refresh ? new[] { audience, "derive" } : new[] { audience },
            exp = DateTimeOffset.UtcNow.AddHours(expired ? -1 : 1).ToUnixTimeSeconds(),
        });
        return "e30." + Convert.ToBase64String(payload).TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".offline-signature";
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ExplicitMobileLoginUsesDeviceCodeAndDoesNotFinalizeWebSession(bool supplyCodeAtStart)
    {
        using var rsa = RSA.Create(2048);
        var key = rsa.ExportParameters(false);
        var token = Token();
        var refresh = Token(refresh: true);
        var handler = new FakeHandler(async (request, cancellationToken) =>
        {
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/IAuthenticationService/GetPasswordRSAPublicKey/v1/":
                    Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                    Assert.That(request.RequestUri.Query, Does.Contain("origin=SteamMobile"));
                    return Response(new CAuthentication_GetPasswordRSAPublicKey_Response
                    {
                        PublickeyExp = Convert.ToHexString(key.Exponent!),
                        PublickeyMod = Convert.ToHexString(key.Modulus!),
                        Timestamp = 123,
                    }, result: null);
                case "/IAuthenticationService/BeginAuthSessionViaCredentials/v1/":
                    // Steam's gateway returns 502 for the quoted boundary emitted by .NET.
                    var boundary = request.Content!.Headers.ContentType!.Parameters.Single(x => x.Name == "boundary").Value!;
                    if (boundary.StartsWith('"'))
                        return new HttpResponseMessage(HttpStatusCode.BadGateway);
                    var beginInput = CAuthentication_BeginAuthSessionViaCredentials_Request.Parser.ParseFrom(
                        await ReadInputAsync(request, cancellationToken));
                    Assert.That(beginInput.AccountName, Is.EqualTo("offline"));
                    Assert.That(beginInput.EncryptionTimestamp, Is.EqualTo(123));
                    var decryptedPassword = rsa.Decrypt(Convert.FromBase64String(beginInput.EncryptedPassword), RSAEncryptionPadding.Pkcs1);
                    try
                    {
                        Assert.That(Encoding.ASCII.GetString(decryptedPassword), Is.EqualTo("offline-password"));
                    }
                    finally
                    {
                        CryptographicOperations.ZeroMemory(decryptedPassword);
                    }
                    Assert.That(beginInput.WebsiteId, Is.EqualTo("Mobile"));
                    Assert.That(beginInput.PlatformType, Is.EqualTo(EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp));
                    Assert.That(beginInput.DeviceDetails.PlatformType, Is.EqualTo(EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp));
                    var begin = new CAuthentication_BeginAuthSessionViaCredentials_Response
                    {
                        ClientId = Challenge.ClientId, Steamid = SteamId, RequestId = ByteString.CopyFromUtf8("offline"),
                    };
                    begin.AllowedConfirmations.Add(new CAuthentication_AllowedConfirmation
                    {
                        ConfirmationType = EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode,
                    });
                    return Response(begin);
                case "/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1/":
                    var guard = CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request.Parser.ParseFrom(
                        await ReadInputAsync(request, cancellationToken));
                    Assert.That(guard.Code, Is.EqualTo("ABCDE"));
                    Assert.That(guard.CodeType, Is.EqualTo(EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode));
                    return Response(new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response());
                case "/IAuthenticationService/PollAuthSessionStatus/v1/":
                    return Response(new CAuthentication_PollAuthSessionStatus_Response { AccessToken = token, RefreshToken = refresh });
                default:
                    throw new AssertionException("Mobile login must not finalize a web session or send other requests.");
            }
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState
        {
            Username = "offline", Password = "offline-password", TwofactorCode = supplyCodeAtStart ? "ABCDE" : null,
        };
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        if (!supplyCodeAtStart)
        {
            Assert.That(state.Success, Is.False);
            Assert.That(state.Requires2FA, Is.True);
            Assert.That(handler.Calls, Is.EqualTo(2));
            state.TwofactorCode = "ABCDE";
            await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        }
        Assert.That(state.Success, Is.True);
        Assert.That(state.Requires2FA, Is.False);
        Assert.That(state.AccessToken, Is.EqualTo(token));
        Assert.That(state.RefreshToken, Is.EqualTo(refresh));
        Assert.That(handler.Calls, Is.EqualTo(4));
    }

    [Test]
    public void MissingResultDoesNotAllowAnEmptyRsaKey()
    {
        var handler = new FakeHandler((_, _) => Task.FromResult(Response(
            new CAuthentication_GetPasswordRSAPublicKey_Response(), result: null)));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState { Username = "offline", Password = "offline-password" };
        Assert.ThrowsAsync<InvalidOperationException>(() => service.DoLoginV2Async(
            state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp));
        Assert.That(handler.Calls, Is.EqualTo(1));
        Assert.That(state.ClientId, Is.Null);
        Assert.That(state.Success, Is.False);
    }

    [Test]
    public async Task InvalidPasswordReturnsCredentialFailureWithoutStartingASession()
    {
        using var rsa = RSA.Create(2048);
        var key = rsa.ExportParameters(false);
        var handler = new FakeHandler((request, _) => Task.FromResult(request.RequestUri!.AbsolutePath switch
        {
            "/IAuthenticationService/GetPasswordRSAPublicKey/v1/" => Response(new CAuthentication_GetPasswordRSAPublicKey_Response
            {
                PublickeyExp = Convert.ToHexString(key.Exponent!), PublickeyMod = Convert.ToHexString(key.Modulus!), Timestamp = 123,
            }),
            "/IAuthenticationService/BeginAuthSessionViaCredentials/v1/" => Response(
                new CAuthentication_BeginAuthSessionViaCredentials_Response(), "5"),
            _ => throw new AssertionException("Rejected credentials must not poll or finalize a session."),
        }));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState { Username = "offline", Password = "offline-password", Requires2FA = true };
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        Assert.That(state.Success, Is.False);
        Assert.That(state.Message, Is.EqualTo("请核对您的密码和账户名称并重试。"));
        Assert.That(state.ClientId, Is.Null);
        Assert.That(state.RequestId, Is.Null);
        Assert.That(state.Requires2FA, Is.False);
        Assert.That(state.AccessToken, Is.Null);
        Assert.That(state.RefreshToken, Is.Null);
        Assert.That(handler.Calls, Is.EqualTo(2));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task PendingPollKeepsRotatedSessionWithoutResubmittingAcceptedCode(bool submitCode)
    {
        const ulong rotatedClientId = 123456;
        var pollCount = 0;
        var guardCount = 0;
        var token = Token();
        var refresh = Token(refresh: true);
        var state = new SteamLoginState
        {
            ClientId = Challenge.ClientId, SteamId = SteamId, RequestId = [1, 2, 3],
            Requires2FA = true, TwofactorCode = submitCode ? "ABCDE" : null,
        };
        var handler = new FakeHandler(async (request, cancellationToken) =>
        {
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1/":
                    guardCount++;
                    Assert.That(guardCount, Is.EqualTo(1));
                    return Response(new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response());
                case "/IAuthenticationService/PollAuthSessionStatus/v1/":
                    var poll = CAuthentication_PollAuthSessionStatus_Request.Parser.ParseFrom(await ReadInputAsync(request, cancellationToken));
                    Assert.That(poll.ClientId, Is.EqualTo(pollCount == 0 ? Challenge.ClientId : rotatedClientId));
                    Assert.That(poll.RequestId.ToByteArray(), Is.EqualTo(new byte[] { 1, 2, 3 }));
                    pollCount++;
                    return Response(pollCount == 1
                        ? new CAuthentication_PollAuthSessionStatus_Response { NewClientId = rotatedClientId }
                        : new CAuthentication_PollAuthSessionStatus_Response { AccessToken = token, RefreshToken = refresh });
                default:
                    throw new AssertionException("Pending retries must reuse the original authentication session.");
            }
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        Assert.That(state.Success, Is.False);
        Assert.That(state.ClientId, Is.EqualTo(rotatedClientId));
        Assert.That(state.RequestId, Is.EqualTo(new byte[] { 1, 2, 3 }));
        Assert.That(state.Requires2FA, Is.EqualTo(!submitCode));
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        Assert.That(state.Success, Is.True);
        Assert.That(state.Requires2FA, Is.False);
        Assert.That(state.RequiresEmailAuth, Is.False);
        Assert.That(state.AccessToken, Is.EqualTo(token));
        Assert.That(guardCount, Is.EqualTo(submitCode ? 1 : 0));
        Assert.That(pollCount, Is.EqualTo(2));
    }

    [TestCase(true, "88")]
    [TestCase(false, "65")]
    public async Task RejectedGuardCodeRetainsTheChallengeForCorrection(bool deviceCode, string result)
    {
        var attempts = 0;
        var state = new SteamLoginState
        {
            ClientId = Challenge.ClientId, SteamId = SteamId, RequestId = [1, 2, 3],
            Requires2FA = deviceCode, RequiresEmailAuth = !deviceCode, TwofactorCode = "BAD", EmailCode = "BAD",
        };
        var handler = new FakeHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri!.AbsolutePath == "/IAuthenticationService/PollAuthSessionStatus/v1/")
                return Response(new CAuthentication_PollAuthSessionStatus_Response { AccessToken = Token(), RefreshToken = Token(refresh: true) });
            Assert.That(request.RequestUri.AbsolutePath, Is.EqualTo("/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1/"));
            var guard = CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request.Parser.ParseFrom(await ReadInputAsync(request, cancellationToken));
            Assert.That(guard.CodeType, Is.EqualTo(deviceCode
                ? EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode : EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode));
            Assert.That(guard.ClientId, Is.EqualTo(Challenge.ClientId));
            Assert.That(guard.Code, Is.EqualTo(attempts++ == 0 ? "BAD" : "ABCDE"));
            return Response(new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response(), attempts == 1 ? result : "1");
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var error = Assert.CatchAsync<InvalidOperationException>(() => service.DoLoginV2Async(
            state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp));
        Assert.That(error!.Message, Does.Contain("EResult " + result));
        Assert.That(state.Success, Is.False);
        Assert.That(state.Requires2FA, Is.EqualTo(deviceCode));
        Assert.That(state.RequiresEmailAuth, Is.EqualTo(!deviceCode));
        Assert.That(state.ClientId, Is.EqualTo(Challenge.ClientId));
        Assert.That(state.RequestId, Is.EqualTo(new byte[] { 1, 2, 3 }));
        Assert.That(handler.Calls, Is.EqualTo(1));
        state.TwofactorCode = state.EmailCode = "ABCDE";
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        Assert.That(state.Success, Is.True);
        Assert.That(handler.Calls, Is.EqualTo(3));
    }

    [Test]
    public async Task RefreshOnlySuccessDerivesMobileAccessAndCanResumeInterruptedDerivation()
    {
        var token = Token();
        var refresh = Token(refresh: true);
        var derivations = 0;
        var polls = 0;
        var handler = new FakeHandler(async (request, cancellationToken) =>
        {
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/IAuthenticationService/PollAuthSessionStatus/v1/":
                    Assert.That(++polls, Is.EqualTo(1));
                    return Response(new CAuthentication_PollAuthSessionStatus_Response { RefreshToken = refresh });
                case "/IAuthenticationService/GenerateAccessTokenForApp/v1/":
                    var input = CAuthentication_AccessToken_GenerateForApp_Request.Parser.ParseFrom(await ReadInputAsync(request, cancellationToken));
                    Assert.That(input.RefreshToken, Is.EqualTo(refresh));
                    Assert.That(input.Steamid, Is.EqualTo(SteamId));
                    if (++derivations == 1)
                        throw new HttpRequestException("Offline interruption");
                    return Response(new CAuthentication_AccessToken_GenerateForApp_Response { AccessToken = token });
                default:
                    throw new AssertionException("Access derivation must not restart login or finalize web cookies.");
            }
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState { ClientId = Challenge.ClientId, SteamId = SteamId, RequestId = [1], Requires2FA = true };
        Assert.ThrowsAsync<HttpRequestException>(() => service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp));
        Assert.That(state.Success, Is.False);
        Assert.That(state.RefreshToken, Is.EqualTo(refresh));
        Assert.That(state.Requires2FA, Is.False);
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        Assert.That(state.Success, Is.True);
        Assert.That(state.AccessToken, Is.EqualTo(token));
        Assert.That(state.RefreshToken, Is.EqualTo(refresh));
        Assert.That(polls, Is.EqualTo(1));
        Assert.That(derivations, Is.EqualTo(2));
    }

    [Test]
    public async Task AlternativeMobileConfirmationCanCompleteBeforeAnyGuardCodeIsEntered()
    {
        using var rsa = RSA.Create(2048);
        var key = rsa.ExportParameters(false);
        var token = Token();
        var handler = new FakeHandler((request, _) =>
        {
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/IAuthenticationService/GetPasswordRSAPublicKey/v1/":
                    return Task.FromResult(Response(new CAuthentication_GetPasswordRSAPublicKey_Response
                    {
                        PublickeyExp = Convert.ToHexString(key.Exponent!), PublickeyMod = Convert.ToHexString(key.Modulus!), Timestamp = 123,
                    }));
                case "/IAuthenticationService/BeginAuthSessionViaCredentials/v1/":
                    var begin = new CAuthentication_BeginAuthSessionViaCredentials_Response
                    {
                        ClientId = Challenge.ClientId, Steamid = SteamId, RequestId = ByteString.CopyFromUtf8("offline"),
                    };
                    begin.AllowedConfirmations.Add(new CAuthentication_AllowedConfirmation
                    {
                        ConfirmationType = EAuthSessionGuardType.KEauthSessionGuardTypeDeviceConfirmation,
                    });
                    begin.AllowedConfirmations.Add(new CAuthentication_AllowedConfirmation
                    {
                        ConfirmationType = EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode,
                    });
                    return Task.FromResult(Response(begin));
                case "/IAuthenticationService/PollAuthSessionStatus/v1/":
                    return Task.FromResult(Response(new CAuthentication_PollAuthSessionStatus_Response
                    {
                        AccessToken = token, RefreshToken = Token(refresh: true),
                    }));
                default:
                    throw new AssertionException("Remote confirmation must not require a guard code submission.");
            }
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState { Username = "offline", Password = "offline-password" };
        await service.DoLoginV2Async(state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp);
        Assert.That(state.Success, Is.True);
        Assert.That(state.Requires2FA, Is.False);
        Assert.That(state.AccessToken, Is.EqualTo(token));
        Assert.That(handler.Calls, Is.EqualTo(3));
    }

    [TestCase("15")]
    [TestCase("not-a-result")]
    public void RsaEndpointDoesNotIgnoreAnExplicitFailedOrMalformedResult(string result)
    {
        var handler = new FakeHandler((_, _) => Task.FromResult(Response(
            new CAuthentication_GetPasswordRSAPublicKey_Response(), result)));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState { Username = "offline", Password = "offline-password" };
        Assert.CatchAsync<InvalidOperationException>(() => service.DoLoginV2Async(
            state, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp));
        Assert.That(state.Success, Is.False);
        Assert.That(state.ClientId, Is.Null);
        Assert.That(handler.Calls, Is.EqualTo(1));
    }

    static ServiceProvider CreateServices() => new ServiceCollection()
        .AddLogging()
        .AddSingleton<IRandomGetUserAgentService, SteamAccountServiceTest.TestRandomGetUserAgentService>()
        .AddSingleton<ISteamSessionService, SteamSessionServiceImpl>()
        .BuildServiceProvider();

    static async Task<byte[]> ReadInputAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Parse the transmitted MIME body, not the HttpContent object graph: the latter
        // hid an unquoted field name that Steam fails to recognize on the wire.
        var boundary = request.Content!.Headers.ContentType!.Parameters.Single(x => x.Name == "boundary").Value!.Trim('"');
        var body = await request.Content.ReadAsStringAsync(cancellationToken);
        foreach (var part in body.Split("--" + boundary, StringSplitOptions.RemoveEmptyEntries))
        {
            var headerEnd = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (headerEnd < 0 || !part[..headerEnd].Contains("name=\"input_protobuf_encoded\"", StringComparison.Ordinal))
                continue;
            return Convert.FromBase64String(part[(headerEnd + 4)..].Trim());
        }
        throw new AssertionException("The serialized request has no Steam-compatible protobuf form field.");
    }

    static HttpResponseMessage Response(IMessage message, string? result = "1")
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(message.ToByteArray()) };
        if (result is not null)
            response.Headers.Add("X-eresult", result);
        return response;
    }

    sealed class FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return send(request, cancellationToken);
        }
    }
}
