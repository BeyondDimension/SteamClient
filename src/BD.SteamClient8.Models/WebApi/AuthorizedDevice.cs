namespace BD.SteamClient8.Models.WebApi;

#pragma warning disable SA1600 // Elements should be documented
public record class AuthorizedDevice
{
    public AuthorizedDevice() { }

    public bool Disable { get; set; }

    /// <summary>
    /// 用户个人资料 Url
    /// </summary>
    public string ProfileUrl => string.Format(SteamApiUrls.STEAM_PROFILES_URL, SteamId64_Int);

    public bool First { get; set; }

    public bool End { get; set; }

    public int Index { get; set; }

    public long SteamId3_Int { get; set; }

    public long SteamId64_Int => SteamIdConvert.UndefinedId + SteamId3_Int;

    /// <summary>
    /// 在线状态
    /// </summary>
    public string? OnlineState { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    public string? SteamID { get; set; }

    /// <summary>
    /// 展示名称
    /// </summary>
    public string? ShowName { get; set; }

    /// <summary>
    /// 用户小型简介
    /// </summary>
    public SteamMiniProfile? MiniProfile { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? SteamNickName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? AccountName { get; set; }

    public long Timeused { get; set; }

    public DateTime TimeusedTime => Timeused.ToDateTimeS();

    public string? Description { get; set; }

    public string? Tokenid { get; set; }

    public string? AvatarIcon { get; set; }

    public string? AvatarMedium { get; set; }
}
