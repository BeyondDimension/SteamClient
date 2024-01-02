namespace BD.SteamClient8.UnitTest.Helpers;

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

public static partial class SteamAuthenticatorHelper
{
    const string steamAuthenticatorCacheFileName = "SteamAuthenticator.mpo";

    public static SteamAuthenticator? SteamAuthenticator;
    static readonly AsyncExclusiveLock lock_GetSteamAuthenticatorAsync = new();

    public static string ApiKey { get; private set; } = string.Empty;

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
                    var steamAuthenticatorCacheFilePath = Path.Combine(DataPath,
                        steamAuthenticatorCacheFileName);
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
                finally
                {
                    var identitySecret = configuration["identitySecret"];
                    long serverTimeDiff;
                    // 本地令牌获取相关信息
                    if (identitySecret is null)
                    {
                        var steamData = SystemTextJsonSerializer.Deserialize<SteamConvertSteamDataJsonStruct>(SteamAuthenticator.ThrowIsNull().SteamData!);
                        identitySecret = steamData!.IdentitySecret;
                    }
                    if (SteamAuthenticator is null)
                    {
                        var serverTime = (await steamAuthenticatorService.TwoFAQueryTime())?.Content?.Response?.ServerTime.ThrowIsNull();
                        serverTimeDiff = (long.Parse(serverTime!) * 1000L) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    }
                    else
                    {
                        serverTimeDiff = SteamAuthenticator.ServerTimeDiff;
                    }

                    var steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService, steamSessionService);
                    var sessionRsp = await steamSessionService.RentSession(steamLoginState.ThrowIsNull().SteamId.ToString());
                    var session = sessionRsp.Content;
                    if (session is not null)
                    {
                        session.ServerTimeDiff = serverTimeDiff;
                        session.IdentitySecret = identitySecret;
                        session.APIKey = ApiKey = (await steamAccountService.GetApiKey(steamLoginState)).Content ?? (await steamAccountService.RegisterApiKey(steamLoginState)).Content.ThrowIsNull();
                        await steamSessionService.AddOrSetSession(session);
                    }
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
