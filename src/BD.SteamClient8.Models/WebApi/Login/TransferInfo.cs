#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// 跳转信息
/// </summary>
public sealed class TransferInfo : JsonModel
{
    /// <summary>
    /// 跳转域名地址
    /// </summary>
    [SystemTextJsonProperty("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 跳转参数
    /// </summary>
    [SystemTextJsonProperty("params")]
    public TransferInfoParams? Params { get; set; }
}
