// ReSharper disable once CheckNamespace
namespace WinAuth.Services;

/// <summary>
/// 战网令牌相关服务
/// </summary>
public interface IBattleNetService
{
    /// <summary>
    /// 地理位置 IP
    /// </summary>
    /// <returns></returns>
    Task<HttpResponseMessage?> GEOIP();

    /// <summary>
    /// 令牌添加
    /// </summary>
    /// <param name="region"></param>
    /// <param name="encrypted"></param>
    /// <returns></returns>
    Task<HttpResponseMessage?> EnRoll(string region, byte[] encrypted);

    /// <summary>
    /// 令牌同步
    /// </summary>
    /// <param name="region"></param>
    /// <returns></returns>
    HttpResponseMessage? Sync(string region);

    /// <summary>
    /// 令牌恢复
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="serialBytes"></param>
    /// <returns></returns>
    Task<HttpResponseMessage?> ReStore(string serial, byte[] serialBytes);

    /// <summary>
    /// 令牌恢复验证
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="postbytes"></param>
    /// <returns></returns>
    Task<HttpResponseMessage?> ReStoreValidate(string serial, byte[] postbytes);
}
