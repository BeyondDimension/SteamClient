namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public sealed class SteamSyncStruct
{
    [SystemTextJsonProperty("response")]
    public SteamSyncResponseStruct? Response { get; set; }
}

public sealed class SteamSyncResponseStruct
{
    [SystemTextJsonProperty("server_time")]
    public string ServerTime { get; set; } = string.Empty;

    [SystemTextJsonProperty("skew_tolerance_seconds")]
    public string SkewToleranceSeconds { get; set; } = string.Empty;

    [SystemTextJsonProperty("large_time_jink")]
    public string LargeTimeJink { get; set; } = string.Empty;

    [SystemTextJsonProperty("probe_frequency_seconds")]
    public int ProbeFrequencySeconds { get; set; }

    [SystemTextJsonProperty("adjusted_time_probe_frequency_seconds")]
    public int AdjustedTimeProbeFrequencySeconds { get; set; }

    [SystemTextJsonProperty("hint_probe_frequency_seconds")]
    public int HintProbeFrequencySeconds { get; set; }

    [SystemTextJsonProperty("sync_timeout")]
    public int SyncTimeOut { get; set; }

    [SystemTextJsonProperty("try_again_seconds")]
    public int TryAgainSeconds { get; set; }

    [SystemTextJsonProperty("max_attempts")]
    public int MaxAttempts { get; set; }
}
