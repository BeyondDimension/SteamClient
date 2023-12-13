#if WINDOWS || MACCATALYST || MACOS || LINUX

using Color = System.Drawing.Color;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning disable SA1600 // Elements should be documented

namespace System;

public static partial class BinaryReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAppInfoString(this BinaryReader reader)
    {
        byte[] buffer;
        int count;
        using (MemoryStream memoryStream = new())
        {
            byte value;
            while ((value = reader.ReadByte()) != 0)
            {
                memoryStream.WriteByte(value);
            }
            buffer = memoryStream.GetBuffer();
            count = (int)memoryStream.Length;
        }
        return Encoding.UTF8.GetString(buffer, 0, count);
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
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAppInfoWideString(this BinaryWriter writer, string str)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(str);
        writer.Write(bytes);
        writer.Write((byte)0);
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
    public static SteamAppPropertyTable ReadPropertyTable(this BinaryReader reader)
    {
        SteamAppPropertyTable propertyTable = new SteamAppPropertyTable();
        SteamAppPropertyType propertyType;
        while ((propertyType = (SteamAppPropertyType)reader.ReadByte()) != SteamAppPropertyType._EndOfTable_)
        {
            string name = reader.ReadAppInfoString();
            propertyTable.AddPropertyValue(value: propertyType switch
            {
                SteamAppPropertyType.Table => reader.ReadPropertyTable(),
                SteamAppPropertyType.String => reader.ReadAppInfoString(),
                SteamAppPropertyType.WString => reader.ReadAppInfoWideString(),
                SteamAppPropertyType.Int32 => reader.ReadInt32(),
                SteamAppPropertyType.Uint64 => reader.ReadUInt64(),
                SteamAppPropertyType.Float => reader.ReadSingle(),
                SteamAppPropertyType.Color => reader.ReadColor(),
                _ => throw new NotImplementedException("The property type " + propertyType.ToString() + " has not been implemented."),
            }, name: name, type: propertyType);
        }
        return propertyTable;
    }
}

#endif