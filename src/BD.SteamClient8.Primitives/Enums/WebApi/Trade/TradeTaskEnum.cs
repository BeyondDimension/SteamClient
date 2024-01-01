#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Enums;

/// <summary>
/// 交易报价后台任务类型枚举
/// </summary>
public enum TradeTaskEnum : byte
{
    None = 0,

    /// <summary>
    /// 自动接收礼品报价
    /// </summary>
    AutoAcceptGitTrade = 1,
}

