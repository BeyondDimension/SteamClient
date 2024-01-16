#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models.Converters;

/// <summary>
/// Steam API 数据 <see cref="JsonTokenType.Number"/> 数字类型 => <see cref="string"/>
/// </summary>
public sealed class SteamDataStringConverter : System.Text.Json.Serialization.JsonConverter<string>
{
    /// <inheritdoc/>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && typeToConvert == typeof(string) && reader.TryGetDecimal(out var value))
        {
            return value.ToString();
        }

        return string.Empty; // 返回默认值或其他自定义逻辑
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, string value, SystemTextJsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
