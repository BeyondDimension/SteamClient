namespace BD.SteamClient8.Impl.Extensions;

/// <summary>
/// 提供 <see cref="CommonImageSource"/> 扩展
/// </summary>
public static class CommonImageSourceExtensions
{
    /// <summary>
    /// 获取 <see cref="CommonImageSource"/> 对象
    /// </summary>
    /// <param name="requestUri">图片的请求地址</param>
    /// <param name="isPolly">是否进行 Polly 重试</param>
    /// <param name="cache">是否使用缓存</param>
    /// <param name="cacheFirst">是否优先使用缓存</param>
    /// <param name="category">HTTP 请求的优先级别</param>
    /// <param name="isCircle">是否将图片剪裁为圆形</param>
    /// <param name="config">对 CommonImageSource 对象的配置委托</param>
    /// <param name="cancellationToken">取消操作的取消标记</param>
    /// <returns>异步返回 <see cref="CommonImageSource"/> 对象，如果获取失败则返回 <see langword="null"/></returns>
    public static async Task<CommonImageSource?> GetAsync(
        string requestUri,
        bool isPolly = true,
        bool cache = false,
        bool cacheFirst = false,
        HttpHandlerCategory category = HttpHandlerCategory.UserInitiated,
        bool isCircle = false,
        Action<CommonImageSource>? config = null,
        CancellationToken cancellationToken = default)
    {
        var imageHttpClientService = Ioc.Get_Nullable<IImageHttpClientService>();
        if (imageHttpClientService == default)
            return default;

        var imageMemoryStream = await imageHttpClientService.GetImageMemoryStreamAsync(requestUri, isPolly, cache, cacheFirst, category, cancellationToken);
        if (imageMemoryStream == default)
            return default;

        CommonImageSource? clipStream = imageMemoryStream;
        if (clipStream != null)
        {
            clipStream.Circle = isCircle;
            config?.Invoke(clipStream);
        }
        return clipStream;
    }
}
