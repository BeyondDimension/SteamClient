namespace BD.SteamClient8.Models.WebApi.SteamGridDB;

#pragma warning disable SA1600 // Elements should be documented

public record class SteamGridApp
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    [SystemTextJsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SystemTextJsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 类型列表
    /// </summary>
    [SystemTextJsonProperty("types")]
    public List<string> Types { get; set; } = [];

    /// <summary>
    /// 是否验证
    /// </summary>
    [SystemTextJsonProperty("verified")]
    public bool Verified { get; set; }
}

public class SteamGridAppData
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 数据内容
    /// </summary>
    [SystemTextJsonProperty("data")]
    public SteamGridApp Data { get; set; } = new();

    /// <summary>
    /// 错误列表
    /// </summary>
    [SystemTextJsonProperty("errors")]
    public List<string> Errors { get; set; } = [];
}
