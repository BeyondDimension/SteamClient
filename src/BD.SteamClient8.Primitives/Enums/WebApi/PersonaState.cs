namespace BD.SteamClient8.Primitives.Enums.WebApi;

/// <summary>
/// 在线状态
/// </summary>
public enum PersonaState
{
    Default = -1,

    /// <summary>
    /// 离线
    /// </summary>
    Offline = 0,

    /// <summary>
    /// 在线
    /// </summary>
    Online = 1,

    /// <summary>
    /// 忙碌的
    /// </summary>
    Busy = 2,

    /// <summary>
    /// 离开
    /// </summary>
    Away = 3,

    /// <summary>
    /// 打盹
    /// </summary>
    Snooze = 4,

    /// <summary>
    /// 想交易
    /// </summary>
    LookingToTrade = 5,

    /// <summary>
    /// 想玩游戏
    /// </summary>
    LookingToPlay = 6,

    /// <summary>
    /// 隐身
    /// </summary>
    Invisible = 7,
}
