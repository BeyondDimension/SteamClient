namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// Steam 令牌服务
/// </summary>
public interface ISteamAuthenticatorService
{
    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamAuthenticatorService Instance => Ioc.Get<ISteamAuthenticatorService>();

    /// <summary>
    /// 添加令牌
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="authenticator_time"></param>
    /// <param name="device_identifier"></param>
    /// <param name="authenticator_type"></param>
    /// <param name="sms_phone_id"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamDoLoginTfaJsonStruct?>> AddAuthenticatorAsync(string steam_id, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1");

    /// <summary>
    /// 完成令牌添加
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="activation_code"></param>
    /// <param name="authenticator_code"></param>
    /// <param name="authenticator_time"></param>
    /// <param name="validate_sms_code"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamDoLoginFinalizeJsonStruct?>> FinalizeAddAuthenticatorAsync(string steam_id, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1");

    /// <summary>
    /// 获取用户国家地区
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task<ApiRspImpl<GetUserCountryResponse?>> GetUserCountry(string steam_id);

    /// <summary>
    /// 绑定手机
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="phone_number"></param>
    /// <param name="contury_code"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamAddPhoneNumberResponse?>> AddPhoneNumberAsync(string steam_id, string phone_number, string? contury_code);

    /// <summary>
    /// 等待邮箱确认
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task<ApiRspImpl<IsAccountWaitingForEmailConfirmationResponse?>> AccountWaitingForEmailConfirmation(string steam_id);

    /// <summary>
    /// 发送手机验证码
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> SendPhoneVerificationCode(string steam_id);

    /// <summary>
    /// 移除令牌
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="revocation_code"></param>
    /// <param name="steamguard_scheme"></param>
    /// <param name="revocation_reason"></param>
    /// <returns></returns>
    Task<ApiRspImpl<RemoveAuthenticatorResponse?>> RemoveAuthenticatorAsync(string steam_id, string? revocation_code, string steamguard_scheme, string revocation_reason = "1");

    /// <summary>
    /// 申请 Steam 替换安全防护令牌验证码
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task<ApiRspImpl<CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response?>> RemoveAuthenticatorViaChallengeStartSync(string steam_id);

    /// <summary>
    /// Steam 替换安全防护令牌
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="sms_code"></param>
    /// <param name="generate_new_token"></param>
    /// <returns></returns>
    Task<ApiRspImpl<CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response?>> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true);

    /// <summary>
    /// 服务器时间
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<SteamSyncStruct?>> TwoFAQueryTime();

    /// <summary>
    /// 刷新AccessToken
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    Task<ApiRspImpl<string>> RefreshAccessToken(string steam_id);
}
