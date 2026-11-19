#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient.Models;

public static class SteamAppEditBackup
{
    public static IReadOnlyDictionary<uint, SteamAppEdit> Read(Stream source) => ReadEdits(source);

    internal static Dictionary<uint, SteamAppEdit> ReadEdits(Stream source)
    {
        // Current backups are UTF-8 JSON. Previous .stmbak files were MessagePack
        // lists of whole KeyValues records. Decode only the editable values on import.
        if (source.CanSeek)
            return ReadSeekable(source);
        using var copy = new MemoryStream();
        source.CopyTo(copy);
        copy.Position = 0;
        return ReadSeekable(copy);
    }

    static Dictionary<uint, SteamAppEdit> ReadSeekable(Stream source)
    {
        var offset = source.Position;
        var first = source.ReadByte();
        source.Position = offset;
        if (first == '{')
            return JsonSerializer.Deserialize(source, SteamAppEditJsonContext.Default.DictionaryUInt32SteamAppEdit)
                ?? throw new InvalidDataException("Missing Steam app edits.");
        return ReadLegacy(source);
    }

    public static void Write(Stream destination, IReadOnlyDictionary<uint, SteamAppEdit> edits)
    {
        using var writer = new Utf8JsonWriter(destination);
        writer.WriteStartObject();
        foreach (var (id, edit) in edits)
        {
            writer.WritePropertyName(id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            JsonSerializer.Serialize(writer, edit, SteamAppEditJsonContext.Default.SteamAppEdit);
        }
        writer.WriteEndObject();
    }

    static Dictionary<uint, SteamAppEdit> ReadLegacy(Stream source)
    {
        var edits = new Dictionary<uint, SteamAppEdit>();
        foreach (var row in Serializable.DMP<List<ModifiedApp>>(source))
        {
            var properties = row.ReadChanges()?.GetPropertyValue<SteamAppPropertyTable>("appinfo");
            if (row.AppId == 0 || properties == null)
                continue;
            edits[row.AppId] = SteamAppEditProperties.ReadLegacyEdit(properties);
        }
        return edits;
    }
}

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Dictionary<uint, SteamAppEdit>))]
internal partial class SteamAppEditJsonContext : JsonSerializerContext;
#endif
