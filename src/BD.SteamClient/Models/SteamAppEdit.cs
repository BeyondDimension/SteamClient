#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Text.Json.Serialization;

namespace BD.SteamClient.Models;

/// <summary>Only overridden fields are present. Empty values explicitly clear a field.</summary>
public sealed record SteamAppEdit
{
    public string? Name { get; init; }

    public string? SortAs { get; init; }

    public string? Developer { get; init; }

    public string? Publisher { get; init; }

    public SteamLaunchEdit[]? LaunchItems { get; init; }

    [JsonIgnore]
    public bool IsEmpty => Name == null && SortAs == null && Developer == null && Publisher == null && LaunchItems == null;

    public static SteamAppEdit Capture(SteamApp app) => new()
    {
        Name = app.Name ?? string.Empty,
        SortAs = app.SortAs ?? string.Empty,
        Developer = app.Developer ?? string.Empty,
        Publisher = app.Publisher ?? string.Empty,
        LaunchItems = app.LaunchItems is { Count: > 0 } items ? items.Select(SteamLaunchEdit.Capture).ToArray() : [],
    };

    public SteamAppEdit Diff(SteamAppEdit baseline) => new()
    {
        Name = Name == baseline.Name ? null : Name,
        SortAs = SortAs == baseline.SortAs ? null : SortAs,
        Developer = Developer == baseline.Developer ? null : Developer,
        Publisher = Publisher == baseline.Publisher ? null : Publisher,
        LaunchItems = LaunchItems == null || LaunchItems.AsSpan().SequenceEqual(baseline.LaunchItems) ? null : LaunchItems,
    };

    public SteamAppEdit WithOverrides(SteamAppEdit? edit) => edit == null ? this : new()
    {
        Name = edit.Name ?? Name,
        SortAs = edit.SortAs ?? SortAs,
        Developer = edit.Developer ?? Developer,
        Publisher = edit.Publisher ?? Publisher,
        LaunchItems = edit.LaunchItems ?? LaunchItems,
    };

    public void ApplyTo(SteamApp app)
    {
        var sortAs = SortAs ?? app.SortAs;
        if (Name != null)
            app.Name = Name;
        app.SortAs = sortAs;
        if (Developer != null)
            app.Developer = Developer;
        if (Publisher != null)
            app.Publisher = Publisher;
        if (LaunchItems != null)
            app.LaunchItems = LaunchItems.Length == 0 ? null : new(LaunchItems.Select(static item => item.ToLaunchItem()));
    }

    internal SteamAppEdit OriginalFields(SteamApp app) => new()
    {
        Name = Name == null ? null : app.Name ?? string.Empty,
        SortAs = SortAs == null ? null : app.SortAs ?? string.Empty,
        Developer = Developer == null ? null : app.Developer ?? string.Empty,
        Publisher = Publisher == null ? null : app.Publisher ?? string.Empty,
        LaunchItems = LaunchItems == null ? null : app.LaunchItems is { Count: > 0 } items ? items.Select(SteamLaunchEdit.Capture).ToArray() : [],
    };
}

/// <summary>Serializable launch fields, without UI state or executable handles.</summary>
public readonly record struct SteamLaunchEdit(string? Label, string? Executable, string? Arguments, string? WorkingDir, string? Platform)
{
    internal static SteamLaunchEdit Capture(SteamAppLaunchItem item) => new(item.Label, item.Executable, item.Arguments, item.WorkingDir, item.Platform);

    internal SteamAppLaunchItem ToLaunchItem() => new()
    {
        Label = Label,
        Executable = Executable,
        Arguments = Arguments,
        WorkingDir = WorkingDir,
        Platform = Platform,
    };
}
#endif
