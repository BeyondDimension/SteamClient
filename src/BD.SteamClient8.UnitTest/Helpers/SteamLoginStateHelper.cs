global using static BD.SteamClient8.UnitTest.Helpers.SteamLoginStateHelper;

namespace BD.SteamClient8.UnitTest.Helpers;

static partial class SteamLoginStateHelper
{
    const string steamLoginStateCacheFileName = "SteamLoginState.mpo";

    static SteamLoginState? steamLoginState;
    static readonly AsyncExclusiveLock lock_GetSteamLoginStateAsync = new();
    static readonly string steamLoginStateCacheFilePath = Path.Combine(DataPath, steamLoginStateCacheFileName);

    public static SteamLoginState SteamLoginState => steamLoginState.ThrowIsNull();

    public static string ApiKey { get; private set; } = string.Empty;

    public static async ValueTask<SteamLoginState> GetSteamLoginStateAsync(
        IConfiguration configuration,
        ISteamAccountService steamAccountService,
        ISteamSessionService steamSession,
        ISteamAuthenticatorService steamAuthenticatorService)
    {
        using (await lock_GetSteamLoginStateAsync.AcquireLockAsync(CancellationToken.None))
        {
            if (steamLoginState == null)
            {
                try
                {
                    byte[] steamLoginStateCache = File.ReadAllBytes(steamLoginStateCacheFilePath);
                    if (OperatingSystem.IsWindows())
                        steamLoginStateCache = ProtectedData.Unprotect(
                            steamLoginStateCache, null, DataProtectionScope.LocalMachine);
                    steamLoginState = Serializable.DMP2<SteamLoginState>(steamLoginStateCache);
                    steamLoginState.ThrowIsNull();
                    var check = await steamAccountService.CheckAccessTokenValidation(steamLoginState.AccessToken!);
                    if (steamLoginState.Username != configuration["steamUsername"] || !check.Content)
                        throw ThrowHelper.GetArgumentOutOfRangeException(steamLoginState.Username);

                    var session = new SteamSession
                    {
                        SteamId = steamLoginState.SteamId.ToString(),
                        AccessToken = steamLoginState.AccessToken!,
                        RefreshToken = steamLoginState.RefreshToken!,
                    };
                    session.Cookies.Add(steamLoginState.Cookies!);
                    session.GenerateSetCookie();
                    await steamSession.AddOrSetSession(session);
                }
                catch
                {
                    steamLoginState = new()
                    {
                        Username = configuration["steamUsername"],
                        Password = configuration["steamPassword"],
                    };
                    await steamAccountService.DoLoginV2Async(steamLoginState);
                    if (!steamLoginState.Success && (steamLoginState.Requires2FA || steamLoginState.RequiresEmailAuth))
                    {
                        if (steamLoginState.Requires2FA)
                        {
                            steamLoginState.TwofactorCode = SteamAuthenticatorHelper.SteamAuthenticator?.CurrentCode;
                            // input your TwoFactorCode
                            steamLoginState.TwofactorCode.ThrowIsNull();
                        }
                        else if (steamLoginState.RequiresEmailAuth)
                        {
                            // input your EmailCode
                            steamLoginState.EmailCode.ThrowIsNull();
                        }

                        await steamAccountService.DoLoginV2Async(steamLoginState);
                    }
                    byte[] steamLoginStateCache = Serializable.SMP2(steamLoginState);
                    if (OperatingSystem.IsWindows())
                        steamLoginStateCache = ProtectedData.Protect(
                            steamLoginStateCache, null, DataProtectionScope.LocalMachine);
                    File.WriteAllBytes(steamLoginStateCacheFilePath, steamLoginStateCache);
                }
                finally
                {
                    var identitySecret = configuration["identitySecret"];
                    long serverTimeDiff;
                    var authenticator = SteamAuthenticatorHelper.SteamAuthenticator;
                    // 本地令牌获取相关信息
                    if (identitySecret is null && authenticator is not null)
                    {
                        var steamData = SystemTextJsonSerializer.Deserialize<SteamConvertSteamDataJsonStruct>(authenticator.ThrowIsNull().SteamData!);
                        identitySecret = steamData!.IdentitySecret;
                    }

                    if (authenticator is null)
                    {
                        var serverTime = (await steamAuthenticatorService.TwoFAQueryTime())?.Content?.Response?.ServerTime.ThrowIsNull();
                        serverTimeDiff = (long.Parse(serverTime!) * 1000L) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    }
                    else
                    {
                        serverTimeDiff = authenticator.ServerTimeDiff;
                    }

                    var sessionRsp = await steamSession.RentSession(steamLoginState.ThrowIsNull().SteamId.ToString());
                    var session = sessionRsp.Content;
                    if (session is not null)
                    {
                        session.ServerTimeDiff = serverTimeDiff;
                        session.IdentitySecret = identitySecret.ThrowIsNull();
                        var apiKey = (await steamAccountService.GetApiKey(steamLoginState)).Content;
                        session.APIKey = ApiKey = string.IsNullOrEmpty(apiKey) ? (await steamAccountService.RegisterApiKey(steamLoginState)).Content.ThrowIsNull() : apiKey;
                        await steamSession.AddOrSetSession(session);
                    }
                }
            }
        }

        return steamLoginState;
    }

    public static ValueTask<SteamLoginState> GetSteamLoginStateAsync()
    {
        var configuration = Ioc.Get<IConfiguration>();
        var steamAccountService = Ioc.Get<ISteamAccountService>();
        var steamSessionService = Ioc.Get<ISteamSessionService>();
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        return GetSteamLoginStateAsync(configuration, steamAccountService, steamSessionService, steamAuthenticatorService);
    }
}
