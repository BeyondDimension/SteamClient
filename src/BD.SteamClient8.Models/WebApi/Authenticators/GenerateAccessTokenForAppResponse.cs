namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// 刷新 AccessToken 接口返回模型类
/// </summary>
public class GenerateAccessTokenForAppResponse
{
    /// <summary>
    /// <see cref="GenerateAccessTokenForAppResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public GenerateAccessTokenForAppResponseResponse? Response;
}

/// <summary>
/// <see cref="GenerateAccessTokenForAppResponse.Response"/> 模型类
/// </summary>
public class GenerateAccessTokenForAppResponseResponse
{
    /// <summary>
    /// JWT AccessToken
    /// </summary>
    [SystemTextJsonProperty("access_token")]
    public string? AccessToken { get; set; }
}
