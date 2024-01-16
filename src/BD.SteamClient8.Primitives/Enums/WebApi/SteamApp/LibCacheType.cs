#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Enums;

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
