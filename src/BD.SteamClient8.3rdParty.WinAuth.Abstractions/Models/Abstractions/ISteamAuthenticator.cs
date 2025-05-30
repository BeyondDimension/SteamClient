using System.Net;

namespace BD.SteamClient8.WinAuth.Models.Abstractions;

public interface ISteamAuthenticator : IAuthenticatorValueModelBase
{
    /// <summary>
    /// 如果网络错误，忽略同步的分钟数
    /// </summary>
    const int SYNC_ERROR_MINUTES = 60;

    /// <summary>
    /// 尝试激活的次数
    /// </summary>
    const int ENROLL_ACTIVATE_RETRIES = 30;

    /// <summary>
    /// 激活码不正确
    /// </summary>
    const int INVALID_ACTIVATION_CODE = 89;

    /// <summary>
    /// 返回验证器的序列号
    /// </summary>
    string? Serial { get; set; }

    /// <summary>
    /// 创建并注册的随机设备 ID
    /// </summary>
    string? DeviceId { get; set; }

    /// <summary>
    /// Steam Json 数据
    /// </summary>
    string? SteamData { get; set; }

    /// <summary>
    /// 撤销代码
    /// </summary>
    string? RecoveryCode { get; }

    /// <summary>
    /// Steam Json 数据模型类实例
    /// </summary>
    ISteamConvertSteamDataJsonStruct? SteamDataObj { get; }

    /// <summary>
    /// 计算验证器的当前代码
    /// </summary>
    /// <param name="resync"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    string CalculateCode(bool resync = false, long interval = -1);

    /// <summary>
    /// 注册状态
    /// </summary>
    interface IEnrollState
    {
        /// <summary>
        /// 语言
        /// </summary>
        string? Language { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        string? Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        string? Password { get; set; }

        /// <summary>
        /// 电子邮件域
        /// </summary>
        public string? EmailDomain { get; set; }

        /// <summary>
        /// 电子邮件认证文本
        /// </summary>
        public string? EmailAuthText { get; set; }

        /// <summary>
        /// 激活代码
        /// </summary>
        public string? ActivationCode { get; set; }

        /// <summary>
        /// Cookies
        /// </summary>
        CookieContainer? Cookies { get; set; }

        /// <summary>
        /// SteamId
        /// </summary>
        long SteamId { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        string? PhoneNumber { get; set; }

        /// <summary>
        /// 电话号码是否存在
        /// </summary>
        bool NoPhoneNumber { get; set; }

        /// <summary>
        /// 是否更换身份验证方式
        /// </summary>
        bool ReplaceAuth { get; set; }

        /// <summary>
        /// 需要邮箱确认电话
        /// </summary>
        bool RequiresEmailConfirmPhone { get; set; }

        /// <summary>
        /// 需要激活
        /// </summary>
        bool RequiresActivation { get; set; }

        /// <summary>
        /// 撤销代码
        /// </summary>
        string? RevocationCode { get; set; }

        /// <summary>
        /// 秘密密钥
        /// </summary>
        string? SecretKey { get; set; }

        /// <summary>
        /// Success
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        string? Error { get; set; }

        /// <summary>
        /// 访问令牌
        /// </summary>
        string? AccessToken { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        string? RefreshToken { get; set; }
    }

    /// <summary>
    /// 为注册创建一个随机的设备 Id 字符串
    /// </summary>
    static string BuildRandomId(Guid? uuid = null) => "android:" + (uuid ?? Guid.NewGuid()).ToString();
}
