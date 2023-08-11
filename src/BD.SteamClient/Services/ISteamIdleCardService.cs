namespace BD.SteamClient.Services;

public interface ISteamIdleCardService
{
    /// <summary>
    /// 获取用户徽章和卡片数据
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<IEnumerable<Badge>> GetBadgesAsync(SteamSession steamSession);
}
