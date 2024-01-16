#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Enums;

/// <summary>
/// 交易历史操作类型
/// </summary>
public enum MarketTradingHistoryRowType
{
    /// <summary>
    /// 普通行
    /// </summary>
    Normal,

    /// <summary>
    /// 上架
    /// </summary>
    Publish,

    /// <summary>
    /// 下架
    /// </summary>
    UnPublish,
}
