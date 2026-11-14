namespace BD.SteamClient.Models.ModelConverters;

internal class DecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? readStr = Encoding.Default.GetString(reader.ValueSpan);

        return decimal.TryParse(readStr, out decimal parsedResult) ? parsedResult : 0;
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
