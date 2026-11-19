#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Buffers.Binary;
using System.Globalization;

namespace BD.SteamClient.Services.Implementation;

partial class SteamServiceImpl
{
    const uint ShardedAppInfoMagic = 0x0756442A;
    const int ShardHeaderSize = 44;
    const int ShardIndexEntrySize = 16;
    const int AppInfoSha1Size = 20;

    static IEnumerable<string> EnumerateAppInfoShards(string? steamDirectory)
    {
        var directory = CombineSteamPath(steamDirectory, "appcache", "appinfo");
        if (directory == null || !Directory.Exists(directory))
            yield break;

        foreach (var path in Directory.EnumerateFiles(directory, "appinfo_??.vdf"))
        {
            var name = Path.GetFileNameWithoutExtension(path.AsSpan());
            if (name.Length == 10 && byte.TryParse(name[8..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out _))
                yield return path;
        }
    }

    static void ReadAppInfoShard(string path, IReadOnlySet<uint> installedAppIds, Action<SteamApp> onApp,
        bool isSaveProperties, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete, bufferSize: 1, FileOptions.SequentialScan);
        Span<byte> header = stackalloc byte[ShardHeaderSize];
        stream.ReadExactly(header);

        // After the magic, universe and shard identity/count: index count and size,
        // string count and size, data size, and an eight-byte checksum.
        var magic = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (magic != ShardedAppInfoMagic)
            throw new InvalidDataException($"Unsupported Steam appinfo shard {Path.GetFileName(path)}: 0x{magic:X8}.");
        var appCount = BinaryPrimitives.ReadUInt32LittleEndian(header[16..]);
        var indexSize = BinaryPrimitives.ReadUInt32LittleEndian(header[20..]);
        var stringCount = BinaryPrimitives.ReadUInt32LittleEndian(header[24..]);
        var stringSize = BinaryPrimitives.ReadUInt32LittleEndian(header[28..]);
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(header[32..]);
        var bodySize = (long)indexSize + stringSize + dataSize;
        if (bodySize > Array.MaxLength || bodySize != stream.Length - ShardHeaderSize ||
            appCount * (long)ShardIndexEntrySize > indexSize || stringCount > stringSize)
            throw new InvalidDataException($"Invalid Steam appinfo shard sections in {Path.GetFileName(path)}.");

        // Keep only one shard's bytes at a time. The property reader shares the buffer
        // and only the extracted fields survive a normal library load.
        var buffer = ArrayPool<byte>.Shared.Rent((int)bodySize);
        try
        {
            stream.ReadExactly(buffer, 0, (int)bodySize);
            var index = buffer.AsSpan(0, (int)indexSize);
            var stringPool = ReadShardStringTable(buffer.AsSpan((int)indexSize, (int)stringSize), (int)stringCount);
            using var data = new MemoryStream(buffer, (int)(indexSize + stringSize), (int)dataSize, writable: false, publiclyVisible: true);
            using var reader = new BinaryReader(data, Encoding.UTF8, leaveOpen: true);
            for (var i = 0u; i < appCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (index.Length < ShardIndexEntrySize)
                    throw new InvalidDataException($"Truncated Steam appinfo index in {Path.GetFileName(path)}.");

                var id = BinaryPrimitives.ReadUInt32LittleEndian(index);
                var offset = BinaryPrimitives.ReadUInt32LittleEndian(index[4..]);
                var size = BinaryPrimitives.ReadUInt32LittleEndian(index[8..]);
                var metadataSize = BinaryPrimitives.ReadUInt32LittleEndian(index[12..]);
                index = index[ShardIndexEntrySize..];
                if (metadataSize > index.Length || size <= AppInfoSha1Size || (long)offset + size > dataSize)
                    throw new InvalidDataException($"Invalid Steam appinfo record {id} in {Path.GetFileName(path)}.");
                index = index[(int)metadataSize..];

                // Offsets point into the data section; skip its SHA-1 before the indexed KeyValues.
                data.Position = offset + AppInfoSha1Size;
                var end = (long)offset + size;
                var properties = isSaveProperties ? reader.ReadPropertyTable(stringPool, end)
                    : reader.ReadAppInfoMetadata(stringPool, end, installedAppIds.Contains(id));
                if (data.Position != end)
                    throw new InvalidDataException($"Invalid Steam appinfo record length for {id} in {Path.GetFileName(path)}.");
                onApp(SteamApp.FromShardedAppInfo(id, properties, installedAppIds, isSaveProperties));
            }
            if (!index.IsEmpty)
                throw new InvalidDataException($"Unexpected bytes after the Steam appinfo index in {Path.GetFileName(path)}.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    static string[] ReadShardStringTable(ReadOnlySpan<byte> bytes, int count)
    {
        var strings = new string[count];
        for (var i = 0; i < strings.Length; i++)
        {
            var length = bytes.IndexOf((byte)0);
            if (length < 0)
                throw new InvalidDataException("Truncated Steam appinfo shard string table.");
            strings[i] = Encoding.UTF8.GetString(bytes[..length]);
            bytes = bytes[(length + 1)..];
        }
        if (!bytes.IsEmpty)
            throw new InvalidDataException("Unexpected bytes after the Steam appinfo shard string table.");
        return strings;
    }
}
#endif
