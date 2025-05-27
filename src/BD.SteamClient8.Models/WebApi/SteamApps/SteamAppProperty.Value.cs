#if !(IOS || ANDROID)
using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Enums.WebApi.SteamApps;
using SDColor = System.Drawing.Color;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

partial class SteamAppProperty : JsonModel<SteamAppProperty>, IJsonSerializerContext, IEquatable<SteamAppProperty>
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    SteamAppPropertyType _propType;

    // 强类型值字段
    SteamAppPropertyTable? valueTable;
    string? valueString;
    int? valueInt32;
    float? valueSingle;
    SDColor? valueColor;
    ulong? valueUInt64;

    internal void SetCopyValue(SteamAppProperty p, bool isCreateNewTable = true)
    {
        Name = p.Name;
        _propType = p._propType;

        // 复制表格属性
        if (p._propType == SteamAppPropertyType.Table)
        {
            valueTable = p.valueTable == null ? null : (isCreateNewTable ? new SteamAppPropertyTable(p.valueTable) : p.valueTable);
        }
        else
        {
            valueString = p.valueString;
            valueInt32 = p.valueInt32;
            valueSingle = p.valueSingle;
            valueColor = p.valueColor;
            valueUInt64 = p.valueUInt64;
        }
    }

    void SetAllNullValue()
    {
        valueTable = null;
        valueString = null;
        valueInt32 = null;
        valueSingle = null;
        valueColor = null;
        valueUInt64 = null;
    }

    [global::System.Text.Json.Serialization.JsonIgnore]
    internal object? Value => _propType switch
    {
        SteamAppPropertyType.Table => ValueTable,
        SteamAppPropertyType.WString or SteamAppPropertyType.String => ValueString,
        SteamAppPropertyType.Int32 => ValueInt32,
        SteamAppPropertyType.Float => ValueSingle,
        SteamAppPropertyType.Color => ValueColor,
        SteamAppPropertyType.Uint64 => ValueUInt64,
        _ => null,
    };

    /// <inheritdoc/>
    public /*virtual*/ bool Equals(SteamAppProperty? p)
    {
        if (p == null)
        {
            return false;
        }
        if (p.Name != Name)
        {
            return false;
        }
        if (p._propType != _propType)
        {
            return false;
        }
        switch (_propType)
        {
            case SteamAppPropertyType.Table:
                {
                    var l = p.ValueTable;
                    var r = ValueTable;
                    if (l == null && r == null)
                    {
                        return true;
                    }
                    else if (l != null)
                    {
                        if (r == null)
                        {
                            return false;
                        }
                        return l.Equals(r);
                    }
                    else if (r != null)
                    {
                        if (l == null)
                        {
                            return false;
                        }
                        return r.Equals(l);
                    }
                    else
                    {
                        return false;
                    }
                }
            case SteamAppPropertyType.String:
            case SteamAppPropertyType.WString:
                return string.Equals(p.ValueString, ValueString);
            case SteamAppPropertyType.Int32:
                return p.ValueInt32 == ValueInt32;
            case SteamAppPropertyType.Float:
                return p.ValueSingle == ValueSingle;
            case SteamAppPropertyType.Color:
                return p.ValueColor == ValueColor;
            case SteamAppPropertyType.Uint64:
                return p.ValueUInt64 == ValueUInt64;
        }
        return true;
    }

    int GetValueHashCode() => _propType switch
    {
        SteamAppPropertyType.Table => ValueTable?.GetHashCode() ?? default,
        SteamAppPropertyType.WString or SteamAppPropertyType.String => ValueString?.GetHashCode() ?? default,
        SteamAppPropertyType.Int32 => ValueInt32.GetHashCode(),
        SteamAppPropertyType.Float => ValueSingle.GetHashCode(),
        SteamAppPropertyType.Color => ValueColor.GetHashCode(),
        SteamAppPropertyType.Uint64 => ValueUInt64.GetHashCode(),
        _ => default,
    };

    /// <summary>
    /// 属性类型 <see cref="SteamAppPropertyType"/> => <see cref="Type"/>
    /// </summary>
    [global::System.Text.Json.Serialization.JsonIgnore]
    public Type? ValueType => _propType switch
    {
        SteamAppPropertyType.Table => typeof(SteamAppPropertyTable),
        SteamAppPropertyType.String or SteamAppPropertyType.WString => typeof(string),
        SteamAppPropertyType.Int32 => typeof(int),
        SteamAppPropertyType.Float => typeof(float),
        SteamAppPropertyType.Color => typeof(SDColor),
        SteamAppPropertyType.Uint64 => typeof(ulong),
        _ => null,
    };

    [global::System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name
    {
        get => field ?? string.Empty;
        set => field = value;
    }

    /// <summary>
    /// 属性类型枚举
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("type")]
    public SteamAppPropertyType PropertyType
    {
        get => _propType;
        set
        {
            if (_propType != value)
            {
                _propType = value;
                SetAllNullValue();
            }
        }
    }

    [global::System.Text.Json.Serialization.JsonPropertyName("table")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public SteamAppPropertyTable? ValueTable
    {
        get
        {
            if (_propType == SteamAppPropertyType.Table)
            {
                return valueTable;
            }
            return null;
        }
        set => valueTable = value;
    }

    [global::System.Text.Json.Serialization.JsonPropertyName("string")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? ValueString
    {
        get
        {
            if (_propType == SteamAppPropertyType.String || _propType == SteamAppPropertyType.WString)
            {
                return valueString;
            }
            return null;
        }
        set => valueString = value;
    }

    [global::System.Text.Json.Serialization.JsonPropertyName("int")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public int ValueInt32
    {
        get
        {
            if (_propType == SteamAppPropertyType.Int32)
            {
                if (valueInt32.HasValue)
                {
                    return valueInt32.Value;
                }
                else if (int.TryParse(valueString, out var number))
                {
                    valueInt32 = number;
                    return number;
                }
            }
            return default;
        }
        set => valueInt32 = value;
    }

    [global::System.Text.Json.Serialization.JsonPropertyName("float")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public float ValueSingle
    {
        get
        {
            if (_propType == SteamAppPropertyType.Float)
            {
                if (valueSingle.HasValue)
                {
                    return valueSingle.Value;
                }
                else if (float.TryParse(valueString, out var number))
                {
                    valueSingle = number;
                    return number;
                }
            }
            return default;
        }
        set => valueSingle = value;
    }

    [global::System.Text.Json.Serialization.JsonPropertyName("color")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public SDColor ValueColor
    {
        get
        {
            if (_propType == SteamAppPropertyType.Color)
            {
                if (valueColor.HasValue)
                {
                    return valueColor.Value;
                }
                else if (!string.IsNullOrWhiteSpace(valueString))
                {
                    if (int.TryParse(valueString, out var number))
                    {
                        var color = SDColor.FromArgb(number);
                        valueColor = color;
                        return color;
                    }
                    else if (Enum.TryParse<global::System.Drawing.KnownColor>(valueString, true, out var kColor))
                    {
                        var color = SDColor.FromKnownColor(kColor);
                        valueColor = color;
                        return color;
                    }
                }
            }
            return default;
        }
        set => valueColor = value;
    }

    [global::System.Text.Json.Serialization.JsonPropertyName("ulong")]
    [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public ulong ValueUInt64
    {
        get
        {
            if (_propType == SteamAppPropertyType.Uint64)
            {
                if (valueUInt64.HasValue)
                {
                    return valueUInt64.Value;
                }
                else if (ulong.TryParse(valueString, out var number))
                {
                    valueUInt64 = number;
                    return number;
                }
            }
            return default;
        }
        set => valueUInt64 = value;
    }
}
#endif