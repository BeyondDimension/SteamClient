namespace BD.SteamClient8.Services.PInvoke;

/// <summary>
/// Steam 相关助手、工具类服务
/// </summary>
public partial interface ISteamService
{
    static ISteamService Instance => Ioc.Get<ISteamService>();
}
