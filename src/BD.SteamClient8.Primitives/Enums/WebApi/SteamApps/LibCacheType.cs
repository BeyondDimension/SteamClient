namespace BD.SteamClient8.Enums.WebApi.SteamApps;

/// <summary>
/// Library 图片缓存类型
/// </summary>
public enum LibCacheType : byte
{
    /// <summary>
    /// <see langword="HeaderLogoUrl"/>
    /// </summary>
    Header,

    /// <summary>
    /// <see langword="abstract"/>
    /// </summary>
    Icon,

    /// <summary>
    /// <see langword="LibraryGridUrl"/>
    /// </summary>
    Library_Grid,

    /// <summary>
    /// <see langword="LibraryHeroUrl"/>
    /// </summary>
    Library_Hero,

    /// <summary>
    /// <see langword="LibraryHeroBlurUrl"/>
    /// </summary>
    Library_Hero_Blur,

    /// <summary>
    /// <see langword="LibraryLogoUrl"/>
    /// </summary>
    Logo,
}
