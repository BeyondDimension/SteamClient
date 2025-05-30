using BD.Common8.Helpers;
using BD.Common8.Http.ClientFactory.Models;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.Protobuf;
using BD.SteamClient8.Models.WebApi.Authenticators;
using BD.SteamClient8.Models.WebApi.Authenticators.PhoneNumber;
using BD.SteamClient8.Services.Abstractions.WebApi;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Extensions;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamAuthenticatorService"/> Steam 令牌服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamAuthenticatorServiceImpl"/> 类的新实例
/// </remarks>
public sealed class SteamAuthenticatorServiceImpl(
    ISteamSessionService sessions,
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider) : WebApiClientFactoryService(
        loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamAuthenticatorService
{
    /// <inheritdoc/>
    protected sealed override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override JsonSerializerOptions GetJsonSerializerOptions()
    {
        var o = DefaultJsonSerializerContext_.Default.Options;
        return o;
    }

    /// <summary>
    /// 用于标识和记录日志信息
    /// </summary>
    public const string TAG = "SteamAuthenticatorWebApiS";

    /// <inheritdoc/>
    public async Task<SteamDoLoginTfaJsonStruct?> AddAuthenticatorAsync(string steam_id, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1", CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var data = new Dictionary<string, string>
        {
            { "steamid", steamSession.SteamId },
            { "authenticator_time", authenticator_time },
            { "authenticator_type", authenticator_type },
            { "device_identifier", device_identifier ?? "" },
            { "sms_phone_id", sms_phone_id },
        };

        using var sendArgs = new WebApiClientSendArgs(string.Format(SteamApiUrls.STEAM_AUTHENTICATOR_ADD, steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<SteamDoLoginTfaJsonStruct, Dictionary<string, string>>(sendArgs, data, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<SteamDoLoginFinalizeJsonStruct?> FinalizeAddAuthenticatorAsync(string steam_id, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1", CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var data = new Dictionary<string, string>
        {
            { "steamid", steamSession.SteamId },
            { "activation_code", activation_code ?? "" },
            { "validate_sms_code", validate_sms_code },
            { "authenticator_code", authenticator_code },
            { "authenticator_time", authenticator_time },
        };
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_FINALIZEADD.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<SteamDoLoginFinalizeJsonStruct, Dictionary<string, string>>(sendArgs, data, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<GetUserCountryOrRegionResponse?> GetUserCountryOrRegion(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var param = new Dictionary<string, string>
        {
            { "steamid", steamSession.SteamId },
        };
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_GET_USERCOUNTRY.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<GetUserCountryOrRegionResponse, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<SteamAddPhoneNumberResponse?> AddPhoneNumberAsync(string steam_id, string phone_number, string? contury_code, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var data = new Dictionary<string, string>
        {
            { "phone_number", phone_number },
            { "phone_country_code", contury_code ?? "" },
        };

        using var sendArgs = new WebApiClientSendArgs(string.Format(SteamApiUrls.STEAM_AUTHENTICATOR_ADD_PHONENUMBER, steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<SteamAddPhoneNumberResponse, Dictionary<string, string>>(sendArgs, data, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyPhoneNumberAsync(string steam_id, string phone_number, string? sms_code, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var data = new Dictionary<string, string>
        {
            { "code", sms_code ?? string.Empty }
        };

        using var sendArgs = new WebApiClientSendArgs(string.Format(SteamApiUrls.STEAM_AUTHENTICATOR_VERIFY_PHONENUMBER, steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        using var resp = await SendAsync<Stream, Dictionary<string, string>>(sendArgs, data, cancellationToken);
        var bSize = 4096;
        var b = ArrayPool<byte>.Shared.Rent(bSize);
        try
        {
            var i = await resp.ThrowIsNull().ReadAsync(b, cancellationToken);
            if (i > 0)
            {
                var str = Encoding.UTF8.GetString(b.AsSpan(0, i));
                return !string.IsNullOrWhiteSpace(str);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(b);
        }
        return false;

        //try
        //{
        //    var resp = await SendAsync<string, Dictionary<string, string>>(sendArgs, data, cancellationToken);

        //    return ApiRspHelper.Ok<bool?>(!string.IsNullOrWhiteSpace(resp));
        //}
        //catch (Exception ex)
        //{
        //    logger.LogError(ex, "verify phone number error");
        //    return ApiRspHelper.Fail<bool?>(null);
        //}
    }

    /// <inheritdoc/>
    public async Task<IsAccountWaitingForEmailConfirmationResponse?> AccountWaitingForEmailConfirmation(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_ACCOUNTWAITINGFOREMAILCONF.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<IsAccountWaitingForEmailConfirmationResponse>(sendArgs, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> SendPhoneVerificationCode(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_SEND_PHONEVERIFICATIONCODE.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        return result.ThrowIsNull().IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<RemoveAuthenticatorResponse?> RemoveAuthenticatorAsync(string steam_id, string? revocation_code, string steamguard_scheme, string revocation_reason = "1", CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var param = new Dictionary<string, string>
        {
            { "revocation_code", revocation_code ??
#pragma warning disable CA2208 // 正确实例化参数异常
                throw new ArgumentNullException("恢复代码 revocation_code 不能为 null") },
#pragma warning restore CA2208 // 正确实例化参数异常
            { "revocation_reason", revocation_reason },
            { "steamguard_scheme", steamguard_scheme },
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REMOVE.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var result = await SendAsync<RemoveAuthenticatorResponse, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response?> RemoveAuthenticatorViaChallengeStartSync(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var base64string = UrlEncoder.Default.Encode(new CTwoFactor_RemoveAuthenticatorViaChallengeStart_Request().ToByteString().ToBase64());
        var data = new Dictionary<string, string>()
        {
            { "input_protobuf_encoded", base64string },
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGESTARTSYNC.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<Stream, Dictionary<string, string>>(sendArgs, data, cancellationToken);
        var result = CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response.Parser.ParseFrom(response);
        return result;
    }

    /// <inheritdoc/>
    public async Task<CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response?> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var base64string = UrlEncoder.Default.Encode(new CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Request
        {
            SmsCode = sms_code,
            GenerateNewToken = generate_new_token,
        }.ToByteString().ToBase64());

        var data = new Dictionary<string, string>()
        {
            { "input_protobuf_encoded", base64string },
        };

        using var sendArgs = new WebApiClientSendArgs(string.Format(SteamApiUrls.STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGECONTINUESYNC, steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<Stream, Dictionary<string, string>>(sendArgs, data, cancellationToken);
        var result = CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response.Parser.ParseFrom(response);
        return result;
    }

    static bool IsTokenExpired(string token)
    {
        var tokenComponents = token.Split('.');
        // Fix up base64url to normal base64
        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0)
        {
            base64 += new string('=', 4 - (base64.Length % 4));
        }

        var payloadBytes = Convert.FromBase64String(base64);
        var jwt = JsonSerializer.Deserialize(payloadBytes,
            SteamAuthenticatorServiceImpl_SteamAccessToken_JsonSerializerContext_.Default.SteamAccessToken);

        // Compare expire time of the token to the current time
        return jwt is not null && DateTimeOffset.UtcNow.ToUnixTimeSeconds() > jwt.Exp;
    }

    /// <inheritdoc/>
    public async Task<string?> RefreshAccessToken(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        if (string.IsNullOrEmpty(steamSession.RefreshToken))
            throw new ApplicationException("Refresh token is empty");

        if (IsTokenExpired(steamSession.RefreshToken))
            throw new ApplicationException("Refresh token is expired");

        GenerateAccessTokenForAppResponse? response;
        try
        {
            var postData = new Dictionary<string, string>
            {
                { "refresh_token", steamSession.RefreshToken },
                { "steamid", steamSession.SteamId.ToString() },
            };

            using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REFRESHACCESSTOKEN)
            {
                Method = HttpMethod.Post,
                ContentType = MediaTypeNames.FormUrlEncoded,
            };
            sendArgs.SetHttpClient(steamSession.HttpClient!);

            response = await SendAsync<GenerateAccessTokenForAppResponse, Dictionary<string, string>>(sendArgs, postData, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to refresh token: " + ex.Message, ex);
        }

        var accessToken = response.ThrowIsNull()?.Response?.AccessToken ?? string.Empty;
        if (string.IsNullOrEmpty(accessToken))
        {
            steamSession.AccessToken = accessToken;
            await sessions.AddOrSetSession(steamSession, cancellationToken);
        }
        return accessToken;
    }

    /// <inheritdoc/>
    public async Task<SteamSyncStruct?> TwoFAQueryTime(CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_TWOFAQUERYTIME) { Method = HttpMethod.Post };
        sendArgs.SetHttpClient(CreateClient());
        var result = await SendAsync<SteamSyncStruct>(sendArgs, cancellationToken);
        return result;
    }
}

sealed record class SteamAccessToken
{
    [JsonPropertyName("exp")]
    public long Exp { get; set; }
}

[JsonSerializable(typeof(SteamAccessToken))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SteamAuthenticatorServiceImpl_SteamAccessToken_JsonSerializerContext_ : JsonSerializerContext
{
    static SteamAuthenticatorServiceImpl_SteamAccessToken_JsonSerializerContext_()
    {
        // https://github.com/dotnet/runtime/issues/94135
        s_defaultOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,
        };
        Default = new SteamAuthenticatorServiceImpl_SteamAccessToken_JsonSerializerContext_(new JsonSerializerOptions(s_defaultOptions));
    }
}