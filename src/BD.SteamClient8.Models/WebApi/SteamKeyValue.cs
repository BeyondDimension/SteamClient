using BD.SteamClient8.Models.Converters;
using System.Extensions;
using System.Globalization;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 用户统计类型枚举
/// </summary>
public enum UserStatType
{
    Invalid = 0,
    Integer = 1,
    Float = 2,
    AverageRate = 3,
    Achievements = 4,
    GroupAchievements = 5,
}

/// <summary>
/// 键值对象
/// </summary>
public sealed record class SteamKeyValue
{
    static readonly SteamKeyValue _Invalid = new();

    /// <summary>
    /// 键名
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name = "<root>";

    /// <summary>
    /// 键值类型
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("type")]
    public SteamKeyValueType Type = SteamKeyValueType.None;

    [global::System.Text.Json.Serialization.JsonPropertyName("string")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? StringValue;

    [global::System.Text.Json.Serialization.JsonPropertyName("int")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Int32Value;

    [global::System.Text.Json.Serialization.JsonPropertyName("ulong")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public ulong? UInt64Value;

    [global::System.Text.Json.Serialization.JsonPropertyName("float")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public float? SingleValue;

    [global::System.Text.Json.Serialization.JsonPropertyName("uint")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public uint? UInt32Value;

    /// <summary>
    /// 是否有效
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("valid")]
    public bool Valid;

    /// <summary>
    /// 子集
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("children")]
    public List<SteamKeyValue>? Children;

    /// <summary>
    /// 通过索引获取对象
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public SteamKeyValue this[string key]
    {
        get
        {
            if (Children == null)
            {
                return _Invalid;
            }

            var child = Children.SingleOrDefault(
                c => string.Compare(c.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (child == null)
            {
                return _Invalid;
            }

            return child;
        }
    }

    /// <summary>
    /// 验证有效性并返回当前值的 <see cref="string" /> 类型; 否则返回传入的默认值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public string? AsString(string defaultValue)
    {
        if (Valid == false)
        {
            return defaultValue;
        }

        if (StringValue == null)
        {
            return defaultValue;
        }

        return StringValue;
    }

    /// <summary>
    /// 验证有效性并返回当前值的 <see cref="int" /> 类型; 否则返回传入的默认值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public int AsInteger(int defaultValue)
    {
        if (Valid == false)
        {
            return defaultValue;
        }

        switch (Type)
        {
            case SteamKeyValueType.String:
            case SteamKeyValueType.WideString:
                if (int.TryParse(StringValue, out var value))
                {
                    return value;
                }
                break;
            case SteamKeyValueType.Int32:
                if (Int32Value.HasValue)
                {
                    return Int32Value.Value;
                }
                break;
            case SteamKeyValueType.Float32:
                if (SingleValue.HasValue)
                {
                    return (int)SingleValue.Value;
                }
                break;
            case SteamKeyValueType.UInt64:
                if (UInt64Value.HasValue)
                {
                    return (int)(UInt64Value.Value & 0xFFFFFFFF);
                }
                break;
        }

        return defaultValue;
    }

    /// <summary>
    /// 验证有效性并返回当前值的 <see cref="float" /> 类型; 否则返回传入的默认值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public float AsFloat(float defaultValue)
    {
        if (Valid == false)
        {
            return defaultValue;
        }

        switch (Type)
        {
            case SteamKeyValueType.String:
            case SteamKeyValueType.WideString:
                if (float.TryParse(StringValue, out var value))
                {
                    return value;
                }
                break;
            case SteamKeyValueType.Int32:
                if (Int32Value.HasValue)
                {
                    return Int32Value.Value;
                }
                break;
            case SteamKeyValueType.Float32:
                if (SingleValue.HasValue)
                {
                    return SingleValue.Value;
                }
                break;
            case SteamKeyValueType.UInt64:
                if (UInt64Value.HasValue)
                {
                    return (ulong)UInt64Value.Value & 0xFFFFFFFF;
                }
                break;
        }

        return defaultValue;
    }

    /// <summary>
    /// 验证有效性并返回当前值的 <see cref="bool" /> 类型; 否则返回传入的默认值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public bool AsBoolean(bool defaultValue)
    {
        if (Valid == false)
        {
            return defaultValue;
        }

        switch (Type)
        {
            case SteamKeyValueType.String:
            case SteamKeyValueType.WideString:
                if (!string.IsNullOrWhiteSpace(StringValue))
                {
                    if (short.TryParse(StringValue, out var value))
                    {
                        return value != 0;
                    }
                    else if (bool.TryParse(StringValue, out var boolValue))
                    {
                        return boolValue;
                    }
                }
                break;
            case SteamKeyValueType.Int32:
                if (Int32Value.HasValue)
                {
                    return Int32Value.Value != 0;
                }
                break;
            case SteamKeyValueType.Float32:
                if (SingleValue.HasValue)
                {
                    return (int)SingleValue.Value != 0;
                }
                break;
            case SteamKeyValueType.UInt64:
                if (UInt64Value.HasValue)
                {
                    return UInt64Value.Value != 0;
                }
                break;
        }

        return defaultValue;
    }

    /// <summary>
    /// 如果有效，返回 键名 或 键值信息
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (Valid == false)
        {
            return "<invalid>";
        }

        if (Type == SteamKeyValueType.None)
        {
            return Name;
        }

        return string.Format(CultureInfo.CurrentCulture,
            "{0} = {1}",
            Name,
            AsString(string.Empty));
    }

    /// <summary>
    /// 通过文件加载对象
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static SteamKeyValue? LoadAsBinary(string path)
    {
        if (File.Exists(path) == false)
        {
            return null;
        }

        try
        {
            using var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var kv = new SteamKeyValue();
            if (kv.ReadAsBinary(input) == false)
            {
                return null;
            }
            return kv;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 读取键值信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public bool ReadAsBinary(Stream input)
    {
        Children = [];

        try
        {
            while (true)
            {
                var type = (SteamKeyValueType)input.ReadValueU8();

                if (type == SteamKeyValueType.End)
                {
                    break;
                }

                var current = new SteamKeyValue
                {
                    Type = type,
                    Name = input.ReadStringUnicode(),
                };

                switch (type)
                {
                    case SteamKeyValueType.None:
                        {
                            current.ReadAsBinary(input);
                            break;
                        }

                    case SteamKeyValueType.String:
                        {
                            current.Valid = true;
                            current.StringValue = input.ReadStringUnicode();
                            break;
                        }

                    case SteamKeyValueType.WideString:
                        {
                            throw new FormatException("wstring is unsupported");
                        }

                    case SteamKeyValueType.Int32:
                        {
                            current.Valid = true;
                            current.Int32Value = input.ReadValueS32();
                            break;
                        }

                    case SteamKeyValueType.UInt64:
                        {
                            current.Valid = true;
                            current.UInt64Value = input.ReadValueU64();
                            break;
                        }

                    case SteamKeyValueType.Float32:
                        {
                            current.Valid = true;
                            current.SingleValue = input.ReadValueF32();
                            break;
                        }

                    case SteamKeyValueType.Color:
                        {
                            current.Valid = true;
                            current.UInt32Value = input.ReadValueU32();
                            break;
                        }

                    case SteamKeyValueType.Pointer:
                        {
                            current.Valid = true;
                            current.UInt32Value = input.ReadValueU32();
                            break;
                        }

                    default:
                        {
                            throw new FormatException();
                        }
                }

                if (input.Position >= input.Length)
                {
                    throw new FormatException();
                }

                Children.Add(current);
            }

            Valid = true;
            return input.Position == input.Length;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

/// <summary>
/// 键值类型枚举
/// </summary>
public enum SteamKeyValueType : byte
{
    None = 0,
    String = 1,
    Int32 = 2,
    Float32 = 3,
    Pointer = 4,
    WideString = 5,
    Color = 6,
    UInt64 = 7,
    End = 8,
}
