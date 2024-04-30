namespace BD.SteamClient8.Models.WebApi.Profiles;

/// <summary>
/// 礼物发送记录
/// </summary>
public sealed record class SendGiftHistoryItem
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
