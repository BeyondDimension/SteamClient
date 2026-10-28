using Google.Protobuf;

namespace BD.SteamClient.UnitTest;

/// <summary>Offline tests for displaying a QR code and waiting for Steam Mobile confirmation.</summary>
public sealed class SteamDisplayQrLoginTests
{
    const ulong SteamId = 76561198000000001;

    [TestCase(null)]
    [TestCase(EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp)]
    public async Task PendingAndRotatedChallengePollUntilTokensThenFinalizeCookiesAndSession(EAuthTokenPlatformType? platformType)
    {
        var polls = 0;
        var accessToken = platformType == EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp
            ? "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"sub\":\"76561198000000001\",\"aud\":[\"mobile\"],\"exp\":4102444800}")).TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".offline-signature"
            : "offline-access";
        var displayed = new List<(ulong ClientId, string Url)>();
        var cookies = new CookieContainer();
        cookies.Add(new Cookie("sessionid", "offline-session", "/", "store.steampowered.com"));
        var handler = new FakeHandler(async (request, token) =>
        {
            switch (request.RequestUri!.AbsolutePath.TrimEnd('/'))
            {
                case "/IAuthenticationService/BeginAuthSessionViaQR/v1":
                    var begin = CAuthentication_BeginAuthSessionViaQR_Request.Parser.ParseFrom(await ReadInputAsync(request, token));
                    Assert.That(begin.PlatformType, Is.EqualTo(platformType ?? EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser));
                    Assert.That(begin.DeviceDetails.PlatformType, Is.EqualTo(platformType ?? EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser));
                    return Response(Begin());
                case "/IAuthenticationService/PollAuthSessionStatus/v1":
                    var poll = CAuthentication_PollAuthSessionStatus_Request.Parser.ParseFrom(await ReadInputAsync(request, token));
                    Assert.That(poll.ClientId, Is.EqualTo(polls == 0 ? 123UL : 456UL));
                    Assert.That(poll.RequestId.ToStringUtf8(), Is.EqualTo("offline-request"));
                    polls++;
                    return Response(polls == 1
                        ? new CAuthentication_PollAuthSessionStatus_Response { NewClientId = 456, NewChallengeUrl = "https://s.team/q/1/456" }
                        : new CAuthentication_PollAuthSessionStatus_Response { AccessToken = accessToken, RefreshToken = "offline-refresh", AccountName = "offline-account" });
                case "/jwt/finalizelogin":
                    var finalize = await ReadFormAsync(request, token);
                    Assert.That(finalize["nonce"], Is.EqualTo("offline-refresh"));
                    Assert.That(finalize["sessionid"], Is.EqualTo("offline-session"));
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"steamID\":\"76561198000000001\",\"transfer_info\":[{\"url\":\"https://steamcommunity.com/login/settoken\",\"params\":{\"nonce\":\"transfer-nonce\",\"auth\":\"transfer-auth\"}}]}", Encoding.UTF8, "application/json"),
                    };
                case "/login/settoken":
                    var transfer = await ReadFormAsync(request, token);
                    Assert.That(transfer["steamID"], Is.EqualTo("76561198000000001"));
                    Assert.That(transfer["nonce"], Is.EqualTo("transfer-nonce"));
                    cookies.Add(new Cookie("steamLoginSecure", "offline-cookie", "/", "steamcommunity.com"));
                    return new HttpResponseMessage(HttpStatusCode.OK);
                default:
                    throw new AssertionException("Unexpected HTTP request: " + request.RequestUri.AbsolutePath);
            }
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, container =>
        {
            container.Add(cookies.GetAllCookies());
            cookies = container;
            return handler;
        });
        var state = new SteamLoginState { Success = true, Requires2FA = true, RequiresEmailAuth = true, RequiresCaptcha = true };

        Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>> display = session =>
        {
            displayed.Add((session.ClientId, session.ChallengeUrl));
            return Task.FromResult(true);
        };
        if (platformType.HasValue)
        {
            await service.DoLoginWithQRCodeAsync(state, display, platformType.Value);
        }
        else
        {
            await service.DoLoginWithQRCodeAsync(state, display);
        }

        Assert.That(state.Success, Is.True);
        Assert.That(state.Message, Is.Null);
        Assert.That(state.Requires2FA || state.RequiresEmailAuth || state.RequiresCaptcha, Is.False);
        Assert.That(state.SteamId, Is.EqualTo(SteamId));
        Assert.That(state.SteamIdString, Is.EqualTo("76561198000000001"));
        Assert.That(state.Username, Is.EqualTo("offline-account"));
        Assert.That(state.Cookies!.Cast<Cookie>().Any(x => x.Name == "steamLoginSecure"), Is.True);
        Assert.That(displayed, Is.EqualTo(new[] { (123UL, "https://s.team/q/1/123"), (456UL, "https://s.team/q/1/456") }));
        var saved = services.GetRequiredService<ISteamSessionService>().RentSession("76561198000000001");
        Assert.That(saved, Is.Not.Null);
        Assert.That(state.AccessToken, Is.EqualTo(accessToken));
        Assert.That(saved!.AccessToken, Is.EqualTo(accessToken));
        Assert.That(saved.RefreshToken, Is.EqualTo("offline-refresh"));
        services.GetRequiredService<ISteamSessionService>().RemoveSession("76561198000000001");
    }

    [Test]
    public void AlreadyCancelledLoginDoesNotSendOrKeepPreviousSuccess()
    {
        var handler = new FakeHandler((_, _) => throw new AssertionException("A cancelled login cannot send HTTP requests."));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var state = new SteamLoginState { Success = true, AccessToken = "old", RefreshToken = "old" };

        Assert.CatchAsync<OperationCanceledException>(() => service.DoLoginWithQRCodeAsync(state, _ => Task.FromResult(true), cancellationToken: cancellation.Token));

        Assert.That(state.Success, Is.False);
        Assert.That(state.AccessToken, Is.Null);
        Assert.That(state.RefreshToken, Is.Null);
    }

    [Test]
    public async Task CancellationInterruptsDisplayCallbackBeforePolling()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new FakeHandler((request, _) => Task.FromResult(request.RequestUri!.Host == "store.steampowered.com"
            ? new HttpResponseMessage(HttpStatusCode.OK) : Response(Begin())));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        using var cancellation = new CancellationTokenSource();
        var state = new SteamLoginState();
        var login = service.DoLoginWithQRCodeAsync(state, _ =>
        {
            entered.SetResult();
            return pending.Task;
        }, cancellationToken: cancellation.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        try
        {
            Assert.CatchAsync<OperationCanceledException>(async () => await login.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.That(state.Success, Is.False);
        }
        finally
        {
            pending.TrySetResult(false);
            await login.ContinueWith(_ => { });
        }
    }

    [TestCase("15")]
    [TestCase("27")]
    [TestCase("84")]
    [TestCase("invalid")]
    [TestCase(null)]
    public void InvalidResultCannotDisplayOrCreateSession(string? result)
    {
        var handler = new FakeHandler((request, _) => Task.FromResult(request.RequestUri!.Host == "store.steampowered.com"
            ? new HttpResponseMessage(HttpStatusCode.OK) : Response(Begin(), result)));
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler) { UseRetry = false };
        var state = new SteamLoginState();

        Assert.ThrowsAsync<InvalidOperationException>(() => service.DoLoginWithQRCodeAsync(state,
            _ => throw new AssertionException("A rejected begin response must never display a QR.")));

        Assert.That(state.Success, Is.False);
    }

    [TestCase("/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1")]
    [TestCase("/IAuthenticationService/PollAuthSessionStatus/v1")]
    [TestCase("/jwt/finalizelogin")]
    [TestCase("/login/settoken")]
    public async Task PasswordCancellationStopsHttpAndCannotRegisterSession(string cancelledPath)
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        var handler = new FakeHandler(async (request, token) =>
        {
            var path = request.RequestUri!.AbsolutePath.TrimEnd('/');
            if (path == cancelledPath)
            {
                entered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            }
            return path switch
            {
                "/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1" => Response(new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response()),
                "/IAuthenticationService/PollAuthSessionStatus/v1" => Response(new CAuthentication_PollAuthSessionStatus_Response { AccessToken = "offline-access", RefreshToken = "offline-refresh" }),
                "/jwt/finalizelogin" => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"steamID\":\"76561198000000001\",\"transfer_info\":[{\"url\":\"https://steamcommunity.com/login/settoken\",\"params\":{\"nonce\":\"nonce\",\"auth\":\"auth\"}}]}", Encoding.UTF8, "application/json"),
                },
                "/login/settoken" => new HttpResponseMessage(HttpStatusCode.OK),
                _ => throw new AssertionException("Unexpected password login request: " + path),
            };
        });
        using var services = CreateServices();
        using var service = new SteamAccountService(services, _ => handler);
        var state = new SteamLoginState { SteamId = SteamId, ClientId = 123, RequestId = [1], Requires2FA = true, TwofactorCode = "ABCDE" };
        var login = service.DoLoginV2Async(state, cancellationToken: cancellation.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        try
        {
            Assert.CatchAsync<OperationCanceledException>(async () => await login.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.That(state.Success, Is.False);
            Assert.That(services.GetRequiredService<ISteamSessionService>().RentSession("76561198000000001"), Is.Null);
        }
        finally
        {
            // Unblock the old implementation too when observing the regression's failing run.
            service.Dispose();
            await login.ContinueWith(_ => { }).WaitAsync(TimeSpan.FromSeconds(10));
        }
    }

    static CAuthentication_BeginAuthSessionViaQR_Response Begin() => new()
    {
        ClientId = 123,
        ChallengeUrl = "https://s.team/q/1/123",
        RequestId = ByteString.CopyFromUtf8("offline-request"),
        Interval = .01f,
        Version = 1,
    };

    static ServiceProvider CreateServices() => new ServiceCollection()
        .AddLogging()
        .AddSingleton<IRandomGetUserAgentService, SteamAccountServiceTest.TestRandomGetUserAgentService>()
        .AddSingleton<ISteamSessionService, SteamSessionServiceImpl>()
        .BuildServiceProvider();

    static async Task<Dictionary<string, string>> ReadFormAsync(HttpRequestMessage request, CancellationToken token)
        => (await request.Content!.ReadAsStringAsync(token)).Split('&').Select(part => part.Split('=', 2))
            .ToDictionary(part => WebUtility.UrlDecode(part[0]), part => WebUtility.UrlDecode(part[1]));

    static async Task<byte[]> ReadInputAsync(HttpRequestMessage request, CancellationToken token)
        => Convert.FromBase64String((await ReadFormAsync(request, token))["input_protobuf_encoded"]);

    static HttpResponseMessage Response(IMessage message, string? result = "1")
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(message.ToByteArray()) };
        if (result is not null)
            response.Headers.Add("X-eresult", result);
        return response;
    }

    sealed class FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
