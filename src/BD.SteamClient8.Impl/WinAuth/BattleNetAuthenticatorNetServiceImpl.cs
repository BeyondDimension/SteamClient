namespace BD.SteamClient8.WinAuth;

/// <summary>
/// Initializes a new instance of the <see cref="BattleNetAuthenticatorNetServiceImpl"/> class.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
sealed class BattleNetAuthenticatorNetServiceImpl(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG), serviceProvider), BattleNetAuthenticator.IAuthenticatorNetService
{
    const string TAG = "BattleNetAuthenticatorNetService";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

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
    public async Task<ApiRspImpl<string>> GEOIP(CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(GEOIPURL);
        using var rsp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        var result = await rsp!.Content.ReadAsStringAsync(cancellationToken);
        return ApiRspHelper.Ok(result)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<byte[]>> EnRoll(string region, byte[] encrypted, CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(region) + ENROLL_PATH)
        {
            Method = HttpMethod.Post,
            ConfigureRequestMessage = (req, args, token) =>
            {
                var content = new ByteArrayContent(encrypted);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream", Encoding.UTF8.WebName);
                req.Content = content;

                return Task.CompletedTask;
            },
        };
        using var rsp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        var result = await rsp!.Content.ReadAsByteArrayAsync(cancellationToken);
        return ApiRspHelper.Ok(result)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<byte[]>> Sync(string region, CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(region) + SYNC_PATH);
        using var rsp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        var result = await rsp!.Content.ReadAsByteArrayAsync(cancellationToken);
        return ApiRspHelper.Ok(result)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<byte[]>> ReStore(string serial, byte[] serialBytes, CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(serial) + RESTORE_PATH)
        {
            Method = HttpMethod.Post,
            ConfigureRequestMessage = (req, args, token) =>
            {
                var content = new ByteArrayContent(serialBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream", Encoding.UTF8.WebName);
                req.Content = content;

                return Task.CompletedTask;
            },
        };
        using var rsp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        var result = await rsp!.Content.ReadAsByteArrayAsync(cancellationToken);
        return ApiRspHelper.Ok(result)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<byte[]>> ReStoreValidate(string serial, byte[] postbytes, CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(GetMobileUrl(serial) + RESTOREVALIDATE_PATH)
        {
            Method = HttpMethod.Post,
            ConfigureRequestMessage = (req, args, token) =>
            {
                var content = new ByteArrayContent(postbytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream", Encoding.UTF8.WebName);
                req.Content = content;

                return Task.CompletedTask;
            },
        };
        using var rsp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        var result = await rsp!.Content.ReadAsByteArrayAsync(cancellationToken);
        return ApiRspHelper.Ok(result)!;
    }
}
