#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略

namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.String"/> => <see cref="long"/>
/// </summary>
public sealed class SteamDataInt64Converter : System.Text.Json.Serialization.JsonConverter<long>
{
    /// <inheritdoc/>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
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
    public override void Write(Utf8JsonWriter writer, long value, SystemTextJsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}