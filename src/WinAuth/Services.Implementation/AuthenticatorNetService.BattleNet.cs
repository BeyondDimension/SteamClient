// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

#pragma warning disable SA1600 // Elements should be documented

public sealed class BattleNetService : WebApiClientFactoryService, IBattleNetService
{
    private const string Tag = "BattleNetService";

    protected override string ClientName => Tag;

    public BattleNetService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(Tag), serviceProvider)
    {
    }

    /// <summary>
    /// URLs for all mobile services
    /// </summary>
    const string REGION_US = "US";
    const string REGION_EU = "EU";
    const string REGION_KR = "KR";
    const string REGION_CN = "CN";

    /// <summary>
    /// URLS for mobile service
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
    /// URL for GEO IP lookup to determine region
    /// </summary>
    static readonly string GEOIPURL = "http://geoiplookup.wikimedia.org";

    /// <summary>
    /// Get the base mobil url based on the region
    /// </summary>
    /// <param name="region">two letter region code, i.e US or CN</param>
    /// <returns>string of Url for region</returns>
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
