namespace BD.SteamClient.Enums;

/// <summary>
/// 图片下载渠道类型
/// </summary>
enum ImageChannelType : byte
{
    /// <summary>
    /// Steam 游戏图片
    /// </summary>
    SteamGames = 1,

    /// <summary>
    /// Steam 成就图标
    /// </summary>
    SteamAchievementIcon = 3,

    BD_SteamClient = 50,

    // 给 BD.SteamClient 保留的值，从 51 开始
}

/// <summary>
/// Enum 扩展 <see cref="ImageChannelType"/>
/// </summary>
internal static class ImageChannelTypeEnumExtensions
{
    /// <inheritdoc cref="IImageHttpClientService.GetImageAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Task<string?> GetImageAsync(this IImageHttpClientService imageHttpClientService,
        string? requestUri,
        ImageChannelType channelType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestUri)) return Task.FromResult((string?)null);
        var channelType_ = channelType.ToString();
        return imageHttpClientService.GetImageAsync(requestUri, channelType_, cancellationToken);
    }

    /// <inheritdoc cref="IImageHttpClientService.GetImageAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Task<string?> GetImageAsync(this ImageChannelType channelType,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        var imageHttpClientService = Ioc.Get<IImageHttpClientService>();
        return imageHttpClientService.GetImageAsync(requestUri, channelType, cancellationToken);
    }

    /// <inheritdoc cref="IImageHttpClientService.GetImageStreamAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Task<Stream?> GetImageStreamAsync(this IImageHttpClientService imageHttpClientService,
        string? requestUri,
        ImageChannelType channelType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestUri)) return Task.FromResult((Stream?)null);
        var channelType_ = channelType.ToString();
        return imageHttpClientService.GetImageStreamAsync(requestUri, channelType_, cancellationToken);
    }

    /// <inheritdoc cref="IImageHttpClientService.GetImageStreamAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Task<Stream?> GetImageStreamAsync(this ImageChannelType channelType,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        var imageHttpClientService = Ioc.Get<IImageHttpClientService>();
        return imageHttpClientService.GetImageStreamAsync(requestUri, channelType, cancellationToken);
    }
}