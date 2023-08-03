namespace BD.SteamClient.Models;

public class AuthorizedDevice : ReactiveObject
{
    public AuthorizedDevice() { }

    bool _Disable;

    public bool Disable
    {
        get => _Disable;
        set => this.RaiseAndSetIfChanged(ref _Disable, value);
    }

    public string ProfileUrl => string.Format(STEAM_PROFILES_URL, SteamId64_Int);

    public bool First { get; set; }

    public bool End { get; set; }

    public int Index { get; set; }

    public long SteamId3_Int { get; set; }

    public long SteamId64_Int => SteamIdConvert.UndefinedId + SteamId3_Int;

    string? _OnlineState;

    public string? OnlineState
    {
        get => _OnlineState;
        set => this.RaiseAndSetIfChanged(ref _OnlineState, value);
    }

    string? _Remark;

    /// <summary>
    /// 备注
    /// </summary> 
    public string? Remark
    {
        get => _Remark;
        set => this.RaiseAndSetIfChanged(ref _Remark, value);
    }

    public string? SteamID { get; set; }

    public string? ShowName { get; set; }

    public SteamMiniProfile? MiniProfile { get; set; }

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

    string? _AvatarMedium;

    public string? AvatarMedium
    {
        get => _AvatarMedium;
        set => this.RaiseAndSetIfChanged(ref _AvatarMedium, value);
    }
}
