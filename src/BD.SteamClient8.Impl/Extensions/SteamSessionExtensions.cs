namespace BD.SteamClient8.Impl.Extensions;

/// <summary>
/// <see cref="SteamSession"/> 扩展方法
/// </summary>
public static class SteamSessionExtensions
{
    /// <summary>
    /// 空引用抛出异常，提示用户登录
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static SteamSession ThrowIsNull(this SteamSession? steamSession, string steam_id)
    {
        if (steamSession is null)
            throw new NullReferenceException($"Unable to find session for {steam_id}, please login first");
        return steamSession;
    }
}
