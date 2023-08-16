namespace BD.SteamClient.Models.ModelConverters;

internal class DecimalConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? readStr = Encoding.Default.GetString(reader.ValueSpan);

        return decimal.TryParse(readStr, out decimal parsedResult) ? parsedResult : 0;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        string strValue = value?.ToString() ?? string.Empty;

        if (decimal.TryParse(strValue, out decimal parsedResult))
        {
            writer.WriteNumberValue(parsedResult);
        }
        else
        {
            writer.WriteNumberValue(0m);
        }
    }
}
