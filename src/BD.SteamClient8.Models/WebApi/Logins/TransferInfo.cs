using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Logins;

/// <summary>
/// 跳转信息
/// </summary>
public sealed class TransferInfo : JsonModel<TransferInfo>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 跳转域名地址
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 跳转参数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("params")]
    public TransferInfoParams? Params { get; set; }
}
