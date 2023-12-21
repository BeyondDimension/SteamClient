#pragma warning disable SA1600 // Elements should be documented

namespace BD.SteamClient8.UnitTest.Helpers;

public static partial class SteamAuthenticatorHelper
{
    const string steamAuthenticatorCacheFileName = "SteamAuthenticator.mpo";

    static SteamAuthenticator? steamAuthenticator;
    static readonly AsyncExclusiveLock lock_GetSteamAuthenticatorAsync = new();

    public static async ValueTask<SteamAuthenticator?> GetSteamAuthenticatorAsync()
    {
        if (steamAuthenticator == null)
        {
            using (await lock_GetSteamAuthenticatorAsync.AcquireLockAsync(CancellationToken.None))
            {
                var steamAuthenticatorCacheFilePath = Path.Combine(ProjPath,
                    "..", steamAuthenticatorCacheFileName);
                try
                {
                    byte[] steamAuthenticatorCache = File.ReadAllBytes(steamAuthenticatorCacheFilePath);
                    if (OperatingSystem.IsWindows())
                        steamAuthenticatorCache = ProtectedData.Unprotect(
                            steamAuthenticatorCache, null, DataProtectionScope.LocalMachine);
                    steamAuthenticator = Serializable.DJSON<SteamAuthenticator>(Serializable.JsonImplType.SystemTextJson, Encoding.UTF8.GetString(steamAuthenticatorCache));
                    steamAuthenticator.ThrowIsNull();
                }
                catch
                {
                    return null;
                }
            }
        }
        return steamAuthenticator;
    }

    public static async ValueTask<bool> SaveSteamAuthenticatorAsync(SteamAuthenticator steamAuthenticator)
    {
        using (await lock_GetSteamAuthenticatorAsync.AcquireLockAsync(CancellationToken.None))
        {
            var steamAuthenticatorCacheFilePath = Path.Combine(ProjPath,
                "..", steamAuthenticatorCacheFileName);
            try
            {
                byte[] steamAuthenticatorCache = Encoding.UTF8.GetBytes(Serializable.SJSON(Serializable.JsonImplType.SystemTextJson, steamAuthenticator));
                if (OperatingSystem.IsWindows())
                    steamAuthenticatorCache = ProtectedData.Protect(
                        steamAuthenticatorCache, null, DataProtectionScope.LocalMachine);
                File.WriteAllBytes(steamAuthenticatorCacheFilePath, steamAuthenticatorCache);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static async ValueTask<bool> DeleteSteamAuthenticatorAsync()
    {
        using (await lock_GetSteamAuthenticatorAsync.AcquireLockAsync(CancellationToken.None))
        {
            var steamAuthenticatorCacheFilePath = Path.Combine(ProjPath,
                "..", steamAuthenticatorCacheFileName);
            try
            {
                File.Delete(steamAuthenticatorCacheFilePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
