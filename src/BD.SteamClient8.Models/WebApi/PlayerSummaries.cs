namespace BD.SteamClient8.Models.WebApi;

public sealed record PlayerSummaries
{
    /// <summary>
    /// 用户昵称
    /// </summary>
    [NewtonsoftJsonProperty("personaname")]
    [SystemTextJsonProperty("personaname")]
    public string PersonaName { get; set; } = string.Empty;

    /// <summary>
    /// 用户真实姓名
    /// </summary>
    [NewtonsoftJsonProperty("realname")]
    [SystemTextJsonProperty("realname")]
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 账号创建时间戳
    /// </summary>
    [NewtonsoftJsonProperty("timecreated")]
    [SystemTextJsonProperty("timecreated")]
    public long TimeCreated { get; set; }

    /// <summary>
    /// 用户个人资料主页
    /// </summary>
    [NewtonsoftJsonProperty("profileurl")]
    [SystemTextJsonProperty("profileurl")]
    public string Profileurl { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像
    /// </summary>
    [NewtonsoftJsonProperty("avatar")]
    [SystemTextJsonProperty("avatar")]
    public string Avatar { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像（中）
    /// </summary>
    [NewtonsoftJsonProperty("avatarmedium")]
    [SystemTextJsonProperty("avatarmedium")]
    public string AvatarMedium { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像（大）
    /// </summary>
    [NewtonsoftJsonProperty("avatarfull")]
    [SystemTextJsonProperty("avatarfull")]
    public string AvatarFull { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像哈希值，可用于拼接头像地址
    /// </summary>
    [NewtonsoftJsonProperty("avatarhash")]
    [SystemTextJsonProperty("avatarhash")]
    public string AvatarHash { get; set; } = string.Empty;

    /// <summary>
    /// 用户创建时间
    /// </summary>
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public DateTimeOffset CreationTime => DateTimeOffset.FromUnixTimeSeconds(TimeCreated);
}
