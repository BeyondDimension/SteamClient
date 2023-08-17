namespace BD.SteamClient;

public struct SendGiftHisotryItem
{
    /// <summary>
    /// 游戏名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 游戏图片地址
    /// </summary>
    public string ImgUrl { get; set; }

    /// <summary>
    /// 签收状态文本
    /// </summary>
    public string RedeemedGiftStatusText { get; set; }

    /// <summary>
    /// 礼物状态文本
    /// </summary>
    public string GiftStatusText { get; set; }
}
