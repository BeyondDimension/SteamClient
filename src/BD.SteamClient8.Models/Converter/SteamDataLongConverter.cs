namespace BD.SteamClient8.Models.Converter;

#pragma warning disable SA1600 // Elements should be documented

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.String"/> => <see cref="long"/>
/// </summary>
public class SteamDataLongConverter : System.Text.Json.Serialization.JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out long value))
        {
            return value;
        }

        return 0; // 返回默认值或其他自定义逻辑
    }

    public override void Write(Utf8JsonWriter writer, long value, SystemTextJsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
