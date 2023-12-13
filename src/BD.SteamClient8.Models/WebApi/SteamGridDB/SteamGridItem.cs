namespace BD.SteamClient8.Models.WebApi.SteamGridDB;

#pragma warning disable SA1600 // Elements should be documented

public record class SteamGridItem
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分数
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 风格
    /// </summary>
    public string Style { get; set; } = "";

    /// <summary>
    /// 链接地址
    /// </summary>
    public string Url { get; set; } = "";

    public string Thumb { get; set; } = "";

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 作者
    /// </summary>
    public SteamGridItemAuthor Author { get; set; } = new();

    /// <summary>
    /// <see cref="SteamGridItemType" /> 类型
    /// </summary>
    public SteamGridItemType GridType { get; set; }
}

public class SteamGridItemAuthor
{
    /// <summary>
    /// 用户名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Steam64 ID
    /// </summary>
    public string Steam64 { get; set; } = "";

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; } = "";
}

public class SteamGridItemData
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 数据列表
    /// </summary>
    public List<SteamGridItem> Data { get; set; } = [];

    /// <summary>
    /// 错误列表
    /// </summary>
    public List<string> Errors { get; set; } = [];
}
