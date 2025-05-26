using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// <see cref="SteamAppList"/> Model
/// </summary>
public sealed class SteamApps : JsonModel<SteamApps>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="SteamAppList"/> Instance
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("applist")]
    public SteamAppList? AppList { get; set; }
}