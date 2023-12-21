// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

/// <summary>
/// <see cref="IGoogleNetService"/> 谷歌令牌相关服务实现
/// </summary>
public sealed partial class GoogleNetService : IGoogleNetService
{
    /// <summary>
    /// 初始化 <see cref="GoogleNetService"/> 类的新实例
    /// </summary>
    public GoogleNetService()
    {
    }

    /// <summary>
    /// 用于同步时间的 URL
    /// </summary>
    const string TIME_SYNC_URL = "http://www.google.com";

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> TimeSync()
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, TIME_SYNC_URL)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/html"),
        };
        using var client = new HttpClient();
        client.Timeout = new TimeSpan(0, 0, 5);
        return await client.SendAsync(requestMessage);
    }
}
