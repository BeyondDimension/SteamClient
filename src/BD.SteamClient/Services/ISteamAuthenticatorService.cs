namespace BD.SteamClient.Services;

public interface ISteamAuthenticatorService
{
    /// <summary>
    /// 添加令牌
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="authenticator_time"></param>
    /// <param name="device_identifier"></param>
    /// <param name="authenticator_type"></param>
    /// <param name="sms_phone_id"></param>
    /// <returns></returns>
    Task<string> AddAuthenticatorAsync(string steam_id, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1");

    /// <summary>
    /// 完成令牌添加
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="activation_code"></param>
    /// <param name="authenticator_code"></param>
    /// <param name="authenticator_time"></param>
    /// <param name="validate_sms_code"></param>
    /// <returns></returns>
    Task<string> FinalizeAddAuthenticatorAsync(string steam_id, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1");

    /// <summary>
    /// 获取用户国家地区
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<string> GetUserCountry(string steam_id);

    /// <summary>
    /// 绑定手机
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="phone_number"></param>
    /// <param name="contury_code"></param>
    /// <returns></returns>
    Task<string> AddPhoneNumberAsync(string steam_id, string phone_number, string? contury_code);

    /// <summary>
    /// 等待邮箱确认
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<string> AccountWaitingForEmailConfirmation(string steam_id);

    /// <summary>
    /// 发送手机验证码
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<string> SendPhoneVerificationCode(string steam_id);

    /// <summary>
    /// 移除令牌
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="revocation_code"></param>
    /// <param name="steamguard_scheme"></param>
    /// <param name="revocation_reason"></param>
    /// <returns></returns>
    Task<string> RemoveAuthenticatorAsync(string steam_id, string? revocation_code, string steamguard_scheme, string revocation_reason = "1");

    Task<CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response?> RemoveAuthenticatorViaChallengeStartSync(string steam_id);

    Task<CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response?> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true);

    /// <summary>
    /// 服务器时间
    /// </summary>
    /// <returns></returns>
    Task<string> TwoFAQueryTime();

    /// <summary>
    /// 刷新AccessToken
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task<string> RefreshAccessToken(string steam_id);
}
