#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 账户历史数据返回值
/// </summary>
public sealed record class HistoryParseResponse
{
    /// <summary>未知</summary>
    public int Unknown;

    /// <summary>商店购买</summary>
    public int StorePurchase;

    /// <summary>商店购买[余额]</summary>
    public int StorePurchaseWallet;

    /// <summary>礼物购买</summary>
    public int GiftPurchase;

    /// <summary>礼物购买[余额]</summary>
    public int GiftPurchaseWallet;

    /// <summary>市场购买</summary>
    public int MarketPurchase;

    /// <summary>市场出售</summary>
    public int MarketSelling;

    /// <summary>游戏内购</summary>
    public int InGamePurchase;

    /// <summary>退款</summary>
    public int RefundPurchase;

    /// <summary>退款[余额]</summary>
    public int RefundPurchaseWallet;

    /// <summary>购买余额</summary>
    public int WalletPurchase;

    /// <summary>其他</summary>
    public int Other;

    public static HistoryParseResponse operator +(HistoryParseResponse a, HistoryParseResponse b)
    {
        HistoryParseResponse result = new()
        {
            Unknown = a.Unknown + b.Unknown,
            StorePurchase = a.StorePurchase + b.StorePurchase,
            StorePurchaseWallet = a.StorePurchaseWallet + b.StorePurchaseWallet,
            GiftPurchase = a.GiftPurchase + b.GiftPurchase,
            GiftPurchaseWallet = a.GiftPurchaseWallet + b.GiftPurchaseWallet,
            MarketPurchase = a.MarketPurchase + b.MarketPurchase,
            MarketSelling = a.MarketSelling + b.MarketSelling,
            InGamePurchase = a.InGamePurchase + b.InGamePurchase,
            RefundPurchase = a.RefundPurchase + b.RefundPurchase,
            RefundPurchaseWallet = a.RefundPurchaseWallet + b.RefundPurchaseWallet,
            WalletPurchase = a.WalletPurchase + b.WalletPurchase,
            Other = a.Other + b.Other,
        };
        return result;
    }
}
