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
    Task<string> AddAuthenticatorAsync(SteamSession steamSession, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1");

    /// <summary>
    /// 完成令牌添加
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="activation_code"></param>
    /// <param name="authenticator_code"></param>
    /// <param name="authenticator_time"></param>
    /// <param name="validate_sms_code"></param>
    /// <returns></returns>
    Task<string> FinalizeAddAuthenticatorAsync(SteamSession steamSession, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1");

    /// <summary>
    /// 获取用户国家地区
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<string> GetUserCountry(SteamSession steamSession);

    /// <summary>
    /// 绑定手机
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="phone_number"></param>
    /// <param name="contury_code"></param>
    /// <returns></returns>
    Task<string> AddPhoneNumberAsync(SteamSession steamSession, string phone_number, string? contury_code);

    /// <summary>
    /// 等待邮箱确认
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<string> AccountWaitingForEmailConfirmation(SteamSession steamSession);

    /// <summary>
    /// 发送手机验证码
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<string> SendPhoneVerificationCode(SteamSession steamSession);

    /// <summary>
    /// 移除令牌
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="revocation_code"></param>
    /// <param name="steamguard_scheme"></param>
    /// <param name="revocation_reason"></param>
    /// <returns></returns>
    Task<string> RemoveAuthenticatorAsync(SteamSession steamSession, string? revocation_code, string steamguard_scheme, string revocation_reason = "1");

    /// <summary>
    /// 服务器时间
    /// </summary>
    /// <returns></returns>
    Task<string> TwoFAQueryTime();
}
