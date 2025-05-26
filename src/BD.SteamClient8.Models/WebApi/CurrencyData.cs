using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 货币信息
/// </summary>
public sealed record class CurrencyData : JsonRecordModel<CurrencyData>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 展示
    /// </summary>
    public string? Display { get; set; }

    /// <summary>
    /// 货币 ISO Code
    /// </summary>
    public string? CurrencyCode { get; set; }
}
