namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.Number"/> 数字类型 => <see cref="string"/>
/// </summary>
public sealed class SteamDataStringConverter : System.Text.Json.Serialization.JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && typeToConvert == typeof(string) && reader.TryGetDecimal(out var value))
        {
            return value.ToString();
        }

        return string.Empty; // 返回默认值或其他自定义逻辑
    }

    public override void Write(Utf8JsonWriter writer, string value, SystemTextJsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
