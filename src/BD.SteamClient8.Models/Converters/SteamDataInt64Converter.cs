using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.String"/> => <see cref="long"/>
/// </summary>
public sealed class SteamDataInt64Converter : global::System.Text.Json.Serialization.JsonConverter<long>
{
    /// <inheritdoc/>
    public sealed override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 处理本就是数字的情况
        if (reader.TokenType == JsonTokenType.Number)
        {
            // 暂不处理无法转换为 Int64 的情况
            return reader.TryGetInt64(out long parseInt64) ? parseInt64 : default;
        }

        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out long value))
        {
            return value;
        }

        return 0; // 返回默认值或其他自定义逻辑
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}