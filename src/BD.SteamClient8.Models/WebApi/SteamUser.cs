#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

[XmlRoot("profile")]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public record class SteamUser
{
    /// <summary>
    /// SteamId 3 格式
    /// </summary>
    [XmlIgnore]
    public string? SteamId3 => $"[U:1:{SteamId32}]";

    /// <summary>
    /// SteamId 32 位
    /// </summary>
    [XmlIgnore]
    public int SteamId32 => Convert.ToInt32((SteamId64 >> 0) & 0xFFFFFFFF);

    /// <summary>
    /// SteamId 64 位
    /// </summary>
    [XmlElement("steamID64")]
    public long SteamId64 { get; set; }

    /// <inheritdoc cref="DebuggerDisplayAttribute"/>
    public string DebuggerDisplay() => $"{SteamNickName}, {SteamId64}";

    /// <summary>
    /// 个人资料链接
    /// </summary>
    [XmlIgnore]
    public string ProfileUrl => string.Format(SteamApiUrls.STEAM_PROFILES_URL, SteamId64);

    /// <summary>
    /// Userdata 文件夹相对路径
    /// </summary>
    [XmlIgnore]
    public string UserdataPath => Path.Combine("userdata", SteamId32.ToString());

    /// <summary>
    /// 在线状态
    /// </summary>
    [XmlElement("onlineState")]
    public string? OnlineState { get; set; }

    /// <summary>
    /// Steam 等级
    /// </summary>
    [XmlIgnore]
    public int? Level { get; set; }

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

    /// <summary>
    /// 头像图标
    /// </summary>
    [XmlElement("avatarIcon")]
    public string? AvatarIcon { get; set; }

    /// <summary>
    /// 中等大小头像
    /// </summary>
    [XmlElement("avatarMedium")]
    public string? AvatarMedium { get; set; }

    /// <summary>
    /// 完整头像
    /// </summary>
    [XmlElement("avatarFull")]
    public string? AvatarFull { get; set; }

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
    /// 自我介绍 HTML
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
    /// 从 Valve Data File 读取到的 AllowAutoLogin
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

    /// <summary>
    /// 最近登录
    /// </summary>
    [XmlIgnore]
    public bool MostRecent { get; set; }

    /// <summary>
    /// 离线模式
    /// </summary>
    [XmlIgnore]
    public bool WantsOfflineMode { get; set; }

    /// <summary>
    /// 忽略离线模式警告弹窗
    /// </summary>
    [XmlIgnore]
    public bool SkipOfflineModeWarning { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [XmlIgnore]
    public string? Remark { get; set; }

    /// <summary>
    /// Steam 用户小型简介
    /// </summary>
    public SteamMiniProfile? MiniProfile { get; set; }

    /// <summary>
    /// 离线模式
    /// </summary>
    [XmlIgnore]
    public PersonaState PersonaState { get; set; } = PersonaState.Default;

    ///// <summary>
    ///// 来源 Valve Data File 字符串
    ///// </summary>
    //[XmlIgnore]
    //public string? OriginVdfString { get; set; }

    ///// <summary>
    ///// 导出 Valve Data File 配置字符串
    ///// </summary>
    //[XmlIgnore]
    //public string CurrentVdfString =>
    //    "\"" + SteamId64 + "\"\n{\n" +
    //    "\t\t\"AccountName\"\t\t\"" + AccountName + "\"\n" +
    //    "\t\t\"PersonaName\"\t\t\"" + PersonaName + "\"\n" +
    //    "\t\t\"RememberPassword\"\t\t\"" + Convert.ToByte(RememberPassword) + "\"\n" +
    //    "\t\t\"MostRecent\"\t\t\"" + Convert.ToByte(MostRecent) + "\"\n" +
    //    "\t\t\"WantsOfflineMode\"\t\t\"" + Convert.ToByte(WantsOfflineMode) + "\"\n" +
    //    "\t\t\"SkipOfflineModeWarning\"\t\t\"" + Convert.ToByte(SkipOfflineModeWarning) + "\"\n" +
    //    "\t\t\"Timestamp\"\t\t\"" + Timestamp + "\"\n}";
}