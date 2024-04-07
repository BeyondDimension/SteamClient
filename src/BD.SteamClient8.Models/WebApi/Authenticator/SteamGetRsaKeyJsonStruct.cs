namespace BD.SteamClient8.Models;

/// <summary>
/// SteamGetRsaKey 接口返回类型
/// </summary>
public sealed class SteamGetRsaKeyJsonStruct
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 公钥 Mod
    /// </summary>
    [SystemTextJsonProperty("publickey_mod")]
    public string PublicKeyMod { get; set; } = string.Empty;

    /// <summary>
    /// 公钥 Exp
    /// </summary>
    [SystemTextJsonProperty("publickey_exp")]
    public string PublicKeyExp { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    [SystemTextJsonProperty("timestamp")]
    public string TimeStamp { get; set; } = string.Empty;

    /// <summary>
    /// Token Id
    /// </summary>
    [SystemTextJsonProperty("token_gid")]
    public string TokenGId { get; set; } = string.Empty;
}
