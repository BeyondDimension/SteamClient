namespace BD.SteamClient8.Models.WebApi.Profile;

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
