namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600
/// <summary>
/// <see cref="AuthorizedDevice"/> 视图模型
/// </summary>
public class AuthorizedDeviceViewModel : ReactiveObject
{
    bool _Disable;

    public bool Disable
    {
        get => _Disable;
        set => this.RaiseAndSetIfChanged(ref _Disable, value);
    }

    /// <summary>
    /// 用户个人资料 Url
    /// </summary>
    public string ProfileUrl => string.Format(SteamApiUrls.STEAM_PROFILES_URL, SteamId64_Int);

    public bool First { get; set; }

    public bool End { get; set; }

    public int Index { get; set; }

    public long SteamId3_Int { get; set; }

    public long SteamId64_Int => SteamIdConvert.UndefinedId + SteamId3_Int;

    string? _OnlineState;

    /// <summary>
    /// 在线状态
    /// </summary>
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

    string? _SteamID;

    public string? SteamID
    {
        get => _SteamID;
        set => this.RaiseAndSetIfChanged(ref _SteamID, value);
    }

    /// <summary>
    /// 展示名称
    /// </summary>
    public string? ShowName { get; set; }

    /// <summary>
    /// 用户小型简介
    /// </summary>
    public SteamMiniProfile? MiniProfile { get; set; }

    string? _SteamNickName;

    /// <summary>
    /// 昵称
    /// </summary>
    public string? SteamNickName
    {
        get => _SteamNickName;
        set => this.RaiseAndSetIfChanged(ref _SteamNickName, value);
    }

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

    /// <summary>
    /// <see cref="AuthorizedDevice"/> 隐式转换 <see cref="AuthorizedDeviceViewModel"/>
    /// </summary>
    /// <param name="authorizedDevice"></param>
    public static implicit operator AuthorizedDeviceViewModel(AuthorizedDevice authorizedDevice) => new(authorizedDevice);

    /// <summary>
    /// <see cref="AuthorizedDevice"/> 构造实例 <see cref="AuthorizedDeviceViewModel"/>
    /// </summary>
    /// <param name="authorizedDevice"></param>
    public AuthorizedDeviceViewModel(AuthorizedDevice authorizedDevice)
    {
        Disable = authorizedDevice.Disable;
        First = authorizedDevice.First;
        End = authorizedDevice.End;
        Index = authorizedDevice.Index;
        SteamId3_Int = authorizedDevice.SteamId3_Int;
        OnlineState = authorizedDevice.OnlineState;
        Remark = authorizedDevice.Remark;
        SteamID = authorizedDevice.SteamID;
        ShowName = authorizedDevice.ShowName;
        MiniProfile = authorizedDevice.MiniProfile;
        SteamNickName = authorizedDevice.SteamNickName;
        AccountName = authorizedDevice.AccountName;
        Timeused = authorizedDevice.Timeused;
        Description = authorizedDevice.Description;
        Tokenid = authorizedDevice.Tokenid;
        AvatarIcon = authorizedDevice.AvatarIcon;
        AvatarMedium = authorizedDevice.AvatarMedium;
    }
}
