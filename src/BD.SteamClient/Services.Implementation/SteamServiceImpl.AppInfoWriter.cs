#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Buffers.Binary;
using System.IO;
using System.IO.Hashing;
using System.Security.Cryptography;

namespace BD.SteamClient.Services.Implementation;

partial class SteamServiceImpl
{
    const int LegacyAppInfoMetadataSize = 16 + AppInfoSha1Size + 4;
    static readonly object appInfoWriteLock = new();

    /// <summary>Stages and validates the affected cache files before replacing any originals.</summary>
    /// <returns>Whether at least one file was replaced. Format and I/O errors are propagated.</returns>
    protected static bool RewriteAppInfoCache(string steamDirectory, IReadOnlyDictionary<uint, SteamAppEdit> edits, Func<bool> canWrite)
    {
        if (edits.Count == 0)
            return false;

        lock (appInfoWriteLock)
        {
            var stages = new List<AppInfoWriteStage>();
            try
            {
                void Stage(string path, bool sharded)
                {
                    var stage = new AppInfoWriteStage(path);
                    try
                    {
                        if (sharded ? StageAppInfoShard(stage, edits) : StageLegacyAppInfo(stage, edits))
                        {
                            stages.Add(stage);
                            return;
                        }
                    }
                    catch
                    {
                        stage.DeleteStage();
                        throw;
                    }
                    stage.DeleteStage();
                }

                var hasShards = false;
                foreach (var path in EnumerateAppInfoShards(steamDirectory))
                {
                    hasShards = true;
                    Stage(path, sharded: true);
                }

                if (!hasShards)
                {
                    var path = Path.Combine(steamDirectory, "appcache", "appinfo.vdf");
                    if (File.Exists(path))
                        Stage(path, sharded: false);
                }

                if (stages.Count == 0 || !canWrite() || (!hasShards && EnumerateAppInfoShards(steamDirectory).Any()))
                    return false;

                CommitAppInfoStages(stages);
                return true;
            }
            finally
            {
                foreach (var stage in stages)
                    stage.DeleteStage();
            }
        }
    }

    static void CommitAppInfoStages(List<AppInfoWriteStage> stages)
    {
        var attempted = 0;
        var completed = 0;
        try
        {
            foreach (var stage in stages)
            {
                stage.CheckSource();
                // Include the current file in rollback if the platform creates its backup
                // before reporting a replacement failure.
                attempted++;
                File.Replace(stage.TempPath, stage.SourcePath, stage.BackupPath);
                completed++;
            }
        }
        catch (Exception failure)
        {
            List<Exception>? rollbackErrors = null;
            for (var i = attempted - 1; i >= 0; i--)
            {
                var stage = stages[i];
                try
                {
                    if (i < completed || File.Exists(stage.BackupPath))
                        File.Move(stage.BackupPath, stage.SourcePath, overwrite: true);
                }
                catch (Exception rollbackError)
                {
                    (rollbackErrors ??= new List<Exception> { failure }).Add(new IOException(
                        $"Could not restore '{stage.SourcePath}' from '{stage.BackupPath}'.", rollbackError));
                }
            }
            if (rollbackErrors != null)
                throw new AggregateException("Steam appinfo replacement and rollback failed.", rollbackErrors);
            throw;
        }

        foreach (var stage in stages)
            DeleteAppInfoWorkFile(stage.BackupPath);
    }

    sealed class AppInfoWriteStage(string sourcePath)
    {
        string? workPath;
        long sourceLength;
        DateTime sourceWriteTime;

        public string SourcePath { get; } = sourcePath;

        public string TempPath => workPath + ".tmp";

        public string BackupPath => workPath + ".bak";

        public FileStream OpenSource(bool readIndexOnly = false)
        {
            var info = new FileInfo(SourcePath);
            sourceLength = info.Length;
            sourceWriteTime = info.LastWriteTimeUtc;
            // An unaffected shard must not read ahead into its string or data sections.
            return new FileStream(SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: readIndexOnly ? 1 : 64 * 1024);
        }

        public FileStream CreateStage()
        {
            var path = $"{SourcePath}.{Guid.NewGuid():N}";
            var stream = new FileStream(path + ".tmp", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 64 * 1024);
            workPath = path;
            return stream;
        }

        public void CheckSource()
        {
            var info = new FileInfo(SourcePath);
            if (!info.Exists || info.Length != sourceLength || info.LastWriteTimeUtc != sourceWriteTime)
                throw new IOException($"Steam appinfo cache changed while staging '{SourcePath}'.");
        }

        public void DeleteStage()
        {
            if (workPath != null)
                DeleteAppInfoWorkFile(TempPath);
        }
    }

    static void DeleteAppInfoWorkFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Best effort: do not mask a write/rollback failure or report a completed commit as failed.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    static bool StageLegacyAppInfo(AppInfoWriteStage stage, IReadOnlyDictionary<uint, SteamAppEdit> edits)
    {
        using var source = stage.OpenSource();
        if (source.Length == 0)
            return false;

        using var input = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        var (metadataSize, recordsEnd, indexed) = ReadLegacyAppInfoWriteHeader(input);
        AppInfoWriteStringPool? pool = null;
        FileStream? destination = null;
        var changed = new HashSet<long>();
        using var copy = new AppInfoRangeCopy();
        long copyFrom = 0;
        try
        {
            while (true)
            {
                var entryStart = source.Position;
                var (id, size) = ReadLegacyAppInfoWriteEntry(input, recordsEnd, metadataSize);
                if (id == 0)
                    break;

                var entryEnd = source.Position + size;
                if (!edits.TryGetValue(id, out var edit))
                {
                    source.Position = entryEnd;
                    continue;
                }

                if (indexed && pool == null)
                    pool = ReadLegacyAppInfoWritePool(input, recordsEnd);
                var entry = ReadAppInfoBytes(source, size);
                var properties = ReadAppInfoWriteProperties(entry, metadataSize, pool?.Strings, out var payloadEnd);
                if (!SteamAppEditProperties.Apply(properties, edit))
                    continue;

                var payload = SerializeAppInfoWriteProperties(properties, pool).AsSpan();
                if (payload.SequenceEqual(entry.AsSpan(metadataSize, payloadEnd - metadataSize)))
                    continue;

                destination ??= stage.CreateStage();
                copy.Copy(source, destination, copyFrom, entryStart - copyFrom);
                changed.Add(destination.Position);
                WriteLegacyAppInfoEntry(destination, id, entry, metadataSize, payloadEnd, properties, payload);
                source.Position = copyFrom = entryEnd;
            }

            if (destination == null)
                return false;

            copy.Copy(source, destination, copyFrom, recordsEnd - copyFrom);
            if (indexed)
            {
                var tableOffset = destination.Position;
                using var output = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
                output.Write((uint)pool!.Count);
                pool!.Write(output);
                destination.Position = 8;
                output.Write(tableOffset);
            }

            destination.Flush(flushToDisk: true);
            destination.Position = 0;
            ValidateLegacyAppInfoStage(destination, changed);
            return true;
        }
        finally
        {
            destination?.Dispose();
        }
    }

    static (int MetadataSize, long RecordsEnd, bool Indexed) ReadLegacyAppInfoWriteHeader(BinaryReader reader)
    {
        var version = reader.ReadUInt32();
        var metadataSize = version switch
        {
            MagicNumber => LegacyAppInfoMetadataSize,
            MagicNumberV2 or MagicNumberV3 => LegacyAppInfoMetadataSize + AppInfoSha1Size,
            _ => throw new InvalidDataException($"Unsupported Steam appinfo magic: 0x{version:X8}."),
        };
        reader.ReadUInt32(); // Universe.
        var stream = reader.BaseStream;
        var indexed = version == MagicNumberV3;
        var recordsEnd = indexed ? reader.ReadInt64() : stream.Length;
        if (recordsEnd < stream.Position + sizeof(uint) || recordsEnd > stream.Length - (indexed ? sizeof(uint) : 0))
            throw new InvalidDataException("Invalid Steam appinfo record section length.");
        return (metadataSize, recordsEnd, indexed);
    }

    static (uint Id, int Size) ReadLegacyAppInfoWriteEntry(BinaryReader reader, long recordsEnd, int metadataSize)
    {
        var stream = reader.BaseStream;
        if (recordsEnd - stream.Position < sizeof(uint))
            throw new InvalidDataException("Missing Steam appinfo end-of-records marker.");
        var id = reader.ReadUInt32();
        if (id == 0)
            return (0, 0);
        if (recordsEnd - stream.Position < sizeof(int))
            throw new InvalidDataException($"Missing Steam appinfo length for {id}.");
        var size = reader.ReadInt32();
        if (size <= metadataSize || size > recordsEnd - stream.Position)
            throw new InvalidDataException($"Invalid Steam appinfo length for {id}.");
        return (id, size);
    }

    static AppInfoWriteStringPool ReadLegacyAppInfoWritePool(BinaryReader reader, long recordsEnd)
    {
        var stream = reader.BaseStream;
        var position = stream.Position;
        stream.Position = recordsEnd;
        var count = reader.ReadUInt32();
        var size = stream.Length - stream.Position;
        if (count > size || size > Array.MaxLength)
            throw new InvalidDataException("Invalid Steam appinfo string table length.");
        var pool = new AppInfoWriteStringPool(ReadAppInfoBytes(stream, (int)size), (int)count);
        stream.Position = position;
        return pool;
    }

    static void WriteLegacyAppInfoEntry(Stream destination, uint id, byte[] entry, int metadataSize, int payloadEnd,
        SteamAppPropertyTable properties, ReadOnlySpan<byte> payload)
    {
        var suffix = entry.AsSpan(payloadEnd);
        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
        writer.Write(id);
        writer.Write(checked(metadataSize + payload.Length + suffix.Length));
        writer.Write(entry.AsSpan(0, 16));
        writer.Write(SHA1.HashData(Encoding.UTF8.GetBytes(properties.ToString())));
        writer.Write(entry.AsSpan(16 + AppInfoSha1Size, 4)); // Change number.
        if (metadataSize != LegacyAppInfoMetadataSize)
            writer.Write(SHA1.HashData(payload));
        writer.Write(payload);
        writer.Write(suffix);
    }

    static void ValidateLegacyAppInfoStage(Stream stream, HashSet<long> changed)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var (metadataSize, recordsEnd, indexed) = ReadLegacyAppInfoWriteHeader(reader);
        var strings = indexed ? ReadLegacyAppInfoWritePool(reader, recordsEnd).Strings : null;
        while (true)
        {
            var start = stream.Position;
            var (id, size) = ReadLegacyAppInfoWriteEntry(reader, recordsEnd, metadataSize);
            if (id == 0)
                break;
            if (!changed.Remove(start))
            {
                stream.Position += size;
                continue;
            }

            var entry = ReadAppInfoBytes(stream, size);
            _ = ReadAppInfoWriteProperties(entry, metadataSize, strings, out var payloadEnd);
            if (metadataSize != LegacyAppInfoMetadataSize)
                ValidateAppInfoSha1(entry.AsSpan(LegacyAppInfoMetadataSize, AppInfoSha1Size), entry.AsSpan(metadataSize, payloadEnd - metadataSize));
        }
        if (changed.Count != 0)
            throw new InvalidDataException("A rewritten entry was missing from the staged Steam appinfo cache.");
    }

    static bool StageAppInfoShard(AppInfoWriteStage stage, IReadOnlyDictionary<uint, SteamAppEdit> edits)
    {
        using var source = stage.OpenSource(readIndexOnly: true);
        Span<byte> header = stackalloc byte[ShardHeaderSize];
        var (appCount, index, stringCount, stringBytes, dataBytes) = ReadAppInfoWriteShardIndex(source, header);
        var entryPosition = 0;
        var hasMatches = false;
        for (var i = 0; i < appCount; i++)
        {
            var (id, _, _) = ReadAppInfoWriteShardEntry(index, dataBytes, ref entryPosition);
            hasMatches |= edits.ContainsKey(id);
        }
        if (entryPosition != index.Length)
            throw new InvalidDataException("Unexpected bytes after the Steam appinfo shard index.");
        if (!hasMatches)
            return false;

        var strings = ReadAppInfoBytes(source, stringBytes);
        ValidateAppInfoShardCrc(header, index, strings);
        var pool = new AppInfoWriteStringPool(strings, stringCount);
        var sourceDataStart = source.Position;
        var replacements = new Dictionary<int, ArraySegment<byte>>();
        entryPosition = 0;
        for (var i = 0; i < appCount; i++)
        {
            var start = entryPosition;
            var (id, offset, size) = ReadAppInfoWriteShardEntry(index, dataBytes, ref entryPosition);
            if (!edits.TryGetValue(id, out var edit))
                continue;

            source.Position = sourceDataStart + offset;
            var blob = ReadAppInfoBytes(source, size);
            var properties = ReadAppInfoWriteProperties(blob, AppInfoSha1Size, pool.Strings, out var payloadEnd);
            if (payloadEnd != blob.Length)
                throw new InvalidDataException($"Invalid Steam appinfo shard property length for {id}.");
            if (!SteamAppEditProperties.Apply(properties, edit))
                continue;
            var payload = SerializeAppInfoWriteProperties(properties, pool);
            if (!payload.AsSpan().SequenceEqual(blob.AsSpan(AppInfoSha1Size)))
                replacements.Add(start, payload);
        }
        if (replacements.Count == 0)
            return false;

        using var destination = stage.CreateStage();
        // Reserve the header and index; their sizes are fixed but offsets depend on the new data.
        destination.Position = ShardHeaderSize + (long)index.Length;
        using (var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true))
            pool.Write(writer);
        var destinationDataStart = destination.Position;
        var order = new (int Position, int Offset, int Size)[appCount];
        entryPosition = 0;
        for (var i = 0; i < order.Length; i++)
        {
            var start = entryPosition;
            var (_, offset, size) = ReadAppInfoWriteShardEntry(index, dataBytes, ref entryPosition);
            order[i] = (start, offset, size);
        }
        Array.Sort(order, static (left, right) => left.Offset.CompareTo(right.Offset));

        // Keep the original data order and any gaps/padding. The (possibly differently ordered)
        // index keeps its entry order and all opaque metadata.
        using var copy = new AppInfoRangeCopy();
        var previousEnd = 0;
        long copyFrom = 0;
        long delta = 0;
        Span<byte> sha1 = stackalloc byte[AppInfoSha1Size];
        foreach (var (position, offset, size) in order)
        {
            if (offset < previousEnd)
                throw new InvalidDataException("Overlapping Steam appinfo shard records cannot be rewritten.");
            previousEnd = offset + size;
            BinaryPrimitives.WriteUInt32LittleEndian(index.AsSpan(position + 4), checked((uint)(offset + delta)));
            if (!replacements.TryGetValue(position, out var payload))
                continue;

            copy.Copy(source, destination, sourceDataStart + copyFrom, offset - copyFrom);
            SHA1.HashData(payload.AsSpan(), sha1);
            destination.Write(sha1);
            destination.Write(payload.AsSpan());
            var newSize = (long)AppInfoSha1Size + payload.Count;
            BinaryPrimitives.WriteUInt32LittleEndian(index.AsSpan(position + 8), checked((uint)newSize));
            copyFrom = previousEnd;
            delta += newSize - size;
        }
        copy.Copy(source, destination, sourceDataStart + copyFrom, dataBytes - copyFrom);

        var destinationStringBytes = destinationDataStart - ShardHeaderSize - index.Length;
        BinaryPrimitives.WriteUInt32LittleEndian(header[24..], (uint)pool.Count);
        BinaryPrimitives.WriteUInt32LittleEndian(header[28..], checked((uint)destinationStringBytes));
        BinaryPrimitives.WriteUInt32LittleEndian(header[32..], checked((uint)(destination.Position - destinationDataStart)));
        var crc = new Crc32();
        crc.Append(index);
        destination.Position = ShardHeaderSize + (long)index.Length;
        copy.AppendChecksum(destination, destinationStringBytes, crc);
        BinaryPrimitives.WriteUInt32LittleEndian(header[36..], crc.GetCurrentHashAsUInt32());
        BinaryPrimitives.WriteUInt32LittleEndian(header[40..], Crc32.HashToUInt32(header[..40]));
        destination.Position = 0;
        destination.Write(header);
        destination.Write(index);
        destination.Flush(flushToDisk: true);
        destination.Position = 0;
        ValidateAppInfoShardStage(destination, replacements);
        return true;
    }

    static (int AppCount, byte[] Index, int StringCount, int StringBytes, int DataBytes) ReadAppInfoWriteShardIndex(Stream stream, Span<byte> header)
    {
        stream.ReadExactly(header);
        var magic = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (magic != ShardedAppInfoMagic)
            throw new InvalidDataException($"Unsupported Steam appinfo shard magic: 0x{magic:X8}.");
        var appCount = BinaryPrimitives.ReadUInt32LittleEndian(header[16..]);
        var indexSize = BinaryPrimitives.ReadUInt32LittleEndian(header[20..]);
        var stringCount = BinaryPrimitives.ReadUInt32LittleEndian(header[24..]);
        var stringSize = BinaryPrimitives.ReadUInt32LittleEndian(header[28..]);
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(header[32..]);
        var bodySize = (long)indexSize + stringSize + dataSize;
        if (bodySize > Array.MaxLength || bodySize != stream.Length - ShardHeaderSize ||
            appCount * (long)ShardIndexEntrySize > indexSize || stringCount > stringSize)
            throw new InvalidDataException("Invalid Steam appinfo shard section lengths.");

        return ((int)appCount, ReadAppInfoBytes(stream, (int)indexSize), (int)stringCount, (int)stringSize, (int)dataSize);
    }

    static (uint Id, int Offset, int Size) ReadAppInfoWriteShardEntry(ReadOnlySpan<byte> index, int dataBytes, ref int position)
    {
        var entry = index[position..];
        if (entry.Length < ShardIndexEntrySize)
            throw new InvalidDataException("Truncated Steam appinfo shard index.");
        var id = BinaryPrimitives.ReadUInt32LittleEndian(entry);
        var offset = BinaryPrimitives.ReadUInt32LittleEndian(entry[4..]);
        var size = BinaryPrimitives.ReadUInt32LittleEndian(entry[8..]);
        var metadataSize = BinaryPrimitives.ReadUInt32LittleEndian(entry[12..]);
        if (metadataSize > entry.Length - ShardIndexEntrySize || size <= AppInfoSha1Size || (long)offset + size > dataBytes)
            throw new InvalidDataException($"Invalid Steam appinfo shard index entry for {id}.");
        position += ShardIndexEntrySize + (int)metadataSize;
        return (id, (int)offset, (int)size);
    }

    static void ValidateAppInfoShardStage(Stream stream, Dictionary<int, ArraySegment<byte>> replacements)
    {
        Span<byte> header = stackalloc byte[ShardHeaderSize];
        var (appCount, index, stringCount, stringBytes, dataBytes) = ReadAppInfoWriteShardIndex(stream, header);
        var strings = ReadAppInfoBytes(stream, stringBytes);
        ValidateAppInfoShardCrc(header, index, strings);
        var pool = ReadShardStringTable(strings, stringCount);
        var dataStart = stream.Position;
        var position = 0;
        for (var i = 0; i < appCount; i++)
        {
            var start = position;
            var (id, offset, size) = ReadAppInfoWriteShardEntry(index, dataBytes, ref position);
            if (!replacements.Remove(start, out var payload))
                continue;

            stream.Position = dataStart + offset;
            var entry = ReadAppInfoBytes(stream, size);
            _ = ReadAppInfoWriteProperties(entry, AppInfoSha1Size, pool, out var end);
            if (end != size || !entry.AsSpan(AppInfoSha1Size).SequenceEqual(payload.AsSpan()))
                throw new InvalidDataException($"Invalid staged Steam appinfo shard property length for {id}.");
            ValidateAppInfoSha1(entry.AsSpan(0, AppInfoSha1Size), entry.AsSpan(AppInfoSha1Size));
        }
        if (position != index.Length || replacements.Count != 0)
            throw new InvalidDataException("Invalid staged Steam appinfo shard index.");
    }

    static void ValidateAppInfoShardCrc(ReadOnlySpan<byte> header, ReadOnlySpan<byte> index, ReadOnlySpan<byte> strings)
    {
        var crc = new Crc32();
        crc.Append(index);
        crc.Append(strings);
        if (crc.GetCurrentHashAsUInt32() != BinaryPrimitives.ReadUInt32LittleEndian(header[36..]) ||
            Crc32.HashToUInt32(header[..40]) != BinaryPrimitives.ReadUInt32LittleEndian(header[40..]))
            throw new InvalidDataException("Invalid Steam appinfo shard checksum.");
    }

    static void ValidateAppInfoSha1(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> payload)
    {
        Span<byte> sha1 = stackalloc byte[AppInfoSha1Size];
        SHA1.HashData(payload, sha1);
        if (!expected.SequenceEqual(sha1))
            throw new InvalidDataException("Invalid staged Steam appinfo record SHA-1.");
    }

    static byte[] ReadAppInfoBytes(Stream stream, int size)
    {
        if (size > Array.MaxLength)
            throw new InvalidDataException("Steam appinfo section is too large.");
        var bytes = GC.AllocateUninitializedArray<byte>(size);
        stream.ReadExactly(bytes);
        return bytes;
    }

    static SteamAppPropertyTable ReadAppInfoWriteProperties(byte[] entry, int start, string[]? pool, out int end)
    {
        using var stream = new MemoryStream(entry, 0, entry.Length, writable: false, publiclyVisible: true);
        stream.Position = start;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var properties = pool == null ? reader.ReadPropertyTable() : reader.ReadPropertyTable(pool, entry.Length);
        end = (int)stream.Position;
        return properties;
    }

    static ArraySegment<byte> SerializeAppInfoWriteProperties(SteamAppPropertyTable properties, AppInfoWriteStringPool? pool)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        if (pool == null)
            writer.Write(properties);
        else
            pool.WriteProperties(writer, properties);
        return new ArraySegment<byte>(stream.GetBuffer(), 0, checked((int)stream.Length));
    }

    sealed class AppInfoWriteStringPool(byte[] bytes, int count)
    {
        List<string>? expanded;
        Dictionary<string, uint>? lookup;

        public string[] Strings { get; } = ReadShardStringTable(bytes, count);

        public int Count => expanded?.Count ?? count;

        public void WriteProperties(BinaryWriter writer, SteamAppPropertyTable properties)
        {
            if (lookup == null)
            {
                expanded = new List<string>(Strings);
                lookup = new Dictionary<string, uint>(Strings.Length, StringComparer.Ordinal);
                for (var i = 0; i < Strings.Length; i++)
                    lookup.TryAdd(Strings[i], (uint)i);
            }
            writer.WritePropertyTable_0x07564429(properties, expanded!, lookup);
        }

        public void Write(BinaryWriter writer)
        {
            // Retain the original bytes, including duplicate keys and their indices.
            writer.Write(bytes);
            if (expanded != null)
                for (var i = count; i < expanded.Count; i++)
                    writer.WriteAppInfoString(expanded[i]);
        }
    }

    sealed class AppInfoRangeCopy : IDisposable
    {
        byte[]? buffer;

        public void Copy(Stream source, Stream destination, long offset, long count)
        {
            source.Position = offset;
            while (count > 0)
            {
                var bytes = ReadChunk(source, count);
                destination.Write(bytes);
                count -= bytes.Length;
            }
        }

        public void AppendChecksum(Stream source, long count, Crc32 checksum)
        {
            while (count > 0)
            {
                var bytes = ReadChunk(source, count);
                checksum.Append(bytes);
                count -= bytes.Length;
            }
        }

        ReadOnlySpan<byte> ReadChunk(Stream source, long remaining)
        {
            buffer ??= ArrayPool<byte>.Shared.Rent(64 * 1024);
            var bytes = buffer.AsSpan(0, (int)Math.Min(buffer.Length, remaining));
            source.ReadExactly(bytes);
            return bytes;
        }

        public void Dispose()
        {
            if (buffer != null)
                ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
#endif
