namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 小型用户个人资料游戏
/// </summary>
[MP2Obj]
public partial record class SteamMiniProfileGame
{
    /// <summary>
    /// 游戏名称
    /// </summary>
    [NewtonsoftJsonProperty("name")]
    [SystemTextJsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 非 Steam 游戏
    /// </summary>
    [NewtonsoftJsonProperty("is_non_steam")]
    [SystemTextJsonProperty("is_non_steam")]
    public bool IsNonSteam { get; set; }

    /// <summary>
    /// 游戏LOGO
    /// </summary>
    [NewtonsoftJsonProperty("logo")]
    [SystemTextJsonProperty("logo")]
    public string? Logo { get; set; }
}
