namespace BD.SteamClient8.Impl.Extensions;

#pragma warning disable SA1600 // Elements should be documented

public static class CommonImageSourceExtensions
{
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
