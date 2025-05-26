using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Markets;

/// <summary>
/// 市场出售信息
/// </summary>
public sealed record class MarketListings : JsonRecordModel<MarketListings>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 上架中的物品
    /// </summary>
    public IEnumerable<ActiveListingItem> ActiveListings { get; set; } = [];

    /// <summary>
    /// 求购订单
    /// </summary>
    public IEnumerable<BuyorderItem> Buyorders { get; set; } = [];

    /// <summary>
    /// 上架物品
    /// </summary>
    public sealed record class ActiveListingItem : JsonRecordModel<ActiveListingItem>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 物品图片地址
        /// </summary>
        public string ImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// 买家支付价格
        /// </summary>
        public string Payment { get; set; } = string.Empty;

        /// <summary>
        /// 卖家所收金额
        /// </summary>
        public string Received { get; set; } = string.Empty;

        /// <summary>
        /// 上架日期
        /// </summary>
        public string ListingDate { get; set; } = string.Empty;

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 物品市场链接
        /// </summary>
        public string ItemMarketUrl { get; set; } = string.Empty;
    }

    public sealed record class BuyorderItem : JsonRecordModel<BuyorderItem>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 物品图片地址
        /// </summary>
        public string ImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// 买家支付价格
        /// </summary>
        public string Price { get; set; } = string.Empty;

        /// <summary>
        /// 上架日期
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 物品市场链接
        /// </summary>
        public string ItemMarketUrl { get; set; } = string.Empty;
    }
}
