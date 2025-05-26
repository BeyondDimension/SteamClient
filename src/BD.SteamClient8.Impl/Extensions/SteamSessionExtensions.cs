using BD.SteamClient8.Models.WebApi.Logins;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Extensions;

/// <summary>
/// <see cref="SteamSession"/> 扩展方法
/// </summary>
public static partial class SteamSessionExtensions
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
