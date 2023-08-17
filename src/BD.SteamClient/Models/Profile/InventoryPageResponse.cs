namespace BD.SteamClient.Models.Profile;

public record struct InventoryPageResponse
{
    [S_JsonProperty("more_items")]
    public int? MoreItems { get; set; }

    [S_JsonProperty("last_assetid")]
    public string LastAssetId { get; set; }

    [S_JsonProperty("total_inventory_count")]
    public int TotalInventoryCount { get; set; }

    [S_JsonProperty("success")]
    public int Success { get; set; }

    [S_JsonProperty("assets")]
    public ProfileInventoryAsset[]? Assets { get; set; }

    [S_JsonProperty("descriptions")]
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
    public record ProfileInventoryAsset([property: S_JsonProperty("appid")] int AppId, [property: S_JsonProperty("contextid")] string ContextId, [property: S_JsonProperty("assetid")] string AssetId, [property: S_JsonProperty("classid")] string ClassId, [property: S_JsonProperty("instanceid")] string InstanceId, [property: S_JsonProperty("amount")] string Amount);

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
    public record ProfileInventoryAssetDescription([property: S_JsonProperty("appid")] int AppId, [property: S_JsonProperty("contextid")] string ContextId, [property: S_JsonProperty("assetid")] string AssetId, [property: S_JsonProperty("classid")] string ClassId, [property: S_JsonProperty("instanceid")] string InstanceId, [property: S_JsonProperty("tradable")] int Tradable, [property: S_JsonProperty("marketable")] int Marketable, [property: S_JsonProperty("name")] string Name, [property: S_JsonProperty("market_name")] string MarketName, [property: S_JsonProperty("market_hash_name")] string MarketHashName, [property: S_JsonProperty("icon_url")] string IconUrl)
    {
        [JsonIgnore]
        [JsonExtensionData]
        public Dictionary<string, JsonElement> UnunsedProperties { get; set; } = new();
    }
}
