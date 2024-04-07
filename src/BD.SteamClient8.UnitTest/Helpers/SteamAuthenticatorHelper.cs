global using static BD.SteamClient8.UnitTest.Helpers.SteamAuthenticatorHelper;

namespace BD.SteamClient8.UnitTest.Helpers;

static partial class SteamAuthenticatorHelper
{
    const string steamAuthenticatorCacheFileName = "SteamAuthenticator.mpo";

    public static SteamAuthenticator? SteamAuthenticator;
    static readonly AsyncExclusiveLock lock_GetSteamAuthenticatorAsync = new();

    static readonly string steamAuthenticatorCacheFilePath = Path.Combine(DataPath, steamAuthenticatorCacheFileName);

    public static async ValueTask<SteamAuthenticator?> GetSteamAuthenticatorAsync(
        IConfiguration configuration,
        ISteamAuthenticatorService steamAuthenticatorService,
        ISteamSessionService steamSessionService,
        ISteamAccountService steamAccountService)
    {
        using (await lock_GetSteamAuthenticatorAsync.AcquireLockAsync(CancellationToken.None))
        {
            if (SteamAuthenticator == null)
            {
                try
                {
                    byte[] steamAuthenticatorCache = File.ReadAllBytes(steamAuthenticatorCacheFilePath);
                    if (OperatingSystem.IsWindows())
                        steamAuthenticatorCache = ProtectedData.Unprotect(
                            steamAuthenticatorCache, null, DataProtectionScope.LocalMachine);
                    SteamAuthenticator = Serializable.DJSON<SteamAuthenticator>(Serializable.JsonImplType.SystemTextJson, Encoding.UTF8.GetString(steamAuthenticatorCache));
                    SteamAuthenticator.ThrowIsNull();
                }
                catch
                {
                    var maFilePath = configuration["maFilePath"];
                    if (!File.Exists(maFilePath))
                        return null;

                    var maFile_json = await File.ReadAllTextAsync(maFilePath);
                    var steamData = SystemTextJsonSerializer.Deserialize(maFile_json, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct).ThrowIsNull();
                    var serverTime = (await steamAuthenticatorService.TwoFAQueryTime())?.Content?.Response?.ServerTime.ThrowIsNull();
                    var serverTimeDiff = (long.Parse(serverTime!) * 1000L) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    SteamAuthenticator = new()
                    {
                        SteamData = maFile_json,
                        LastServerTime = long.Parse(serverTime!),
                        ServerTimeDiff = serverTimeDiff,
                        SecretKey = Base64Extensions.Base64DecodeToByteArray_Nullable(steamData.SharedSecret),
                    };
                }
            }
        }
        return SteamAuthenticator;
    }

    public static ValueTask<SteamAuthenticator?> GetSteamAuthenticatorAsync()
    {
        var configuration = Ioc.Get<IConfiguration>();
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var steamSessionService = Ioc.Get<ISteamSessionService>();
        var steamAccountService = Ioc.Get<ISteamAccountService>();
        return GetSteamAuthenticatorAsync(configuration, steamAuthenticatorService, steamSessionService, steamAccountService);
    }

    public static async ValueTask<bool> SaveSteamAuthenticatorAsync(SteamAuthenticator steamAuthenticator)
    {
        using (await lock_GetSteamAuthenticatorAsync.AcquireLockAsync(CancellationToken.None))
        {
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
