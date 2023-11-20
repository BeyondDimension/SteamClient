namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600
public class SteamAppLaunchItemViewModel : ReactiveObject
{
    public string? Label { get; set; }

    public string? Executable { get; set; }

    public string? Arguments { get; set; }

    public string? WorkingDir { get; set; }

    public string? Platform { get; set; }

    /// <summary>
    /// <see cref="SteamAppLaunchItem"/> 隐式转换 <see cref="SteamAppLaunchItemViewModel"/>
    /// </summary>
    /// <param name="steamAppLaunchItem"></param>
    public static implicit operator SteamAppLaunchItemViewModel(SteamAppLaunchItem steamAppLaunchItem) => new(steamAppLaunchItem);

    /// <summary>
    /// <see cref="SteamAppLaunchItem"/> 构造实例 <see cref="SteamAppLaunchItemViewModel"/>
    /// </summary>
    /// <param name="steamAppLaunchItem"></param>
    public SteamAppLaunchItemViewModel(SteamAppLaunchItem steamAppLaunchItem)
    {
        Label = steamAppLaunchItem.Label;
        Executable = steamAppLaunchItem.Executable;
        Arguments = steamAppLaunchItem.Arguments;
        WorkingDir = steamAppLaunchItem.WorkingDir;
        Platform = steamAppLaunchItem.Platform;
    }
}
