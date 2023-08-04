namespace BD.SteamClient.Services;

public interface ISteamSessionService
{
    static ISteamSessionService Instance => Ioc.Get<ISteamSessionService>();

    bool AddOrSetSeesion(SteamSession steamSession);

    SteamSession? RentSession(string steam_id);

    bool RemoveSession(string steam_id);
}
