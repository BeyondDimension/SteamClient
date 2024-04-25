namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamAppList"/> Model
/// </summary>
public sealed class SteamApps : JsonModel<SteamApps>
{
    /// <summary>
    /// <see cref="SteamAppList"/> Instance
    /// </summary>
    [SystemTextJsonProperty("applist")]
    public SteamAppList? AppList { get; set; }
}