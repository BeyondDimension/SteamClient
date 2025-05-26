using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.Number"/> 数字类型 => <see cref="string"/>
/// </summary>
public sealed class SteamDataStringConverter : global::System.Text.Json.Serialization.JsonConverter<string>
{
    /// <inheritdoc/>
    public sealed override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && typeToConvert == typeof(string) && reader.TryGetDecimal(out var value))
        {
            return value.ToString();
        }

        return string.Empty; // 返回默认值或其他自定义逻辑
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
