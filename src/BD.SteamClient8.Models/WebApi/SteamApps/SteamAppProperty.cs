#if !(IOS || ANDROID)
using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Models.Extensions;
using System.Extensions;
using SDColor = System.Drawing.Color;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// <see cref="SteamApp"/> Property
/// </summary>
public sealed partial class SteamAppProperty
{
    internal void Write(BinaryWriter writer)
    {
        writer.Write(unchecked((byte)PropertyType));
        writer.WriteAppInfoString(Name);
        switch (PropertyType)
        {
            case SteamAppPropertyType.Table:
                if (ValueTable != null)
                {
                    writer.Write(ValueTable);
                }
                break;
            case SteamAppPropertyType.String:
                writer.WriteAppInfoString(ValueString);
                break;
            case SteamAppPropertyType.Int32:
                writer.Write(ValueInt32);
                break;
            case SteamAppPropertyType.Float:
                writer.Write(ValueSingle);
                break;
            case SteamAppPropertyType.WString:
                writer.WriteAppInfoWideString(ValueString);
                break;
            case SteamAppPropertyType.Color:
                writer.Write(ValueColor);
                break;
            case SteamAppPropertyType.Uint64:
                writer.Write(ValueUInt64);
                break;
            default:
                throw new NotImplementedException(
                    $"The value type {PropertyType} has not been implemented.");
        }
    }

    /// <summary>
    /// 获取指定类型属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? GetValue<T>()
    {
        TryGetValue<T>(out var result);
        return result;
    }

    /// <summary>
    /// 尝试获取指定类型属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryGetValue<T>(out T? result)
    {
        switch (_propType)
        {
            case SteamAppPropertyType.Table:
                if (typeof(T) == typeof(SteamAppPropertyTable))
                {
                    var r = ValueTable;
                    if (r == null)
                    {
                        result = default;
                    }
                    else
                    {
                        result = Convert2.Convert<T, SteamAppPropertyTable>(r);
                    }
                    return true;
                }
                break;
            case SteamAppPropertyType.WString:
            case SteamAppPropertyType.String:
                if (typeof(T) == typeof(string))
                {
                    var r = ValueString;
                    if (r == null)
                    {
                        result = default;
                    }
                    else
                    {
                        result = Convert2.Convert<T, string>(r);
                    }
                    return true;
                }
                else if (typeof(T) == typeof(SDColor))
                {
                    var r = ValueColor; // 属性值 get 自带转换
                    result = Convert2.Convert<T, SDColor>(r);
                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    var r = ValueString;
                    if (int.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, int>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                else if (typeof(T) == typeof(uint))
                {
                    var r = ValueString;
                    if (uint.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, uint>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    var r = ValueString;
                    if (long.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, long>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                else if (typeof(T) == typeof(ulong))
                {
                    var r = ValueString;
                    if (ulong.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, ulong>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                else if (typeof(T) == typeof(float))
                {
                    var r = ValueString;
                    if (float.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, float>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    var r = ValueString;
                    if (double.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, double>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    var r = ValueString;
                    if (decimal.TryParse(r, out var number))
                    {
                        result = Convert2.Convert<T, decimal>(number);
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
                break;
            case SteamAppPropertyType.Int32:
                if (typeof(T) == typeof(int))
                {
                    var r = ValueInt32;
                    result = Convert2.Convert<T, int>(r);
                    return true;
                }
                else if (typeof(T) == typeof(string))
                {
                    var r = ValueInt32.ToString();
                    result = Convert2.Convert<T, string>(r);
                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    var r = unchecked((uint)ValueInt32);
                    result = Convert2.Convert<T, uint>(r);
                    return true;
                }
                else if (typeof(T) == typeof(long))
                {
                    long r = ValueInt32;
                    result = Convert2.Convert<T, long>(r);
                    return true;
                }
                else if (typeof(T) == typeof(ulong))
                {
                    var r = unchecked((ulong)ValueInt32);
                    result = Convert2.Convert<T, ulong>(r);
                    return true;
                }
                else if (typeof(T) == typeof(float))
                {
                    float r = ValueInt32;
                    result = Convert2.Convert<T, float>(r);
                    return true;
                }
                else if (typeof(T) == typeof(double))
                {
                    double r = ValueInt32;
                    result = Convert2.Convert<T, double>(r);
                    return true;
                }
                else if (typeof(T) == typeof(SDColor))
                {
                    var r = ValueColor; // 属性值 get 自带转换
                    result = Convert2.Convert<T, SDColor>(r);
                    return true;
                }
                break;
            case SteamAppPropertyType.Float:
                if (typeof(T) == typeof(float))
                {
                    var r = ValueSingle;
                    result = Convert2.Convert<T, float>(r);
                    return true;
                }
                else if (typeof(T) == typeof(string))
                {
                    var r = ValueSingle.ToString();
                    result = Convert2.Convert<T, string>(r);
                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    var r = (int)ValueSingle;
                    result = Convert2.Convert<T, int>(r);
                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    var r = (uint)ValueSingle;
                    result = Convert2.Convert<T, uint>(r);
                    return true;
                }
                else if (typeof(T) == typeof(long))
                {
                    var r = (long)ValueSingle;
                    result = Convert2.Convert<T, long>(r);
                    return true;
                }
                else if (typeof(T) == typeof(ulong))
                {
                    var r = (ulong)ValueSingle;
                    result = Convert2.Convert<T, ulong>(r);
                    return true;
                }
                else if (typeof(T) == typeof(double))
                {
                    double r = ValueSingle;
                    result = Convert2.Convert<T, double>(r);
                    return true;
                }
                break;
            case SteamAppPropertyType.Color:
                if (typeof(T) == typeof(SDColor))
                {
                    var r = ValueColor; // 属性值 get 自带转换
                    result = Convert2.Convert<T, SDColor>(r);
                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    var r = ValueColor.ToArgb();
                    result = Convert2.Convert<T, int>(r);
                    return true;
                }
                else if (typeof(T) == typeof(string))
                {
                    var r = ValueColor.ToString();
                    result = Convert2.Convert<T, string>(r);
                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    var r = unchecked((uint)ValueColor.ToArgb());
                    result = Convert2.Convert<T, uint>(r);
                    return true;
                }
                else if (typeof(T) == typeof(long))
                {
                    long r = ValueColor.ToArgb();
                    result = Convert2.Convert<T, long>(r);
                    return true;
                }
                else if (typeof(T) == typeof(ulong))
                {
                    var r = unchecked((ulong)ValueColor.ToArgb());
                    result = Convert2.Convert<T, ulong>(r);
                    return true;
                }
                break;
            case SteamAppPropertyType.Uint64:
                if (typeof(T) == typeof(ulong))
                {
                    var r = ValueUInt64;
                    result = Convert2.Convert<T, ulong>(r);
                    return true;
                }
                else if (typeof(T) == typeof(float))
                {
                    float r = ValueUInt64;
                    result = Convert2.Convert<T, float>(r);
                    return true;
                }
                else if (typeof(T) == typeof(string))
                {
                    var r = ValueUInt64.ToString();
                    result = Convert2.Convert<T, string>(r);
                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    var r = unchecked((int)ValueUInt64);
                    result = Convert2.Convert<T, int>(r);
                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    var r = unchecked((uint)ValueUInt64);
                    result = Convert2.Convert<T, uint>(r);
                    return true;
                }
                else if (typeof(T) == typeof(long))
                {
                    var r = unchecked((long)ValueUInt64);
                    result = Convert2.Convert<T, long>(r);
                    return true;
                }
                else if (typeof(T) == typeof(double))
                {
                    double r = ValueUInt64;
                    result = Convert2.Convert<T, double>(r);
                    return true;
                }
                break;
        }
        result = default;
        return false;
    }

    [global::System.Text.Json.Serialization.JsonConstructor]
    internal SteamAppProperty()
    {
    }

    /// <summary>
    /// 通过 <see cref="string"/> 属性名称 构造 <see cref="SteamAppProperty"/> 实例
    /// </summary>
    /// <param name="name"></param>
    public SteamAppProperty(string name)
    {
        Name = name;
        _propType = SteamAppPropertyType._Invalid_;
    }

    public SteamAppProperty(string name, SteamAppPropertyTable? valueTable)
    {
        Name = name;
        _propType = SteamAppPropertyType.Table;
        this.valueTable = valueTable;
    }

    public SteamAppProperty(string name, string? valueString, bool isWString = false)
    {
        Name = name;
        _propType = isWString ? SteamAppPropertyType.WString : SteamAppPropertyType.String;
        this.valueString = valueString;
    }

    public SteamAppProperty(string name, int? valueInt32)
    {
        Name = name;
        _propType = SteamAppPropertyType.Int32;
        this.valueInt32 = valueInt32;
    }

    public SteamAppProperty(string name, float? valueSingle)
    {
        Name = name;
        _propType = SteamAppPropertyType.Float;
        this.valueSingle = valueSingle;
    }

    public SteamAppProperty(string name, SDColor? valueColor)
    {
        Name = name;
        _propType = SteamAppPropertyType.Color;
        this.valueColor = valueColor;
    }

    public SteamAppProperty(string name, ulong? valueUInt64)
    {
        Name = name;
        _propType = SteamAppPropertyType.Uint64;
        this.valueUInt64 = valueUInt64;
    }

#if DEBUG
    /// <summary>
    /// 通过 <see cref="string"/> 属性名称，<see cref="SteamAppPropertyType"/> 属性数据类型，<see cref="object"/> 属性值 构造 <see cref="SteamAppProperty"/> 实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    [Obsolete("use other ctor", true)]
    public SteamAppProperty(string name, SteamAppPropertyType type, object value)
    {
        Name = name;
        _propType = type;
    }
#endif

    /// <summary>
    /// 通过其他 <see cref="SteamAppProperty"/> 构造 <see cref="SteamAppProperty"/> 实例
    /// </summary>
    /// <param name="other"></param>
    public SteamAppProperty(SteamAppProperty other)
    {
        SetCopyValue(other);
    }

    /// <summary>
    /// Equals <see langword="override"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        bool result = false;
        if (obj is SteamAppProperty property)
        {
            result = Equals(this, property);
        }
        return result;
    }

    /// <summary>
    /// GetHashCode <see langword="override"/>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return GetType().GetHashCode() ^ Name.GetHashCode() ^ _propType.GetHashCode() ^ GetValueHashCode();
    }
}
#endif