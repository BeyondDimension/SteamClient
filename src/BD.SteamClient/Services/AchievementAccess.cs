namespace BD.SteamClient.Services;

/// <summary>Checks the current session's entitlement once per DLC in a read or write batch.</summary>
public sealed class AchievementAccess(ISteamworksLocalApiService steam)
{
    Dictionary<uint, bool>? dlcOwnership;

    public bool OwnsRequiredDlc(AchievementInfo achievement)
    {
        if (achievement.Set is not { DlcAppId: > 0 } set)
            return true;

        dlcOwnership ??= new();
        if (!dlcOwnership.TryGetValue(set.DlcAppId, out var owned))
        {
            try
            {
                owned = steam.OwnsApps(set.DlcAppId);
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(AchievementAccess), ex, $"Unable to verify ownership of Steam DLC {set.DlcAppId}.");
                owned = false;
            }
            dlcOwnership.Add(set.DlcAppId, owned);
        }
        return owned;
    }

    /// <summary>Checks the complete batch before making any native achievement change.</summary>
    public AchievementWriteResult StageChanges(IReadOnlyList<(AchievementInfo Info, bool IsChecked)> changes,
        out AchievementInfo? failedAchievement)
    {
        foreach (var (info, _) in changes)
        {
            if (!info.CanEdit || string.IsNullOrEmpty(info.Id) || !OwnsRequiredDlc(info))
            {
                failedAchievement = info;
                return AchievementWriteResult.Restricted;
            }
        }

        foreach (var (info, isChecked) in changes)
        {
            if (!steam.SetAchievement(info.Id!, isChecked))
            {
                failedAchievement = info;
                return AchievementWriteResult.Rejected;
            }
        }

        failedAchievement = null;
        return AchievementWriteResult.Staged;
    }
}

public enum AchievementWriteResult
{
    Staged,
    Restricted,
    Rejected,
}
