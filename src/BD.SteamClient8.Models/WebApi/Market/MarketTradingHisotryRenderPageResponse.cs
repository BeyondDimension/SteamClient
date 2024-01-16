#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 市场交易历史 html
/// </summary>
public record class MarketTradingHistoryRenderPageResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 页数
    /// </summary>
    [SystemTextJsonProperty("pagesize")]
    public int PageSize { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    [SystemTextJsonProperty("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// 资产列表
    /// </summary>
    [SystemTextJsonProperty("assets")]
    public Dictionary<string, JsonElement>? Assets { get; set; }

    /// <summary>
    /// 鼠标悬浮预览图
    /// </summary>
    [SystemTextJsonProperty("hovers")]
    public string? Hovers { get; set; }

    /// <summary>
    /// 返回 Html
    /// </summary>
    [SystemTextJsonProperty("results_html")]
    public string ResultsHtml { get; set; } = string.Empty;
}

/// <summary>
/// 市场交易历史
/// </summary>
public record class MarketTradingHistoryRenderItem
{
    /// <summary>
    /// 交易历史行 Id
    /// </summary>
    public string HistoryRow { get; set; } = string.Empty;

    /// <summary>
    /// 上架物品名称
    /// </summary>
    public string MarketItemName { get; set; } = string.Empty;

    /// <summary>
    /// 游戏名称
    /// </summary>
    public string GameName { get; set; } = string.Empty;

    /// <summary>
    /// 上架物品图片 Url
    /// </summary>
    public string MarketItemImgUrl { get; set; } = string.Empty;

    /// <summary>
    /// 上架价格
    /// </summary>
    public string? ListingPrice { get; set; }

    /// <summary>
    /// 上架日期
    /// </summary>
    public DateTime ListingDate { get; set; }

    /// <summary>
    /// 交易日期
    /// </summary>
    public DateTime? TradingDate { get; set; }

    /// <summary>
    /// 买家昵称
    /// </summary>
    public string? TradingPartnerLabel { get; set; }

    /// <summary>
    /// 卖家头像
    /// </summary>
    public string? TradingPartnerAvatar { get; set; }

    /// <summary>
    /// 历史交易操作类型
    /// </summary>
    public MarketTradingHistoryRowType RowType { get; set; }
}