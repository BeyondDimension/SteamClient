#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 交易报价物品信息
/// </summary>
public record class TradeOffersItem
{
    /// <summary>
    /// 游戏 AppId
    /// </summary>
    [SystemTextJsonProperty("appid")]
    public int AppId { get; set; }

    /// <summary>
    /// 资产上下文 Id
    /// </summary>
    [SystemTextJsonProperty("contextid")]
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// 资产 Id
    /// </summary>
    [SystemTextJsonProperty("assetid")]
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// 资产类别 Id
    /// </summary>
    [SystemTextJsonProperty("classid")]
    public string ClassId { get; set; } = string.Empty;

    /// <summary>
    /// 资产实例 Id
    /// </summary>
    [SystemTextJsonProperty("instanceid")]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    [SystemTextJsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// 市场中找不到该物品
    /// </summary>
    [SystemTextJsonProperty("missing")]
    public bool Missing { get; set; }

    /// <summary>
    /// 预估的美元
    /// </summary>
    [SystemTextJsonProperty("est_usd")]
    public string EstUsd { get; set; } = string.Empty;
}