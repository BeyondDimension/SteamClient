// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace SteamKit2;

public static partial class ECurrencyCodeEnumExtensions
{
    /// <summary>
    /// 根据货币获取文化信息
    /// </summary>
    /// <param name="eCurrencyCode"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CultureInfo? GetCultureInfo(this ECurrencyCode eCurrencyCode)
    {
        if (eCurrencyCode == ECurrencyCode.Invalid) return null;
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(culture => new RegionInfo(culture.LCID).ISOCurrencySymbol == eCurrencyCode.ToString());
    }
}