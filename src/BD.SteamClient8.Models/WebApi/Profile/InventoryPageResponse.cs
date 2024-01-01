#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// 用户游戏库存信息返回
/// </summary>
public record class InventoryPageResponse
{
    /// <summary>
    /// 更多项
    /// </summary>
    [SystemTextJsonProperty("more_items")]
    public int? MoreItems { get; set; }

    /// <summary>
    /// 最后一个库存资产 Id
    /// </summary>
    [SystemTextJsonProperty("last_assetid")]
    public string LastAssetId { get; set; } = string.Empty;

    /// <summary>
    /// 总共库存资产数量
    /// </summary>
    [SystemTextJsonProperty("total_inventory_count")]
    public int TotalInventoryCount { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public int Success { get; set; }

    /// <summary>
    /// 资产列表
    /// </summary>
    [SystemTextJsonProperty("assets")]
    public ProfileInventoryAsset[]? Assets { get; set; }

    /// <summary>
    /// 资产描述
    /// </summary>
    [SystemTextJsonProperty("descriptions")]
    public ProfileInventoryAssetDescription[]? Descriptions { get; set; }

    /// <summary>
    /// 个人库存资产信息
    /// </summary>
    /// <param name="AppId"></param>
    /// <param name="ContextId"></param>
    /// <param name="AssetId"></param>
    /// <param name="ClassId"></param>
    /// <param name="InstanceId"></param>
    /// <param name="Amount"></param>
    public record class ProfileInventoryAsset(
        [property: SystemTextJsonProperty("appid")] int AppId,
        [property: SystemTextJsonProperty("contextid")] string ContextId,
        [property: SystemTextJsonProperty("assetid")] string AssetId,
        [property: SystemTextJsonProperty("classid")] string ClassId,
        [property: SystemTextJsonProperty("instanceid")] string InstanceId,
        [property: SystemTextJsonProperty("amount")] string Amount);

    /// <summary>
    /// 个人库存资产描述信息
    /// </summary>
    /// <param name="AppId"></param>
    /// <param name="ContextId"></param>
    /// <param name="AssetId"></param>
    /// <param name="ClassId"></param>
    /// <param name="InstanceId"></param>
    /// <param name="Tradable">是否可交易</param>
    /// <param name="Marketable">是否可上架</param>
    /// <param name="Name">名称</param>
    /// <param name="MarketName">市场名称</param>
    /// <param name="MarketHashName">市场唯一名称(市场主页使用该名称作为路由地址)</param>
    /// <param name="IconUrl">图标(需加上 Steam CDN 域名访问)</param>
    public record class ProfileInventoryAssetDescription(
        [property: SystemTextJsonProperty("appid")] int AppId,
        [property: SystemTextJsonProperty("contextid")] string ContextId,
        [property: SystemTextJsonProperty("assetid")] string AssetId,
        [property: SystemTextJsonProperty("classid")] string ClassId,
        [property: SystemTextJsonProperty("instanceid")] string InstanceId,
        [property: SystemTextJsonProperty("tradable")] int Tradable,
        [property: SystemTextJsonProperty("marketable")] int Marketable,
        [property: SystemTextJsonProperty("name")] string Name,
        [property: SystemTextJsonProperty("market_name")] string MarketName,
        [property: SystemTextJsonProperty("market_hash_name")] string MarketHashName,
        [property: SystemTextJsonProperty("icon_url")] string IconUrl)
    {
        /// <summary>
        /// 未设置得属性字典
        /// </summary>
        [SystemTextJsonIgnore]
        [SystemTextJsonExtensionData]
        public Dictionary<string, JsonElement> UnunsedProperties { get; set; } = [];
    }
}
