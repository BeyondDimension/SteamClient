namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamApp"/> Collection Model
/// </summary>
public sealed class SteamAppList : JsonModel<SteamAppList>
{
    /// <summary>
    /// <see cref="SteamApp"/> Collection
    /// </summary>
    [SystemTextJsonProperty("apps")]
    public List<SteamApp>? Apps { get; set; }
}