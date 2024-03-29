#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Services;

/// <summary>
/// Steam 挂卡相关服务
/// </summary>
public interface ISteamIdleCardService
{
    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamIdleCardService Instance => Ioc.Get<ISteamIdleCardService>();

    /// <summary>
    /// 获取用户徽章和卡片数据
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="need_price"></param>
    /// <param name="currency"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<UserBadgesResponse>> GetBadgesAsync(string steam_id, bool need_price = false, string currency = "CNY", CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取游戏卡组卡片平均价格
    /// </summary>
    /// <param name="appIds"></param>
    /// <param name="currency"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<List<AppCardsAvgPrice>>> GetAppCardsAvgPrice(uint[] appIds, string currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取游戏卡片价格
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="currency"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<List<CardsMarketPrice>>> GetCardsMarketPrice(uint appId, string currency, CancellationToken cancellationToken = default);
}
