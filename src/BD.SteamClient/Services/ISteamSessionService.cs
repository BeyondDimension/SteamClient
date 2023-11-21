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
}
