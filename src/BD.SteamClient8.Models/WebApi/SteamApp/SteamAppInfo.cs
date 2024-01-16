#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamApp"/> Detail
/// </summary>
public class SteamAppInfo
{
    public string? ClientIcon { get; set; }

    public string? ClientTga { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 支持语言
    /// </summary>
    public Dictionary<string, short>? Languages { get; set; }

    /// <summary>
    /// 图片 Logo
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// 图片 Logo (小)
    /// </summary>
    public string? Logo_Small { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 系统列表
    /// </summary>
    public string? OsList { get; set; }

    /// <summary>
    /// App 类型
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 评测机构名称
    /// </summary>
    public string? Metacritic_Name { get; set; }

    /// <summary>
    /// 压缩图片列表
    /// </summary>
    public Dictionary<string, string>? Small_Capsule { get; set; }

    /// <summary>
    /// 图片资源
    /// </summary>
    public Dictionary<string, string>? Header_Image { get; set; }
}