namespace BD.SteamClient8.Models;

// https://partner.steamgames.com/doc/webapi/IEconService

/// <summary>
/// 交易历史记录
/// </summary>
public sealed record class TradeHistory
{
    /// <summary>
    /// 返回详情
    /// </summary>
    [SystemTextJsonProperty("response")]
    public TradeHistoryResponseDetail? Response { get; set; }

    /// <summary>
    /// 交易历史记录响应详情
    /// </summary>
    public sealed record class TradeHistoryResponseDetail
    {
        /// <summary>
        /// 是否更多
        /// </summary>
        [SystemTextJsonProperty("more")]
        public bool More { get; set; }

        /// <summary>
        /// 资产物品列表
        /// </summary>
        [SystemTextJsonProperty("trades")]
        public TradeItem[] Trades { get; set; } = [];

        /// <summary>
        /// 描述信息
        /// </summary>
        [SystemTextJsonProperty("descriptions")]
        public TradeItemAssetItemDesc[]? Descriptions { get; set; }
    }

    /// <summary>
    /// 交易历史记录信息
    /// </summary>
    public sealed record class TradeItem
    {
        /// <summary>
        /// 交易 Id
        /// </summary>
        [SystemTextJsonProperty("tradeid")]
        public string TradeId { get; set; } = string.Empty;

        /// <summary>
        /// 交易对象 SteamId
        /// </summary>
        [SystemTextJsonProperty("steamid_other")]
        public string SteamIdOther { get; set; } = string.Empty;

        /// <summary>
        /// 初始化时间
        /// </summary>
        [SystemTextJsonProperty("time_init")]
        public long TimeInit { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        [SystemTextJsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 失去的物品列表
        /// </summary>
        [SystemTextJsonProperty("assets_given")]
        public TradeItemAssetItem[]? AssetsGiven { get; set; }

        /// <summary>
        /// 接收的物品列表
        /// </summary>
        [SystemTextJsonProperty("assets_received")]
        public TradeItemAssetItem[]? AssetsReceived { get; set; }
    }

    /// <summary>
    /// 交易历史记录的交易项
    /// </summary>
    public sealed record class TradeItemAssetItem
    {
        /// <summary>
        /// 游戏 AppId
        /// </summary>
        [SystemTextJsonProperty("appid")]
        public int AppId { get; set; }

        /// <summary>
        /// 库存资产上下文 Id
        /// </summary>
        [SystemTextJsonProperty("contextid")]
        public string ContextId { get; set; } = string.Empty;

        /// <summary>
        /// 库存资产 Id
        /// </summary>
        [SystemTextJsonProperty("assetid")]
        public string AssetId { get; set; } = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        [SystemTextJsonProperty("amount")]
        [SystemTextJsonConverter(typeof(StringToDecimalConverter))]
        public decimal Amount { get; set; }

        /// <summary>
        /// 库存资产类别 Id
        /// </summary>
        [SystemTextJsonProperty("classid")]
        public string ClassId { get; set; } = string.Empty;

        /// <summary>
        /// 库存资产实例 Id
        /// </summary>
        [SystemTextJsonProperty("instanceid")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 交易后新资产 Id
        /// </summary>
        [SystemTextJsonProperty("new_assetid")]
        public string NewAssetId { get; set; } = string.Empty;

        /// <summary>
        /// 交易后新上下文 Id
        /// </summary>
        [SystemTextJsonProperty("new_contextid")]
        public string NewContextId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 交易历史记录的交易项描述
    /// </summary>
    public sealed record class TradeItemAssetItemDesc
    {
        /// <summary>
        /// 游戏 AppId
        /// </summary>
        [SystemTextJsonProperty("appid")]
        public int AppId { get; set; }

        /// <summary>
        /// 库存资产类别 Id
        /// </summary>
        [SystemTextJsonProperty("classid")]
        public string ClassId { get; set; } = string.Empty;

        /// <summary>
        /// 库存资产实例 Id
        /// </summary>
        [SystemTextJsonProperty("instanceid")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 市场名称
        /// </summary>
        [SystemTextJsonProperty("market_name")]
        public string MarketName { get; set; } = string.Empty;

        /// <summary>
        /// 市场 hash 名称
        /// </summary>
        [SystemTextJsonProperty("market_hash_name")]
        public string MarketHashName { get; set; } = string.Empty;
    }
}
