#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamAppList"/> Model
/// </summary>
public class SteamApps
{
    /// <summary>
    /// <see cref="SteamAppList"/> Instance
    /// </summary>
    [SystemTextJsonProperty("applist")]
    public SteamAppList? AppList { get; set; }
}