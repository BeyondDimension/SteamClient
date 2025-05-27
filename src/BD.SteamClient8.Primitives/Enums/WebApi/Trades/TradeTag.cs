using System.ComponentModel;

namespace BD.SteamClient8.Enums.WebApi.Trades;

/// <summary>
/// 交易报价确认信息操作类型
/// </summary>
public enum TradeTag : byte
{
    /// <summary>
    /// conf
    /// </summary>
    [Description("conf")]
    CONF = 1,

    /// <summary>
    /// details
    /// </summary>
    [Description("details")]
    DETAILS = 2,

    /// <summary>
    /// 允许
    /// </summary>
    [Description("allow")]
    ALLOW = 3,

    /// <summary>
    /// 取消
    /// </summary>
    [Description("cancel")]
    CANCEL = 4
}
