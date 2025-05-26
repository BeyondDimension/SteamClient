using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// <see cref="SteamApp"/> Collection Model
/// </summary>
public sealed class SteamAppList : JsonModel<SteamAppList>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="SteamApp"/> Collection
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("apps")]
    public List<SteamApp>? Apps { get; set; }
}