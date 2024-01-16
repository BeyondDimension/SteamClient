#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 礼物发送记录
/// </summary>
public record class SendGiftHistoryItem
{
    /// <summary>
    /// 游戏名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 游戏图片地址
    /// </summary>
    public string ImgUrl { get; set; } = string.Empty;

    /// <summary>
    /// 签收状态文本
    /// </summary>
    public string RedeemedGiftStatusText { get; set; } = string.Empty;

    /// <summary>
    /// 礼物状态文本
    /// </summary>
    public string GiftStatusText { get; set; } = string.Empty;
}
