using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.String"/> => <see cref="int"/>
/// </summary>
public sealed class SteamDataInt32Converter : global::System.Text.Json.Serialization.JsonConverter<int>
{
    /// <inheritdoc/>
    public sealed override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int value))
        {
            return value;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }

        return 0; // 返回默认值或其他自定义逻辑
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
