namespace BD.SteamClient8.Models.Converter;

#pragma warning disable SA1600 // Elements should be documented

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.String"/> => <see cref="int"/>
/// </summary>
public class SteamDataIntConverter : System.Text.Json.Serialization.JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int value))
        {
            return value;
        }

        return 0; // 返回默认值或其他自定义逻辑
    }

    public override void Write(Utf8JsonWriter writer, int value, SystemTextJsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
