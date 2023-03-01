#if WINDOWS || MACCATALYST || MACOS || LINUX

// ReSharper disable once CheckNamespace
namespace SteamKit2;

public static partial class ECurrencyCodeEnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CultureInfo? GetCultureInfo(this ECurrencyCode eCurrencyCode)
    {
        if (eCurrencyCode == ECurrencyCode.Invalid) return null;
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(culture => new RegionInfo(culture.LCID).ISOCurrencySymbol == eCurrencyCode.ToString());
    }
}

#endif