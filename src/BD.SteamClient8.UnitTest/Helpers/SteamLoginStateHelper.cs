namespace BD.SteamClient8.UnitTest.Helpers;

#pragma warning disable SA1600 // Elements should be documented

public static partial class SteamLoginStateHelper
{
    const string steamLoginStateCacheFileName = "SteamLoginState.mpo";

    static SteamLoginState? steamLoginState;
    static readonly AsyncExclusiveLock lock_GetSteamLoginStateAsync = new();

    public static async ValueTask<SteamLoginState> GetSteamLoginStateAsync(
        IConfiguration configuration,
        ISteamAccountService steamAccountService,
        ISteamSessionService steamSession)
    {
        if (steamLoginState == null)
        {
            using (await lock_GetSteamLoginStateAsync.AcquireLockAsync(CancellationToken.None))
            {
                var steamLoginStateCacheFilePath = Path.Combine(ProjPath,
                    "..", steamLoginStateCacheFileName);
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
                        throw new ArgumentException();

                    var session = new SteamSession();
                    session.SteamId = steamLoginState.SteamId.ToString();
                    session.AccessToken = steamLoginState.AccessToken!;
                    session.RefreshToken = steamLoginState.RefreshToken!;
                    session.Cookies.Add(steamLoginState.Cookies!);
                    steamSession.AddOrSetSession(session);
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
            }
        }

        return steamLoginState;
    }
}
