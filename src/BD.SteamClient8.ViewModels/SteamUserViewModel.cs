namespace BD.SteamClient8.ViewModels;

/// <summary>
/// <see cref="SteamUser"/> 模型绑定类型
/// </summary>
[XmlRoot("profile")]
public class SteamUserViewModel : ReactiveObject
{
    /// <summary>
    /// SteamId 3 格式
    /// </summary>
    [XmlIgnore]
    public string? SteamId3 => $"[U:1:{SteamId32}]";

    /// <summary>
    /// SteamId 32位
    /// </summary>
    [XmlIgnore]
    public int SteamId32 => Convert.ToInt32((SteamId64 >> 0) & 0xFFFFFFFF);

    /// <summary>
    /// SteamId 64位
    /// </summary>
    [XmlElement("steamID64")]
    public long SteamId64 { get; set; }

    /// <summary>
    /// 个人资料链接
    /// </summary>
    [XmlIgnore]
    public string ProfileUrl => string.Format(SteamApiUrls.STEAM_PROFILES_URL, SteamId64);

    /// <summary>
    /// Userdata文件夹相对路径
    /// </summary>
    [XmlIgnore]
    public string UserdataPath => Path.Combine("userdata", SteamId32.ToString());

    private string? _OnlineState;

    /// <summary>
    /// 在线状态
    /// </summary>
    [XmlElement("onlineState")]
    public string? OnlineState
    {
        get => _OnlineState;
        set => this.RaiseAndSetIfChanged(ref _OnlineState, value);
    }

    int? _Level;

    /// <summary>
    /// Steam等级
    /// </summary>
    [XmlIgnore]
    public int? Level
    {
        get => _Level;
        set => this.RaiseAndSetIfChanged(ref _Level, value);
    }

    /// <summary>
    /// IP 国家地区
    /// </summary>
    [XmlIgnore]
    public string? IPCountry { get; set; }

    /// <summary>
    /// 公开状态
    /// friendsonly
    /// public
    /// </summary>
    [XmlElement("privacyState")]
    public string? PrivacyState { get; set; }

    string? _AvatarIcon;

    /// <summary>
    /// 头像图标
    /// </summary>
    [XmlElement("avatarIcon")]
    public string? AvatarIcon
    {
        get => _AvatarIcon;
        set => this.RaiseAndSetIfChanged(ref _AvatarIcon, value);
    }

    string? _AvatarMedium;

    /// <summary>
    /// 中等大小头像
    /// </summary>
    [XmlElement("avatarMedium")]
    public string? AvatarMedium
    {
        get => _AvatarMedium;
        set => this.RaiseAndSetIfChanged(ref _AvatarMedium, value);
    }

    string? _AvatarFull;

    /// <summary>
    /// 完整头像
    /// </summary>
    [XmlElement("avatarFull")]
    public string? AvatarFull
    {
        get => _AvatarFull;
        set => this.RaiseAndSetIfChanged(ref _AvatarFull, value);
    }

    /// <summary>
    /// 注册日期
    /// </summary>
    [XmlElement("memberSince")]
    public string? MemberSince { get; set; }

    /// <summary>
    /// VAC
    /// </summary>
    [XmlElement("vacBanned")]
    public bool VacBanned { get; set; }

    /// <summary>
    /// 自我介绍HTML
    /// </summary>
    [XmlElement("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [XmlElement("steamID")]
    public string? SteamID { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [XmlIgnore]
    public string? SteamNickName => string.IsNullOrEmpty(SteamID) ? PersonaName : SteamID;

    /// <summary>
    /// 从 Valve Data File 读取到的用户名
    /// </summary>
    [XmlIgnore]
    public string? PersonaName { get; set; }

    /// <summary>
    /// 从 Valve Data File 读取到的AllowAutoLogin
    /// </summary>
    [XmlIgnore]
    public bool AllowAutoLogin { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// 是否记住密码
    /// </summary>
    [XmlIgnore]
    public bool RememberPassword { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [XmlIgnore]
    public string? PassWord { get; set; }

    /// <summary>
    /// 最后登录时间戳
    /// </summary>
    [XmlIgnore]
    public long Timestamp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    [XmlIgnore]
    public DateTime LastLoginTime { get; set; }

    bool _MostRecent;

    /// <summary>
    /// 最近登录
    /// </summary>
    [XmlIgnore]
    public bool MostRecent
    {
        get => _MostRecent;
        set => this.RaiseAndSetIfChanged(ref _MostRecent, value);
    }

    bool _WantsOfflineMode;

    /// <summary>
    /// 离线模式
    /// </summary>
    [XmlIgnore]
    public bool WantsOfflineMode
    {
        get => _WantsOfflineMode;
        set => this.RaiseAndSetIfChanged(ref _WantsOfflineMode, value);
    }

    /// <summary>
    /// 忽略离线模式警告弹窗
    /// </summary>
    [XmlIgnore]
    public bool SkipOfflineModeWarning { get; set; }

    string? _Remark;

    /// <summary>
    /// 备注
    /// </summary>
    [XmlIgnore]
    public string? Remark
    {
        get => _Remark;
        set => this.RaiseAndSetIfChanged(ref _Remark, value);
    }

    SteamMiniProfile? _MiniProfile;

    /// <summary>
    /// Steam 小型用户个人资料
    /// </summary>
    public SteamMiniProfile? MiniProfile
    {
        get => _MiniProfile;
        set => this.RaiseAndSetIfChanged(ref _MiniProfile, value);
    }

    PersonaState _PersonaState = PersonaState.Default;

    /// <summary>
    /// 离线模式
    /// </summary>
    [XmlIgnore]
    public PersonaState PersonaState
    {
        get => _PersonaState;
        set => this.RaiseAndSetIfChanged(ref _PersonaState, value);
    }

    /// <summary>
    /// <see cref="SteamUser"/> 隐式转换 <see cref="SteamUserViewModel"/>
    /// </summary>
    /// <param name="steamUser"></param>
    public static implicit operator SteamUserViewModel(SteamUser steamUser) => new(steamUser);

    /// <summary>
    /// <see cref="SteamUser"/> 构造实例 <see cref="SteamUserViewModel"/>
    /// </summary>
    /// <param name="steamUser"></param>
    public SteamUserViewModel(SteamUser steamUser)
    {
        SteamId64 = steamUser.SteamId64;
        OnlineState = steamUser.OnlineState;
        Level = steamUser.Level;
        IPCountry = steamUser.IPCountry;
        PrivacyState = steamUser.PrivacyState;
        AvatarIcon = steamUser.AvatarIcon;
        AvatarMedium = steamUser.AvatarMedium;
        AvatarFull = steamUser.AvatarFull;
        MemberSince = steamUser.MemberSince;
        VacBanned = steamUser.VacBanned;
        Summary = steamUser.Summary;
        SteamID = steamUser.SteamID;
        PersonaName = steamUser.PersonaName;
        AllowAutoLogin = steamUser.AllowAutoLogin;
        AccountName = steamUser.AccountName;
        RememberPassword = steamUser.RememberPassword;
        PassWord = steamUser.PassWord;
        Timestamp = steamUser.Timestamp;
        LastLoginTime = steamUser.LastLoginTime;
        MostRecent = steamUser.MostRecent;
        WantsOfflineMode = steamUser.WantsOfflineMode;
        SkipOfflineModeWarning = steamUser.SkipOfflineModeWarning;
        Remark = steamUser.Remark;
        MiniProfile = steamUser.MiniProfile;
        PersonaState = steamUser.PersonaState;
    }
}
