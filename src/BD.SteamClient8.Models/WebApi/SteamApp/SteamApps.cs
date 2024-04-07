namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamAppList"/> Model
/// </summary>
public class SteamApps
{
    /// <summary>
    /// <see cref="SteamAppList"/> Instance
    /// </summary>
    [SystemTextJsonProperty("applist")]
    public SteamAppList? AppList { get; set; }
}