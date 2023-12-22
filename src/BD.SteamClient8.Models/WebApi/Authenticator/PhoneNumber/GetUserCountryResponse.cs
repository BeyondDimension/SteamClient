namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

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