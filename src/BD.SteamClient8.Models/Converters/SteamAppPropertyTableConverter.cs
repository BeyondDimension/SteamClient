#if !(IOS || ANDROID)
using BD.SteamClient8.Models.WebApi.SteamApps;
using System.Text.Json;

namespace BD.SteamClient8.Models.Converters;

sealed class SteamAppPropertyTableConverter : global::System.Text.Json.Serialization.JsonConverter<SteamAppPropertyTable>
{
    /// <inheritdoc/>
    public sealed override SteamAppPropertyTable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize(ref reader, DefaultJsonSerializerContext_.Default.ListSteamAppProperty);
        return list == null ? null : new SteamAppPropertyTable(list);
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, SteamAppPropertyTable? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        List<SteamAppProperty> list = value;
        JsonSerializer.Serialize(writer, list, DefaultJsonSerializerContext_.Default.ListSteamAppProperty);
    }
}
#endif