namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 小型用户个人资料徽章
/// </summary>
[MP2Obj]
public partial record class SteamMiniProfileBadge
{
    /// <summary>
    /// 徽章名称
    /// </summary>
    [NewtonsoftJsonProperty("name")]
    [SystemTextJsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    [NewtonsoftJsonProperty("xp")]
    [SystemTextJsonProperty("xp")]
    public string? Xp { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [NewtonsoftJsonProperty("level")]
    [SystemTextJsonProperty("level")]
    public int Level { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [NewtonsoftJsonProperty("description")]
    [SystemTextJsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 徽章图标
    /// </summary>
    [NewtonsoftJsonProperty("icon")]
    [SystemTextJsonProperty("icon")]
    public string? Icon { get; set; }
}
