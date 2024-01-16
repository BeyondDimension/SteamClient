#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 库存资产
/// </summary>
public record class TradeAsset
{
    /// <summary>
    /// 游戏 AppId
    /// </summary>
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    /// <summary>
    /// 上下文 Id
    /// </summary>
    [JsonPropertyName("contextid")]
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 资产 Id
    /// </summary>
    [JsonPropertyName("assetid")]
    public string AssetId { get; set; } = string.Empty;
}
