using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.WinAuth.Models;

/// <summary>
/// TwoFAQueryTime 接口返回模型类
/// </summary>
public sealed class SteamSyncStruct : JsonModel<SteamSyncStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="SteamSyncStruct"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public SteamSyncResponseStruct? Response { get; set; }
}

/// <summary>
/// <see cref="SteamSyncStruct.Response"/> 详细信息
/// </summary>
public sealed class SteamSyncResponseStruct : JsonModel<SteamSyncResponseStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 服务器时间 （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("server_time")]
    public string ServerTime { get; set; } = string.Empty;

    /// <summary>
    /// 允许的客户端与服务器时间最大偏差秒数  （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("skew_tolerance_seconds")]
    public string SkewToleranceSeconds { get; set; } = string.Empty;

    /// <summary>
    /// 客户端与服务器时间时间戳的 偏差值  （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("large_time_jink")]
    public string LargeTimeJink { get; set; } = string.Empty;

    /// <summary>
    /// 请求服务器时间的频率间隔，多久校准一次 （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("probe_frequency_seconds")]
    public int ProbeFrequencySeconds { get; set; }

    /// <summary>
    /// 调整后的 <see cref="ProbeFrequencySeconds"/>  （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("adjusted_time_probe_frequency_seconds")]
    public int AdjustedTimeProbeFrequencySeconds { get; set; }

    /// <summary>
    /// 提示请求服务器时间的频率 （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("hint_probe_frequency_seconds")]
    public int HintProbeFrequencySeconds { get; set; }

    /// <summary>
    /// 同步超时时间（秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("sync_timeout")]
    public int SyncTimeOut { get; set; }

    /// <summary>
    /// 重试间隔时间（秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("try_again_seconds")]
    public int TryAgainSeconds { get; set; }

    /// <summary>
    /// 最大尝试次数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("max_attempts")]
    public int MaxAttempts { get; set; }
}
