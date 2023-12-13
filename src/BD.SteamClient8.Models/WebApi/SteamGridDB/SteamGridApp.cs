namespace BD.SteamClient8.Models.WebApi.SteamGridDB;

#pragma warning disable SA1600 // Elements should be documented

public record class SteamGridApp
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 类型列表
    /// </summary>
    public List<string> Types { get; set; } = [];

    /// <summary>
    /// 是否验证
    /// </summary>
    public bool Verified { get; set; }
}

public class SteamGridAppData
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 数据内容
    /// </summary>
    public SteamGridApp Data { get; set; } = new();

    /// <summary>
    /// 错误列表
    /// </summary>
    public List<string> Errors { get; set; } = [];
}
