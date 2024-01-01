#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// GetUserCountry 接口返回模型类
/// </summary>
public class GetUserCountryResponse
{
    /// <summary>
    /// <see cref="GetUserCountryResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public GetUserCountryResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="GetUserCountryResponse.Response"/> 详细信息
/// </summary>
public class GetUserCountryResponseResponse
{
    /// <summary>
    /// 国家或地区
    /// </summary>
    [SystemTextJsonProperty("country")]
    public string? Country { get; set; }
}