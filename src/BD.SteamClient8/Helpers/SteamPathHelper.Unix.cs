#if MACCATALYST || MACOS || LINUX
using System.Extensions;
using ValveKeyValue;

namespace BD.SteamClient8.Helpers;

partial class SteamPathHelper
{
    public static partial string? GetAutoLoginUser(bool steamchina)
    {
        var registryVdfPath = GetUnixRegistryVdfFilePath();
        if (!string.IsNullOrWhiteSpace(registryVdfPath))
        {
            var v = VdfHelper.Read(registryVdfPath);
            var steamItem = v.GetCollection("HKCU", "Software", "Valve", "Steam");
            if (steamItem != null)
            {
                var kAutoLoginUser = steamchina ? "AutoLoginUser_steamchina" : "AutoLoginUser";
                if (steamItem[kAutoLoginUser] is KVObjectValue<string> kv)
                {
                    return kv.Value;
                }
            }
        }
        return null;
    }

    public static partial void SetAutoLoginUser(string userName, uint? rememberPassword, bool steamchina)
    {
        var registryVdfPath = GetUnixRegistryVdfFilePath();
        if (!string.IsNullOrWhiteSpace(registryVdfPath))
        {
            var v = VdfHelper.Read(registryVdfPath) ?? GetDefaultUnixRegistryVdf();
            var steamItem = v.GetOrCreateCollection("HKCU", "Software", "Valve", "Steam");
            var kAutoLoginUser = steamchina ? "AutoLoginUser_steamchina" : "AutoLoginUser";
            steamItem.AddOrSet(kAutoLoginUser, userName);
            if (rememberPassword.HasValue)
            {
                var vRememberPassword = rememberPassword.Value.ToString();
                const string kRememberPassword = "RememberPassword";
                steamItem.AddOrSet(kRememberPassword, vRememberPassword);
            }
            VdfHelper.Write(registryVdfPath, v);
        }
    }
}
#endif