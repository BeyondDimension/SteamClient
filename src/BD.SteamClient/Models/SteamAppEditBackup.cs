#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using MessagePack.Resolvers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient.Models;

public static class SteamAppEditBackup
{
    // Old .stmbak files and modifications.vdf share the same three-slot row contract.
    static readonly MessagePackSerializerOptions legacyOptions = Serializable.lz4Options.WithResolver(
        CompositeResolver.Create(
            [new MessagePack.Formatters.ListFormatter<ModifiedApp>(), new ModifiedAppFormatter()],
            [BuiltinResolver.Instance]));

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
        foreach (var row in MessagePackSerializer.Deserialize<List<ModifiedApp>>(source, legacyOptions)!)
        {
            var properties = row.ReadChanges()?.GetPropertyValue<SteamAppPropertyTable>("appinfo");
            if (row.AppId == 0 || properties == null)
                continue;
            edits[row.AppId] = SteamAppEditProperties.ReadLegacyEdit(properties);
        }
        return edits;
    }

    sealed class ModifiedAppFormatter : IMessagePackFormatter<ModifiedApp>
    {
        public void Serialize(ref MessagePackWriter writer, ModifiedApp value, MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(3);
            writer.Write(value.AppId);
            writer.Write(value.OriginalData);
            writer.Write(value.ChangesData);
        }

        public ModifiedApp Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                return null!;

            var count = reader.ReadArrayHeader();
            options.Security.DepthStep(ref reader);
            try
            {
                var row = new ModifiedApp();
                for (var slot = 0; slot < count; slot++)
                {
                    switch (slot)
                    {
                        case 0: row.AppId = reader.ReadUInt32(); break;
                        case 1: row.OriginalData = ByteArrayFormatter.Instance.Deserialize(ref reader, options); break;
                        case 2: row.ChangesData = ByteArrayFormatter.Instance.Deserialize(ref reader, options); break;
                        default: reader.Skip(); break;
                    }
                }
                return row;
            }
            finally { reader.Depth--; }
        }
    }
}

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Dictionary<uint, SteamAppEdit>))]
internal partial class SteamAppEditJsonContext : JsonSerializerContext;
#endif
