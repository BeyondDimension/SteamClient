namespace BD.SteamClient8.Models.WebApi.SteamApp;

#pragma warning disable SA1600 // Elements should be documented

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