#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

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
