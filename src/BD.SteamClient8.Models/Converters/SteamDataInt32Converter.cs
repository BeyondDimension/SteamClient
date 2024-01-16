#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.String"/> => <see cref="int"/>
/// </summary>
public sealed class SteamDataInt32Converter : System.Text.Json.Serialization.JsonConverter<int>
{
    /// <inheritdoc/>
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
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
    public override void Write(Utf8JsonWriter writer, int value, SystemTextJsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
