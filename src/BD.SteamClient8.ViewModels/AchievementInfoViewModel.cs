namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600
/// <summary>
/// <see cref="AchievementInfoViewModel"/> 模型绑定类型
/// </summary>
public class AchievementInfoViewModel : ReactiveObject
{
    /// <summary>
    /// 游戏 AppId
    /// </summary>
    public int AppId { get; set; }

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 完成百分比
    /// </summary>
    public float Percent { get; set; }

    /// <summary>
    /// 成就名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图标正常
    /// </summary>
    public string? IconNormal { get; set; }

    /// <summary>
    /// 未解锁图标
    /// </summary>
    public string? IconLocked { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// 是否已达成
    /// </summary>
    public bool IsAchieved { get; set; }

    private bool _IsChecked;

    public bool IsChecked
    {
        get => _IsChecked;
        set => this.RaiseAndSetIfChanged(ref _IsChecked, value);
    }

    /// <summary>
    /// 解锁时间时间戳
    /// </summary>
    public long UnlockTimeUnix { get; set; }

    /// <summary>
    /// 解锁时间
    /// </summary>
    public DateTime UnlockTime { get; set; }

    /// <summary>
    /// 图标 Url
    /// </summary>
    public string IconUrl => string.Format(
        SteamApiUrls.STEAMAPP_ICON_URL,
        AppId,
        IsAchieved ? IconNormal : IconLocked);

    //public Task<string?> IconNormalStream => IHttpService.Instance.GetImageAsync(string.Format(
    //    STEAMAPP_ICON_URL,
    //    AppId, IconNormal), ImageChannelType.SteamAchievementIcon);
    //public Task<string?> IconLockedStream => IHttpService.Instance.GetImageAsync(string.Format(
    //    STEAMAPP_ICON_URL,
    //    AppId, IconLocked), ImageChannelType.SteamAchievementIcon);

    //public Task<ImageSource.ClipStream?> IconStream => ImageSource.GetAsync(IconUrl);

    public int Permission { get; set; }

    public bool IsProtection => (Permission & 3) != 0;

    public override string ToString()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0}: {1}",
            Name ?? Id ?? base.ToString(),
            Permission);
    }

    /// <summary>
    /// <see cref="AchievementInfo"/> 隐式转换 <see cref="AchievementInfoViewModel"/>
    /// </summary>
    /// <param name="achievementInfo"></param>
    public static implicit operator AchievementInfoViewModel(AchievementInfo achievementInfo) => new(achievementInfo);

    /// <summary>
    /// <see cref="AchievementInfo"/> 构造实例 <see cref="AchievementInfoViewModel"/>
    /// </summary>
    /// <param name="achievementInfo"></param>
    public AchievementInfoViewModel(AchievementInfo achievementInfo)
    {
        AppId = achievementInfo.AppId;
        Id = achievementInfo.Id;
        Name = achievementInfo.Name;
        Percent = achievementInfo.Percent;
        Description = achievementInfo.Description;
        IconNormal = achievementInfo.IconNormal;
        IconLocked = achievementInfo.IconLocked;
        IsHidden = achievementInfo.IsHidden;
        IsAchieved = achievementInfo.IsAchieved;
        IsChecked = achievementInfo.IsChecked;
        UnlockTimeUnix = achievementInfo.UnlockTimeUnix;
        Permission = achievementInfo.Permission;
    }
}
