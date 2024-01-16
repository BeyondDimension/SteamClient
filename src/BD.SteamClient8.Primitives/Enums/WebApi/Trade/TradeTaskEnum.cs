#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
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

