#if WINDOWS || MACCATALYST || MACOS || LINUX

using Color = System.Drawing.Color;

// ReSharper disable once CheckNamespace
namespace System;

public static partial class BinaryReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAppInfoString(this BinaryReader reader)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(256);
        try
        {
            var count = 0;
            byte value;
            while ((value = reader.ReadByte()) != 0)
            {
                if (count == buffer.Length)
                {
                    var largerBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    Buffer.BlockCopy(buffer, 0, largerBuffer, 0, count);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = largerBuffer;
                }
                buffer[count++] = value;
            }
            return Encoding.UTF8.GetString(buffer, 0, count);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAppInfoWideString(this BinaryReader reader)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (char c = (char)reader.ReadUInt16(); c != 0; c = (char)reader.ReadUInt16())
        {
            stringBuilder.Append(c);
        }
        return stringBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAppInfoString(this BinaryWriter writer, string str)
    {
        if (str.Contains('\0'))
            throw new InvalidDataException("A Steam appinfo string cannot contain a NUL character.");

        byte[] bytes = Encoding.UTF8.GetBytes(str);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAppInfoWideString(this BinaryWriter writer, string str)
    {
        if (str.Contains('\0'))
            throw new InvalidDataException("A Steam appinfo wide string cannot contain a NUL character.");

        foreach (var c in str)
            writer.Write((ushort)c);
        writer.Write((ushort)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAppInfoString_0x07564429(this BinaryReader reader, IList<string> stringTable)
    {
        uint num = reader.ReadUInt32();
        return stringTable[(int)num];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAppInfoString_0x07564429(this BinaryWriter writer, string str, List<string> stringTable, Dictionary<string, uint> stringLookup)
    {
        uint num = uint.MaxValue;
        if (!stringLookup.TryGetValue(str, out num))
        {
            num = (uint)stringTable.Count;
            stringTable.Add(str);
            stringLookup.Add(str, num);
        }
        writer.Write(num);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ReadColor(this BinaryReader reader)
    {
        byte red = reader.ReadByte();
        byte green = reader.ReadByte();
        byte blue = reader.ReadByte();
        return Color.FromArgb(red, green, blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(this BinaryWriter writer, Color color)
    {
        writer.Write(color.R);
        writer.Write(color.G);
        writer.Write(color.B);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(this BinaryWriter writer, SteamAppPropertyTable table)
    {
        foreach (SteamAppProperty property in table.Properties)
        {
            writer.Write((byte)property.PropertyType);
            writer.WriteAppInfoString(property.Name);
            switch (property.PropertyType)
            {
                case SteamAppPropertyType.Table:
                    if (property.Value is SteamAppPropertyTable table1)
                        writer.Write(table1);
                    break;
                case SteamAppPropertyType.String:
                    writer.WriteAppInfoString(property.Value?.ToString() ?? string.Empty);
                    break;
                case SteamAppPropertyType.WString:
                    writer.WriteAppInfoWideString(property.Value?.ToString() ?? string.Empty);
                    break;
                case SteamAppPropertyType.Int32:
                    if (property.Value is not int int32)
                        int32 = default;
                    writer.Write(int32);
                    break;
                case SteamAppPropertyType.Uint64:
                    if (property.Value is not ulong uint64)
                        uint64 = default;
                    writer.Write(uint64);
                    break;
                case SteamAppPropertyType.Float:
                    if (property.Value is not float single)
                        single = default;
                    writer.Write(single);
                    break;
                case SteamAppPropertyType.Color:
                    if (property.Value is not Color color)
                        color = default;
                    writer.Write(color);
                    break;
                default:
                    throw new NotImplementedException("The value type " + property.PropertyType.ToString() + " has not been implemented.");
            }
        }
        writer.Write((byte)8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WritePropertyTable_0x07564429(this BinaryWriter writer, SteamAppPropertyTable table, List<string> stringTable, Dictionary<string, uint> stringLookup)
    {
        foreach (SteamAppProperty property in table.Properties)
        {
            writer.Write((byte)property.PropertyType);
            writer.WriteAppInfoString_0x07564429(property.Name, stringTable, stringLookup);
            switch (property.PropertyType)
            {
                case SteamAppPropertyType.Table:
                    if (property.Value is SteamAppPropertyTable table1)
                        writer.WritePropertyTable_0x07564429(table1, stringTable, stringLookup);
                    break;
                case SteamAppPropertyType.String:
                    writer.WriteAppInfoString(property.Value?.ToString() ?? string.Empty);
                    break;
                case SteamAppPropertyType.WString:
                    writer.WriteAppInfoWideString(property.Value?.ToString() ?? string.Empty);
                    break;
                case SteamAppPropertyType.Int32:
                    if (property.Value is not int int32)
                        int32 = default;
                    writer.Write(int32);
                    break;
                case SteamAppPropertyType.Uint64:
                    if (property.Value is not ulong uint64)
                        uint64 = default;
                    writer.Write(uint64);
                    break;
                case SteamAppPropertyType.Float:
                    if (property.Value is not float single)
                        single = default;
                    writer.Write(single);
                    break;
                case SteamAppPropertyType.Color:
                    if (property.Value is not Color color)
                        color = default;
                    writer.Write(color);
                    break;
                default:
                    throw new NotImplementedException("The value type " + property.PropertyType.ToString() + " has not been implemented.");
            }
        }
        writer.Write((byte)8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetPropertyTableByte_0x07564429(this SteamAppPropertyTable table, List<string> stringTable, Dictionary<string, uint> stringLookup)
    {
        byte[] array;
        using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
        {
            binaryWriter.WritePropertyTable_0x07564429(table, stringTable, stringLookup);
            array = ((MemoryStream)binaryWriter.BaseStream).ToArray();
        }
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SteamAppPropertyTable ReadPropertyTable(this BinaryReader reader, string[]? stringPool = null) =>
        ReadPropertyTable(reader, stringPool, null, 0);

    internal static SteamAppPropertyTable ReadPropertyTable(this BinaryReader reader, string[] stringPool, long endOffset) =>
        ReadPropertyTable(reader, stringPool, endOffset, 0);

    internal static SteamAppPropertyTable ReadAppInfoMetadata(this BinaryReader reader, string[] stringPool, long endOffset, bool installed) =>
        ReadPropertyTable(reader, stringPool, endOffset, 0, AppInfoPart.Root, installed);

    enum AppInfoPart : byte { All, Root, App, Common, Extended, Config, Ufs, Skip }

    static AppInfoPart SelectAppInfoPart(AppInfoPart parent, string name, bool installed) => parent switch
    {
        AppInfoPart.All => AppInfoPart.All,
        AppInfoPart.Root when name == "appinfo" => AppInfoPart.App,
        AppInfoPart.App => name switch
        {
            "common" => AppInfoPart.Common,
            "extended" => AppInfoPart.Extended,
            "config" when installed => AppInfoPart.Config,
            "ufs" => AppInfoPart.Ufs,
            "steam_edit" => AppInfoPart.All,
            _ => AppInfoPart.Skip,
        },
        AppInfoPart.Common when name is "name" or "sortas" or "parent" or "type" or "oslist" => AppInfoPart.All,
        AppInfoPart.Extended when name is "developer" or "publisher" => AppInfoPart.All,
        AppInfoPart.Config when name == "launch" => AppInfoPart.All,
        AppInfoPart.Ufs when name is "quota" or "maxnumfiles" or "savefiles" => AppInfoPart.All,
        _ => AppInfoPart.Skip,
    };

    static SteamAppPropertyTable ReadPropertyTable(BinaryReader reader, string[]? stringPool, long? endOffset, int depth,
        AppInfoPart part = AppInfoPart.All, bool installed = false)
    {
        if (depth >= 64)
            throw new InvalidDataException("The Steam appinfo property table is too deeply nested.");

        var stream = reader.BaseStream;
        SteamAppPropertyTable propertyTable = new SteamAppPropertyTable();
        while (endOffset == null || stream.Position < endOffset)
        {
            var propertyType = (SteamAppPropertyType)reader.ReadByte();
            if (propertyType == SteamAppPropertyType._EndOfTable_)
                return propertyTable;

            string name;
            if (stringPool == null)
            {
                name = reader.ReadAppInfoString();
            }
            else
            {
                var index = reader.ReadUInt32();
                if (index >= stringPool.Length)
                    throw new InvalidDataException("Invalid Steam appinfo string table index.");
                name = stringPool[index];
            }
            var child = SelectAppInfoPart(part, name, installed);
            if (child == AppInfoPart.Skip)
            {
                SkipIndexedProperty(reader, propertyType, endOffset!.Value);
                continue;
            }
            object value = propertyType switch
            {
                SteamAppPropertyType.Table => ReadPropertyTable(reader, stringPool, endOffset, depth + 1, child, installed),
                SteamAppPropertyType.String => ReadPropertyString(reader, endOffset),
                SteamAppPropertyType.WString => reader.ReadAppInfoWideString(),
                SteamAppPropertyType.Int32 => reader.ReadInt32(),
                SteamAppPropertyType.Uint64 => reader.ReadUInt64(),
                SteamAppPropertyType.Float => reader.ReadSingle(),
                SteamAppPropertyType.Color => reader.ReadColor(),
                _ => throw new InvalidDataException($"Unsupported Steam appinfo property type: {(byte)propertyType}."),
            };
            if (endOffset is { } end && stream.Position > end)
                throw new InvalidDataException("A Steam appinfo property crosses its record boundary.");
            propertyTable.AddPropertyValue(name, propertyType, value);
        }
        throw new InvalidDataException("Unterminated Steam appinfo property table.");
    }

    static string ReadPropertyString(BinaryReader reader, long? endOffset)
    {
        if (endOffset is not { } end || reader.BaseStream is not MemoryStream stream || !stream.TryGetBuffer(out var buffer))
            return reader.ReadAppInfoString();

        var position = stream.Position;
        if (position >= end)
            throw new InvalidDataException("Missing Steam appinfo string value.");
        var bytes = buffer.AsSpan((int)position, (int)(end - position));
        var length = bytes.IndexOf((byte)0);
        if (length < 0)
            throw new InvalidDataException("Unterminated Steam appinfo string value.");
        stream.Position = position + length + 1;
        return Encoding.UTF8.GetString(bytes[..length]);
    }

    static void SkipIndexedProperty(BinaryReader reader, SteamAppPropertyType type, long end)
    {
        var stream = (MemoryStream)reader.BaseStream;
        var depth = 0;
        while (true)
        {
            switch (type)
            {
                case SteamAppPropertyType.Table:
                    depth++;
                    break;
                case SteamAppPropertyType.String:
                    stream.TryGetBuffer(out var buffer);
                    var position = stream.Position;
                    var length = position < end ? buffer.AsSpan((int)position, (int)(end - position)).IndexOf((byte)0) : -1;
                    if (length < 0) throw new InvalidDataException("Unterminated Steam appinfo string value.");
                    stream.Position += length + 1;
                    break;
                case SteamAppPropertyType.WString:
                    while (stream.Position + 2 <= end && reader.ReadUInt16() != 0) { }
                    break;
                case SteamAppPropertyType.Int32:
                case SteamAppPropertyType.Float:
                    stream.Position += 4;
                    break;
                case SteamAppPropertyType.Uint64:
                    stream.Position += 8;
                    break;
                case SteamAppPropertyType.Color:
                    stream.Position += 3;
                    break;
                case SteamAppPropertyType._EndOfTable_:
                    depth--;
                    break;
                default:
                    throw new InvalidDataException($"Unsupported Steam appinfo property type: {(byte)type}.");
            }

            if (stream.Position > end)
                throw new InvalidDataException("A Steam appinfo property crosses its record boundary.");
            if (depth == 0)
                return;
            if (stream.Position == end)
                throw new InvalidDataException("Unterminated Steam appinfo property table.");
            type = (SteamAppPropertyType)reader.ReadByte();
            if (type != SteamAppPropertyType._EndOfTable_)
                stream.Position += 4;
        }
    }
}

#endif