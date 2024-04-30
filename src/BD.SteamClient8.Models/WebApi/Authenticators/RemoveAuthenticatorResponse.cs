namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// RemoveAuthenticatorAsync 接口返回模型类
/// </summary>
public class RemoveAuthenticatorResponse
{
    /// <summary>
    /// <see cref="RemoveAuthenticatorResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public RemoveAuthenticatorResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="RemoveAuthenticatorResponse.Response"/> 模型类
/// </summary>
public class RemoveAuthenticatorResponseResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 剩余尝试次数
    /// </summary>
    [SystemTextJsonProperty("revocation_attempts_remaining")]
    public int RevocationAttemptsRemaining { get; set; }
}