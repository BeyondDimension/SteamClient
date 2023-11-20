namespace BD.SteamClient8.Models.WebApi.Market;

/// <summary>
/// 市场出售信息
/// </summary>
public record class MarketListings
{
    /// <summary>
    /// 上架中的物品
    /// </summary>
    public IEnumerable<ActiveListingItem> ActiveListings { get; set; } = Enumerable.Empty<ActiveListingItem>();

    /// <summary>
    /// 求购订单
    /// </summary>
    public IEnumerable<BuyorderItem> Buyorders { get; set; } = Enumerable.Empty<BuyorderItem>();

    /// <summary>
    /// 上架物品
    /// </summary>
    public record class ActiveListingItem
    {
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

    public record class BuyorderItem
    {
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
