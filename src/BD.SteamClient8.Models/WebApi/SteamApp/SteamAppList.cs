namespace BD.SteamClient8.Models.WebApi.SteamApp;

/// <summary>
/// <see cref="SteamApp"/> Collection Model
/// </summary>
public class SteamAppList
{
    /// <summary>
    /// <see cref="SteamApp"/> Collection
    /// </summary>
    [SystemTextJsonProperty("apps")]
    public List<SteamApp>? Apps { get; set; }
}