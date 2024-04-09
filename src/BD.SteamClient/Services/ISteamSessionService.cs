namespace BD.SteamClient.Services;

public interface ISteamSessionService
{
    public const string CurrentSteamUserKey = "CurrentSteamUser";

    static ISteamSessionService Instance => Ioc.Get<ISteamSessionService>();

    bool AddOrSetSeesion(SteamSession steamSession);

    SteamSession? RentSession(string steam_id);

    Task<SteamSession?> LoadSession();

    Task<bool> SaveSession(SteamSession steamSession);

    bool RemoveSession(string steam_id);

    /// <summary>
    /// 解除家庭监控
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="pinCode"></param>
    /// <returns></returns>
    Task UnlockParental(string steam_id, string pinCode);

    /// <summary>
    /// 恢复家庭监控
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task LockParental(string steam_id);
}
