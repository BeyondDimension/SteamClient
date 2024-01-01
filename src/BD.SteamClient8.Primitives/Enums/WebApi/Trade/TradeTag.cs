#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Enums;

/// <summary>
/// 交易报价确认信息操作类型
/// </summary>
public enum TradeTag : byte
{
    [Description("conf")]
    CONF = 1,

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
