#if !(IOS || ANDROID)
using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Models.WebApi.SteamApps;
using System.Runtime.CompilerServices;
using System.Text;
using SDColor = System.Drawing.Color;

namespace BD.SteamClient8.Models.Extensions;

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
    public static void WriteAppInfoString(this BinaryWriter writer, string? str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
        }
        writer.Write(unchecked((byte)0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAppInfoWideString(this BinaryWriter writer, string? str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            writer.Write(bytes);
        }
        writer.Write((byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SDColor ReadColor(this BinaryReader reader)
    {
        byte red = reader.ReadByte();
        byte green = reader.ReadByte();
        byte blue = reader.ReadByte();
        return SDColor.FromArgb(red, green, blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(this BinaryWriter writer, SDColor color)
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
            property.Write(writer);
        }
        writer.Write(unchecked((byte)8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SteamAppPropertyTable ReadPropertyTable(this BinaryReader reader, string[]? stringPool = null)
    {
        SteamAppPropertyTable propertyTable = new SteamAppPropertyTable();
        SteamAppPropertyType propertyType;
        while ((propertyType = (SteamAppPropertyType)reader.ReadByte()) != SteamAppPropertyType._EndOfTable_)
        {
            string name;
            if (stringPool == null)
            {
                name = reader.ReadAppInfoString();
            }
            else
            {
                name = stringPool[reader.ReadInt32()];
            }
            SteamAppProperty p = propertyType switch
            {
                SteamAppPropertyType.Table => new(name, reader.ReadPropertyTable(stringPool)),
                SteamAppPropertyType.String => new(name, reader.ReadAppInfoString(), isWString: false),
                SteamAppPropertyType.WString => new(name, reader.ReadAppInfoWideString(), isWString: true),
                SteamAppPropertyType.Int32 => new(name, reader.ReadInt32()),
                SteamAppPropertyType.Uint64 => new(name, reader.ReadUInt64()),
                SteamAppPropertyType.Float => new(name, reader.ReadSingle()),
                SteamAppPropertyType.Color => new(name, reader.ReadColor()),
                _ => throw new NotImplementedException(
                    $"The property type {propertyType} has not been implemented."),
            };
            propertyTable.AddPropertyValue(p);
        }
        return propertyTable;
    }
}

#endif