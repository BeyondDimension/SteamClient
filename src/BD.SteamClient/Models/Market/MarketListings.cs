namespace BD.SteamClient;

public class MarketListings
{
    public IEnumerable<ActiveListingItem> ActiveListings { get; set; } = Enumerable.Empty<ActiveListingItem>();

    public IEnumerable<BuyorderItem> Buyorders { get; set; } = Enumerable.Empty<BuyorderItem>();

    /// <summary>
    /// 上架物品
    /// </summary>
    public record struct ActiveListingItem
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 物品图片地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 买家支付价格
        /// </summary>
        public string Payment { get; set; }

        /// <summary>
        /// 卖家所收金额
        /// </summary>
        public string Received { get; set; }

        /// <summary>
        /// 上架日期
        /// </summary>
        public string ListingDate { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 物品市场链接
        /// </summary>
        public string ItemMarketUrl { get; set; }
    }

    public record struct BuyorderItem
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 物品图片地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 买家支付价格
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 上架日期
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 物品市场链接
        /// </summary>
        public string ItemMarketUrl { get; set; }
    }
}
