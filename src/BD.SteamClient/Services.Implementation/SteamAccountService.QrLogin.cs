using System.Buffers.Binary;
using System.Security.Cryptography;
using Google.Protobuf;

namespace BD.SteamClient.Services;

public sealed partial class SteamAccountService
{
    async Task DoMobileLoginAsync(SteamLoginState loginState, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loginState);
        loginState.Success = false;
        if (loginState.ClientId is null)
        {
            loginState.AccessToken = null;
            loginState.RefreshToken = null;
            if (string.IsNullOrWhiteSpace(loginState.Username) || string.IsNullOrEmpty(loginState.Password))
            {
                loginState.Message = "请填写正确的 Steam 用户名密码";
                return;
            }
            loginState.Username = SteamUNPWDRegex().Replace(loginState.Username, string.Empty);
            loginState.Password = SteamUNPWDRegex().Replace(loginState.Password, string.Empty);
            var rsaInfo = await SendQrAuthRequestAsync("GetPasswordRSAPublicKey", null,
                new CAuthentication_GetPasswordRSAPublicKey_Request { AccountName = loginState.Username },
                CAuthentication_GetPasswordRSAPublicKey_Response.Parser, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(rsaInfo.PublickeyExp) || string.IsNullOrEmpty(rsaInfo.PublickeyMod) || rsaInfo.Timestamp == 0)
                throw new InvalidOperationException("Steam did not return a valid password encryption key.");
            string password;
            using (var rsa = RSA.Create())
            {
                rsa.ImportParameters(new RSAParameters
                {
                    Exponent = Convert.FromHexString(rsaInfo.PublickeyExp),
                    Modulus = Convert.FromHexString(rsaInfo.PublickeyMod),
                });
                var passwordBytes = Encoding.ASCII.GetBytes(loginState.Password);
                try
                {
                    password = Convert.ToBase64String(rsa.Encrypt(passwordBytes, RSAEncryptionPadding.Pkcs1));
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(passwordBytes);
                }
            }
            var begin = await SendQrAuthRequestAsync("BeginAuthSessionViaCredentials", null,
                new CAuthentication_BeginAuthSessionViaCredentials_Request
                {
                    AccountName = loginState.Username,
                    EncryptedPassword = password,
                    EncryptionTimestamp = rsaInfo.Timestamp,
                    WebsiteId = "Mobile",
                    PlatformType = EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp,
                    DeviceFriendlyName = "SteamTools",
                    DeviceDetails = new CAuthentication_DeviceDetails
                    {
                        DeviceFriendlyName = "SteamTools",
                        PlatformType = EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp,
                        OsType = -500, // EOSType.AndroidUnknown, matching the Steam mobile protocol.
                        GamingDeviceType = 528,
                    },
                    RememberLogin = true,
                    Persistence = ESessionPersistence.KEsessionPersistencePersistent,
                }, CAuthentication_BeginAuthSessionViaCredentials_Response.Parser, cancellationToken).ConfigureAwait(false);
            if (begin.ClientId == 0 || begin.Steamid == 0 || begin.RequestId.IsEmpty)
                throw new InvalidOperationException("Steam did not return a valid mobile login session.");
            loginState.ClientId = begin.ClientId;
            loginState.SteamId = begin.Steamid;
            loginState.RequestId = begin.RequestId.ToByteArray();
            // A device can offer multiple methods; prefer the TOTP secret this app already owns.
            loginState.Requires2FA = begin.AllowedConfirmations.Any(x => x.ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode);
            loginState.RequiresEmailAuth = !loginState.Requires2FA
                && begin.AllowedConfirmations.Any(x => x.ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode);
            // Confirmation and code can both be offered. Polling must remain possible without a code.
            if ((loginState.Requires2FA || loginState.RequiresEmailAuth)
                && string.IsNullOrWhiteSpace(loginState.Requires2FA ? loginState.TwofactorCode : loginState.EmailCode)
                && !begin.AllowedConfirmations.Any(x => x.ConfirmationType is EAuthSessionGuardType.KEauthSessionGuardTypeDeviceConfirmation
                    or EAuthSessionGuardType.KEauthSessionGuardTypeEmailConfirmation))
            {
                loginState.Message = loginState.Requires2FA ? "需要2FA验证码" : "需要邮箱验证码";
                return;
            }
        }
        if ((loginState.Requires2FA || loginState.RequiresEmailAuth)
            && !string.IsNullOrWhiteSpace(loginState.Requires2FA ? loginState.TwofactorCode : loginState.EmailCode))
        {
            var code = loginState.Requires2FA ? loginState.TwofactorCode : loginState.EmailCode;
            await SendQrAuthRequestAsync("UpdateAuthSessionWithSteamGuardCode", null,
                new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request
                {
                    ClientId = loginState.ClientId.Value,
                    Steamid = loginState.SteamId,
                    Code = code,
                    CodeType = loginState.Requires2FA
                        ? EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode
                        : EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode,
                }, CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response.Parser, cancellationToken).ConfigureAwait(false);
            loginState.Requires2FA = false;
            loginState.RequiresEmailAuth = false;
        }

        // Poll exactly once per call. Keep the session (including rotated client IDs) for caller-driven retries.
        // Steam can finish with only a refresh token; retain it if generating the access token is interrupted.
        if (string.IsNullOrEmpty(loginState.RefreshToken))
        {
            if (loginState.RequestId is null || loginState.ClientId is null)
                throw new InvalidOperationException("Steam mobile login session is missing.");
            var poll = await SendQrAuthRequestAsync("PollAuthSessionStatus", null,
                new CAuthentication_PollAuthSessionStatus_Request
                {
                    ClientId = loginState.ClientId.Value,
                    RequestId = ByteString.CopyFrom(loginState.RequestId),
                }, CAuthentication_PollAuthSessionStatus_Response.Parser, cancellationToken).ConfigureAwait(false);
            if (poll.HasNewClientId && poll.NewClientId != 0)
                loginState.ClientId = poll.NewClientId;
            if (string.IsNullOrEmpty(poll.RefreshToken))
            {
                loginState.Message = "Steam 移动登录尚未确认，请完成验证后重试。";
                return;
            }
            loginState.RefreshToken = poll.RefreshToken;
            loginState.AccessToken = poll.AccessToken;
        }
        loginState.Requires2FA = false;
        loginState.RequiresEmailAuth = false;
        if (string.IsNullOrEmpty(loginState.AccessToken))
        {
            var generated = await SendQrAuthRequestAsync("GenerateAccessTokenForApp", null,
                new CAuthentication_AccessToken_GenerateForApp_Request
                {
                    RefreshToken = loginState.RefreshToken,
                    Steamid = loginState.SteamId,
                }, CAuthentication_AccessToken_GenerateForApp_Response.Parser, cancellationToken).ConfigureAwait(false);
            loginState.AccessToken = generated.AccessToken;
        }
        if (ValidateQrAccessToken(loginState.AccessToken) != loginState.SteamId)
            throw new InvalidOperationException("Steam returned a mobile token for an unexpected account.");
        loginState.Success = true;
        loginState.Message = null;
    }

    /// <inheritdoc />
    public async Task<CAuthentication_GetAuthSessionInfo_Response> GetQrAuthSessionInfoAsync(
        string accessToken,
        SteamLoginQrChallenge challenge,
        CancellationToken cancellationToken = default)
    {
        ValidateQrChallenge(challenge);
        ValidateQrAccessToken(accessToken);
        var input = new CAuthentication_GetAuthSessionInfo_Request { ClientId = challenge.ClientId };
        return await SendQrAuthRequestAsync(
            "GetAuthSessionInfo", accessToken, input,
            CAuthentication_GetAuthSessionInfo_Response.Parser, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ApproveQrAuthSessionAsync(
        string accessToken,
        SteamLoginQrChallenge challenge,
        ulong steamId,
        byte[] sharedSecret,
        bool persistent,
        CancellationToken cancellationToken = default)
    {
        ValidateQrChallenge(challenge);
        var tokenSteamId = ValidateQrAccessToken(accessToken);
        if (steamId == 0 || steamId != tokenSteamId)
            throw new ArgumentException("The mobile access token does not belong to the selected Steam account.", nameof(steamId));
        ArgumentNullException.ThrowIfNull(sharedSecret);
        if (sharedSecret.Length == 0)
            throw new ArgumentException("A Steam Guard shared secret is required.", nameof(sharedSecret));
        cancellationToken.ThrowIfCancellationRequested();

        // Matches node-steam-session LoginApprover: uint16 LE version, uint64 LE client ID,
        // uint64 LE Steam ID, authenticated with the TOTP shared secret (not identity_secret).
        // https://github.com/DoctorMcKay/node-steam-session/blob/master/src/LoginApprover.ts
        var signature = CreateQrAuthSignature(challenge, steamId, sharedSecret);
        var input = new CAuthentication_UpdateAuthSessionWithMobileConfirmation_Request
        {
            Version = challenge.Version,
            ClientId = challenge.ClientId,
            Steamid = steamId,
            Signature = ByteString.CopyFrom(signature),
            Confirm = true,
            Persistence = persistent
                ? ESessionPersistence.KEsessionPersistencePersistent
                : ESessionPersistence.KEsessionPersistenceEphemeral,
        };
        CryptographicOperations.ZeroMemory(signature);
        await SendQrAuthRequestAsync(
            "UpdateAuthSessionWithMobileConfirmation", accessToken, input,
            CAuthentication_UpdateAuthSessionWithMobileConfirmation_Response.Parser, cancellationToken).ConfigureAwait(false);
    }

    static byte[] CreateQrAuthSignature(SteamLoginQrChallenge challenge, ulong steamId, byte[] sharedSecret)
    {
        Span<byte> data = stackalloc byte[18];
        BinaryPrimitives.WriteUInt16LittleEndian(data, checked((ushort)challenge.Version));
        BinaryPrimitives.WriteUInt64LittleEndian(data[2..], challenge.ClientId);
        BinaryPrimitives.WriteUInt64LittleEndian(data[10..], steamId);
        return HMACSHA256.HashData(sharedSecret, data);
    }

    static void ValidateQrChallenge(SteamLoginQrChallenge challenge)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        if (challenge.Version is <= 0 or > ushort.MaxValue || challenge.ClientId == 0)
            throw new ArgumentException("Invalid Steam login challenge.", nameof(challenge));
    }

    static ulong ValidateQrAccessToken(string accessToken)
    {
        const string invalidToken = "A valid, unexpired Steam MobileApp access token is required.";
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException(invalidToken, nameof(accessToken));
        try
        {
            var parts = accessToken.Split('.');
            if (parts.Length != 3 || parts[0].Length == 0 || parts[2].Length == 0)
                throw new FormatException();
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = document.RootElement;
            if (!root.TryGetProperty("aud", out var audience) || audience.ValueKind != JsonValueKind.Array
                || !audience.EnumerateArray().Any(x => x.ValueKind == JsonValueKind.String && x.GetString() == "mobile")
                || audience.EnumerateArray().Any(x => x.ValueKind == JsonValueKind.String && x.GetString() == "derive")
                || !root.TryGetProperty("exp", out var expiry) || !expiry.TryGetInt64(out var expires)
                || expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                || !root.TryGetProperty("sub", out var subject) || subject.ValueKind != JsonValueKind.String
                || !ulong.TryParse(subject.GetString(), NumberStyles.None, CultureInfo.InvariantCulture, out var steamId)
                || steamId == 0)
                throw new FormatException();
            // This is a local suitability check, not signature verification. Steam authenticates the token.
            return steamId;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or InvalidOperationException or OverflowException)
        {
            throw new ArgumentException(invalidToken, nameof(accessToken));
        }
    }

    async Task<TResponse> SendQrAuthRequestAsync<TResponse>(
        string method,
        string? accessToken,
        IMessage input,
        MessageParser<TResponse> parser,
        CancellationToken cancellationToken)
        where TResponse : IMessage<TResponse>
    {
        // Never fetch a decoded QR URL. Both methods use this fixed Steam Web API origin.
        // Token transport and X-eresult handling follow node-steam-session WebApiTransport.
        // https://github.com/DoctorMcKay/node-steam-session/blob/master/src/transports/WebApiTransport.ts
        // https://github.com/DoctorMcKay/node-steam-session/blob/master/src/AuthenticationClient.ts
        var encoded = input.ToByteString().ToBase64();
        var get = method == "GetPasswordRSAPublicKey";
        var url = $"https://api.steampowered.com/IAuthenticationService/{method}/v1/";
        if (accessToken is not null)
            url += "?access_token=" + Uri.EscapeDataString(accessToken);
        else if (get)
            url += "?origin=SteamMobile&input_protobuf_encoded=" + Uri.EscapeDataString(encoded);
        using var request = new HttpRequestMessage(get ? HttpMethod.Get : HttpMethod.Post, url);
        if (!get)
        {
            // Steam's form parser requires the field name to be quoted. .NET leaves a
            // token-shaped name unquoted, which makes Steam treat the protobuf as absent.
            var field = new StringContent(encoded);
            field.Headers.ContentType = null;
            field.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "\"input_protobuf_encoded\"",
            };
            var content = new MultipartFormDataContent { field };
            // Match the mobile client's unquoted boundary parameter, retaining its random value.
            var boundary = content.Headers.ContentType!.Parameters.Single(x => x.Name == "boundary");
            boundary.Value = boundary.Value!.Trim('"');
            request.Content = content;
        }
        request.Headers.UserAgent.ParseAdd("okhttp/4.9.2");
        request.Headers.TryAddWithoutValidation("Cookie", "mobileClient=android; mobileClientVersion=777777 3.10.3");
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Steam authentication request failed (HTTP {(int)response.StatusCode}).", null, response.StatusCode);
            // Approval/state-changing responses can be empty even on error, so require EResult.
            // The read-only RSA endpoint can omit it; its key fields are checked before use.
            if (response.Headers.TryGetValues("X-eresult", out var values))
            {
                if (!int.TryParse(values.SingleOrDefault(), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    throw new InvalidOperationException("Steam authentication response is missing a valid EResult.");
                if (result != 1)
                    throw new SteamAuthenticationException(result);
            }
            else if (!get)
                throw new InvalidOperationException("Steam authentication response is missing a valid EResult.");
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return parser.ParseFrom(bytes);
        }
        catch (HttpRequestException ex)
        {
            // Do not propagate the request URI, token or remote error text into application logs.
            throw new HttpRequestException("Steam authentication request failed.", null, ex.StatusCode);
        }
    }

    sealed class SteamAuthenticationException(int result)
        : InvalidOperationException($"Steam authentication request was rejected (EResult {result}).")
    {
        public int Result { get; } = result;
    }
}
