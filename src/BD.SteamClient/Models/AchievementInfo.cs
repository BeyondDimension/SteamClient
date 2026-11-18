namespace BD.SteamClient.Models;

public class AchievementInfo : ReactiveObject
{
    public int AppId { get; set; }

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? IconNormal { get; set; }

    public string? IconLocked { get; set; }

    public bool IsHidden { get; set; }

    public bool IsAchieved { get; set; }

    private bool _IsChecked;

    public bool IsChecked
    {
        get => _IsChecked;
        set => this.RaiseAndSetIfChanged(ref _IsChecked, value);
    }

    public long UnlockTimeUnix { get; set; }

    // Steam uses zero for an unavailable unlock time; the upper bound is 9999-12-31T23:59:59Z.
    public bool HasUnlockTime => IsAchieved && UnlockTimeUnix is > 0 and <= 253402300799L;

    public DateTime UnlockTime { get; set; }

    public string? IconUrl
    {
        get
        {
            var icon = IsAchieved || string.IsNullOrEmpty(IconLocked) ? IconNormal : IconLocked;
            return string.IsNullOrEmpty(icon) ? null : string.Format(CultureInfo.InvariantCulture, STEAMAPP_ICON_URL, AppId, icon);
        }
    }

    public int Permission { get; set; }

    public bool IsProtection => (Permission & 3) != 0;

    public AchievementSet? Set { get; init; }

    public bool IsArchived { get; init; }

    public bool CanEdit => !IsProtection && !IsArchived;

    public override string ToString()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0}: {1}",
            Name ?? Id ?? base.ToString(),
            Permission);
    }
}