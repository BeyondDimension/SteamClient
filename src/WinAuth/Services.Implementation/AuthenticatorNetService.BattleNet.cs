// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

/// <summary>
/// <see cref="IBattleNetService"/> 战网令牌相关服务实现
/// </summary>
public sealed class BattleNetService : WebApiClientFactoryService, IBattleNetService
{
    private const string Tag = "BattleNetService";

    /// <inheritdoc/>
    protected override string ClientName => Tag;

    /// <summary>
    /// 初始化 <see cref="BattleNetService"/> 类的新实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public BattleNetService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(Tag), serviceProvider)
    {
    }

    /// <summary>
    /// 所有移动服务的 URL
    /// </summary>
    const string REGION_US = "US";
    const string REGION_EU = "EU";
    const string REGION_KR = "KR";
    const string REGION_CN = "CN";

    /// <summary>
    /// 用于移动服务的 URLS
    /// </summary>
    public static Dictionary<string, string> MOBILE_URLS = new()
    {
        { REGION_US, "http://mobile-service.blizzard.com" },
        { REGION_EU, "http://mobile-service.blizzard.com" },
        { REGION_KR, "http://mobile-service.blizzard.com" },
        { REGION_CN, "http://mobile-service.battlenet.com.cn" },
    };

    const string ENROLL_PATH = "/enrollment/enroll2.htm";
    const string SYNC_PATH = "/enrollment/time.htm";
    const string RESTORE_PATH = "/enrollment/initiatePaperRestore.htm";
    const string RESTOREVALIDATE_PATH = "/enrollment/validatePaperRestore.htm";

    /// <summary>
    /// 用于 GEO IP 查找以确定区域的 URL
    /// </summary>
    static readonly string GEOIPURL = "http://geoiplookup.wikimedia.org";

    /// <summary>
    /// 获取基于区域的基本 mobil url
    /// </summary>
    /// <param name="region">两个字母的地区代码，即美国或中国</param>
    /// <returns>区域的 Url 字符串</returns>
    private static string GetMobileUrl(string region)
    {
        var upperregion = region.ToUpper();
        if (upperregion.Length > 2)
        {
            upperregion = upperregion[..2];
        }
        if (MOBILE_URLS.ContainsKey(upperregion) == true)
        {
            return MOBILE_URLS[upperregion];
        }
        else
        {
            return MOBILE_URLS[REGION_US];
        }
    }

    /// <summary>
    /// 地理位置 IP
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseMessage?> GEOIP()
    {
        using var sendArgs = new WebApiClientSendArgs(GEOIPURL)
        {
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            }
        };
        sendArgs.SetHttpClient(CreateClient());
        return await SendAsync<HttpResponseMessage>(sendArgs);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage?> EnRoll(string region, byte[] encrypted)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(region) + ENROLL_PATH)
        {
            Method = HttpMethod.Post,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Content = new StringContent(Encoding.UTF8.GetString(encrypted, 0, encrypted.Length), Encoding.UTF8, "application/octet-stream");
                req.Content.Headers.ContentLength = encrypted.Length;
            }
        };
        sendArgs.SetHttpClient(CreateClient());
        return await SendAsync<HttpResponseMessage>(sendArgs);
    }

    /// <summary>
    /// 令牌同步
    /// </summary>
    /// <param name="Region"></param>
    /// <returns></returns>
    public HttpResponseMessage? Sync(string Region)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(Region) + SYNC_PATH);
        sendArgs.SetHttpClient(CreateClient());
        return SendAsync<HttpResponseMessage>(sendArgs).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 恢复
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="serialBytes"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage?> ReStore(string serial, byte[] serialBytes)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(serial) + RESTORE_PATH)
        {
            Method = HttpMethod.Post,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Content = new StringContent(Encoding.UTF8.GetString(serialBytes, 0, serialBytes.Length), Encoding.UTF8, "application/octet-stream");
                req.Content.Headers.ContentLength = serialBytes.Length;
            }
        };
        sendArgs.SetHttpClient(CreateClient());
        return await SendAsync<HttpResponseMessage>(sendArgs);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage?> ReStoreValidate(string serial, byte[] postbytes)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(serial) + RESTOREVALIDATE_PATH)
        {
            Method = HttpMethod.Post,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Content = new StringContent(Encoding.UTF8.GetString(postbytes, 0, postbytes.Length), Encoding.UTF8, "application/octet-stream");
                req.Content.Headers.ContentLength = postbytes.Length;
            }
        };
        sendArgs.SetHttpClient(CreateClient());
        return await SendAsync<HttpResponseMessage>(sendArgs);
    }
}
