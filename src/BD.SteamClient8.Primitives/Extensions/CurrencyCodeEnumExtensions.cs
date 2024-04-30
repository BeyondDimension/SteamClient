namespace BD.SteamClient8.Extensions;

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