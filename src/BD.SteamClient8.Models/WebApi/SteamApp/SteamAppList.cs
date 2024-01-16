#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamApp"/> Collection Model
/// </summary>
public class SteamAppList
{
    /// <summary>
    /// <see cref="SteamApp"/> Collection
    /// </summary>
    [SystemTextJsonProperty("apps")]
    public List<SteamApp>? Apps { get; set; }
}