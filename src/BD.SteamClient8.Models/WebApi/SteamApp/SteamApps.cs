#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
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