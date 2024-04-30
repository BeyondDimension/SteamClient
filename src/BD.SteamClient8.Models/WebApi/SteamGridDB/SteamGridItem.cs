namespace BD.SteamClient8.Models.WebApi.SteamGridDB;

public sealed record class SteamGridItem
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    [SystemTextJsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// 分数
    /// </summary>
    [SystemTextJsonProperty("score")]
    public int Score { get; set; }

    /// <summary>
    /// 风格
    /// </summary>
    [SystemTextJsonProperty("style")]
    public string Style { get; set; } = "";

    /// <summary>
    /// 链接地址
    /// </summary>
    [SystemTextJsonProperty("url")]
    public string Url { get; set; } = "";

    [SystemTextJsonProperty("thumb")]
    public string Thumb { get; set; } = "";

    /// <summary>
    /// 标签
    /// </summary>
    [SystemTextJsonProperty("tags")]
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 作者
    /// </summary>
    [SystemTextJsonProperty("author")]
    public SteamGridItemAuthor Author { get; set; } = new();

    /// <summary>
    /// <see cref="SteamGridItemType" /> 类型
    /// </summary>
    [SystemTextJsonIgnore]
    public SteamGridItemType GridType { get; set; }
}

public sealed class SteamGridItemAuthor
{
    /// <summary>
    /// 用户名称
    /// </summary>
    [SystemTextJsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Steam64 ID
    /// </summary>
    [SystemTextJsonProperty("steam64")]
    public string Steam64 { get; set; } = "";

    /// <summary>
    /// 头像
    /// </summary>
    [SystemTextJsonProperty("avatar")]
    public string Avatar { get; set; } = "";
}

public sealed class SteamGridItemData
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 数据列表
    /// </summary>
    [SystemTextJsonProperty("data")]
    public List<SteamGridItem> Data { get; set; } = [];

    /// <summary>
    /// 错误列表
    /// </summary>
    [SystemTextJsonProperty("errors")]
    public List<string> Errors { get; set; } = [];
}
