// ReSharper disable once CheckNamespace
namespace WinAuth.Services;

/// <summary>
/// 谷歌令牌相关服务
/// </summary>
public interface IGoogleNetService
{
    /// <summary>
    /// 同步时间
    /// </summary>
    /// <returns></returns>
    Task<HttpResponseMessage> TimeSync();
}
