using BD.Common8.Models.Abstractions;
using System.Text.Json;

namespace BD.SteamClient8.Models.WebApi.Profiles;

/// <summary>
/// 用户游戏库存信息返回
/// </summary>
public sealed record class InventoryPageResponse : JsonRecordModel<InventoryPageResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 更多项
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("more_items")]
    public int? MoreItems { get; set; }

    /// <summary>
    /// 最后一个库存资产 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("last_assetid")]
    public string LastAssetId { get; set; } = string.Empty;

    /// <summary>
    /// 总共库存资产数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("total_inventory_count")]
    public int TotalInventoryCount { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public int Success { get; set; }

    /// <summary>
    /// 资产列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("assets")]
    public ProfileInventoryAsset[]? Assets { get; set; }

    /// <summary>
    /// 资产描述
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("descriptions")]
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
        [property: global::System.Text.Json.Serialization.JsonPropertyName("appid")] int AppId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("contextid")] string ContextId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("assetid")] string AssetId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("classid")] string ClassId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("instanceid")] string InstanceId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("amount")] string Amount) : JsonRecordModel<ProfileInventoryAsset>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;
    }

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
        [property: global::System.Text.Json.Serialization.JsonPropertyName("appid")] int AppId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("contextid")] string ContextId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("assetid")] string AssetId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("classid")] string ClassId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("instanceid")] string InstanceId,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("tradable")] int Tradable,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("marketable")] int Marketable,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("market_name")] string MarketName,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("market_hash_name")] string MarketHashName,
        [property: global::System.Text.Json.Serialization.JsonPropertyName("icon_url")] string IconUrl) : JsonRecordModel<ProfileInventoryAssetDescription>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 未设置得属性字典
        /// </summary>
        [global::System.Text.Json.Serialization.JsonIgnore]
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public Dictionary<string, JsonElement> UnunsedProperties { get; set; } = [];
    }
}
