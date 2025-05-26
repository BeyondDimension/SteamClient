using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

public sealed record PlayerSummariesResponse : JsonRecordModel<PlayerSummariesResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    [global::Newtonsoft.Json.JsonProperty("response")]
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public required PlayerSummariesDetail Response { get; set; }

    public sealed record PlayerSummariesDetail : JsonRecordModel<PlayerSummariesDetail>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        [global::Newtonsoft.Json.JsonProperty("players")]
        [global::System.Text.Json.Serialization.JsonPropertyName("players")]
        public required List<PlayerSummaries> Players { get; set; }
    }
}
