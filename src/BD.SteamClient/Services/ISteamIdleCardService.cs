using BD.SteamClient.Models.Idle;

namespace BD.SteamClient.Services;

public interface ISteamIdleCardService
{
    static ISteamIdleCardService Instance => Ioc.Get<ISteamIdleCardService>();

    /// <summary>
    /// 获取用户徽章和卡片数据
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<(UserIdleInfo idleInfo, IEnumerable<Badge> badges)> GetBadgesAsync(string steam_id, bool need_price = false, string currency = "CNY");

    /// <summary>
    /// 获取游戏卡组卡片平均价格
    /// </summary>
    /// <param name="appIds"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    Task<IEnumerable<AppCardsAvgPrice>> GetAppCradsAvgPrice(uint[] appIds, string currency);

    /// <summary>
    /// 获取游戏卡片价格
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    Task<IEnumerable<CardsMarketPrice>> GetCardsMarketPrice(uint appId, string currency);
}
