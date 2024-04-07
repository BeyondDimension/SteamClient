namespace BD.SteamClient8.Models;

// https://partner.steamgames.com/doc/webapi/IEconService

/// <summary>
/// 未处理和新的交易报价汇总
/// </summary>
public class TradeSummary
{
    /// <summary>
    /// 待处理的交易报价数量
    /// </summary>
    [SystemTextJsonProperty("pending_received_count")]
    public int PendingReceivedCount { get; set; }

    /// <summary>
    /// 新的收到的交易报价数量
    /// </summary>
    [SystemTextJsonProperty("new_received_count")]
    public int NewReceivedCount { get; set; }

    /// <summary>
    /// 更新的收到交易报价数量
    /// </summary>
    [SystemTextJsonProperty("updated_received_count")]
    public int UpdatedReceivedCount { get; set; }

    /// <summary>
    /// 历史收到的交易报价数量
    /// </summary>
    [SystemTextJsonProperty("historical_received_count")]
    public int HistoricalReceivedCount { get; set; }

    /// <summary>
    /// 待处理的发送交易报价数量
    /// </summary>
    [SystemTextJsonProperty("pending_sent_count")]
    public int PendingSentCount { get; set; }

    /// <summary>
    /// 新接受的发送交易报价数量
    /// </summary>
    [SystemTextJsonProperty("newly_accepted_sent_count")]
    public int NewlyAcceptedSentCount { get; set; }

    /// <summary>
    /// 更新的发送交易报价数量
    /// </summary>
    [SystemTextJsonProperty("updated_sent_count")]
    public int UpdatedSentCount { get; set; }

    /// <summary>
    /// 历史发送数量
    /// </summary>
    [SystemTextJsonProperty("historical_sent_count")]
    public int HistoricalSentCount { get; set; }

    /// <summary>
    /// 在托管状态中的收到交易报价数量
    /// </summary>
    [SystemTextJsonProperty("escrow_received_count")]
    public int EscrowReceivedCount { get; set; }

    /// <summary>
    /// 在托管状态中的发送交易报价数量
    /// </summary>
    [SystemTextJsonProperty("escrow_sent_count")]
    public int EscrowSentCount { get; set; }
}

public partial class TradeSummaryResponse : JsonModel
{
    /// <summary>
    /// 交易报价汇总返回信息
    /// </summary>
    [SystemTextJsonProperty("response")]
    public TradeSummary? Response { get; set; }
}

