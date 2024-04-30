namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// TwoFAQueryTime 接口返回模型类
/// </summary>
public sealed class SteamSyncStruct
{
    /// <summary>
    /// <see cref="SteamSyncStruct"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public SteamSyncResponseStruct? Response { get; set; }
}

/// <summary>
/// <see cref="SteamSyncStruct.Response"/> 详细信息
/// </summary>
public sealed class SteamSyncResponseStruct
{
    /// <summary>
    /// 服务器时间 （秒）
    /// </summary>
    [SystemTextJsonProperty("server_time")]
    public string ServerTime { get; set; } = string.Empty;

    /// <summary>
    /// 允许的客户端与服务器时间最大偏差秒数  （秒）
    /// </summary>
    [SystemTextJsonProperty("skew_tolerance_seconds")]
    public string SkewToleranceSeconds { get; set; } = string.Empty;

    /// <summary>
    /// 客户端与服务器时间时间戳的 偏差值  （秒）
    /// </summary>
    [SystemTextJsonProperty("large_time_jink")]
    public string LargeTimeJink { get; set; } = string.Empty;

    /// <summary>
    /// 请求服务器时间的频率间隔，多久校准一次 （秒）
    /// </summary>
    [SystemTextJsonProperty("probe_frequency_seconds")]
    public int ProbeFrequencySeconds { get; set; }

    /// <summary>
    /// 调整后的 <see cref="ProbeFrequencySeconds"/>  （秒）
    /// </summary>
    [SystemTextJsonProperty("adjusted_time_probe_frequency_seconds")]
    public int AdjustedTimeProbeFrequencySeconds { get; set; }

    /// <summary>
    /// 提示请求服务器时间的频率 （秒）
    /// </summary>
    [SystemTextJsonProperty("hint_probe_frequency_seconds")]
    public int HintProbeFrequencySeconds { get; set; }

    /// <summary>
    /// 同步超时时间（秒）
    /// </summary>
    [SystemTextJsonProperty("sync_timeout")]
    public int SyncTimeOut { get; set; }

    /// <summary>
    /// 重试间隔时间（秒）
    /// </summary>
    [SystemTextJsonProperty("try_again_seconds")]
    public int TryAgainSeconds { get; set; }

    /// <summary>
    /// 最大尝试次数
    /// </summary>
    [SystemTextJsonProperty("max_attempts")]
    public int MaxAttempts { get; set; }
}
