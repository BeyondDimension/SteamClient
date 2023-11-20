namespace BD.SteamClient8.Models.WebApi.SteamApp;

#pragma warning disable SA1600
public class SteamAppLaunchItem
{
    public string? Label { get; set; }

    public string? Executable { get; set; }

    public string? Arguments { get; set; }

    public string? WorkingDir { get; set; }

    public string? Platform { get; set; }
}
