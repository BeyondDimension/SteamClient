#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 登录历史
/// </summary>
public record class LoginHistoryItem
{
    /// <summary>
    /// 登录时间
    /// </summary>
    public string LogInDateTime { get; set; } = string.Empty;

    /// <summary>
    /// 登出时间
    /// </summary>
    public string? LogOutDateTime { get; set; }

    /// <summary>
    /// 系统类型
    /// </summary>
    public int OsType { get; set; }

    /// <summary>
    /// 登录国家或地区
    /// </summary>
    public string CountryOrRegion { get; set; } = string.Empty;

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 州或省
    /// </summary>
    public string State { get; set; } = string.Empty;
}
