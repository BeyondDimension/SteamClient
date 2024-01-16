#if !(IOS || ANDROID)
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamApp"/> Property
/// </summary>
public class SteamAppProperty
{
    private static readonly Type?[] _types =
    [
        typeof(SteamAppPropertyTable),
        typeof(string),
        typeof(int),
        typeof(float),
        null,
        typeof(string),
        typeof(SDColor),
        typeof(ulong),
        null
    ];

    public readonly string Name;

    private SteamAppPropertyType _propType;

    private object? _value;

    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public SteamAppPropertyType PropertyType
    {
        get
        {
            return _propType;
        }

        internal set
        {
            if (_propType != value)
            {
                _propType = value;
                _value = null;
            }
        }
    }

    /// <summary>
    /// 属性类型 <see cref="SteamAppPropertyType"/> => <see cref="Type"/>
    /// </summary>
    public Type? ValueType => _types[(int)_propType];

    internal object? Value
    {
        get
        {
            return _value;
        }

        set
        {
            _value = null;
            if (ValueType != null && ValueType.IsAssignableFrom(value?.GetType()))
            {
                _value = value;
            }
            else if (value is string text)
            {
                switch (_propType)
                {
                    case SteamAppPropertyType.String:
                        _value = text;
                        break;
                    case SteamAppPropertyType.Int32:
                        _value = int.Parse(text);
                        break;
                    case SteamAppPropertyType.Float:
                        _value = float.Parse(text);
                        break;
                    case SteamAppPropertyType.WString:
                        _value = text;
                        break;
                    case SteamAppPropertyType.Uint64:
                        _value = ulong.Parse(text);
                        break;
                    case (SteamAppPropertyType)4:
                    case SteamAppPropertyType.Color:
                        break;
                }
            }
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
        result = default;
        bool result2 = false;
        int propType = (int)_propType;
        if (propType >= 0 && propType < _types.Length)
        {
            var type = _types[propType];
            if (type != null && typeof(T).IsAssignableFrom(type))
            {
                result = _value == null ? default : (T)_value;
                result2 = true;
            }
        }
        return result2;
    }

    /// <summary>
    /// 通过 <see cref="string"/> 属性名称 构造 <see cref="SteamAppProperty"/> 实例
    /// </summary>
    /// <param name="name"></param>
    public SteamAppProperty(string name)
    {
        Name = name;
        _propType = SteamAppPropertyType._Invalid_;
        _value = null;
    }

    /// <summary>
    /// 通过 <see cref="string"/> 属性名称，<see cref="SteamAppPropertyType"/> 属性数据类型，<see cref="object"/> 属性值 构造 <see cref="SteamAppProperty"/> 实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public SteamAppProperty(string name, SteamAppPropertyType type, object value)
    {
        Name = name;
        _propType = type;
        _value = value;
    }

    /// <summary>
    /// 通过其他 <see cref="SteamAppProperty"/> 构造 <see cref="SteamAppProperty"/> 实例
    /// </summary>
    /// <param name="other"></param>
    public SteamAppProperty(SteamAppProperty other)
    {
        Name = other.Name;
        _propType = other._propType;
        if (_propType == SteamAppPropertyType.Table)
        {
            _value = new SteamAppPropertyTable((SteamAppPropertyTable)other._value!);
        }
        else
        {
            _value = other._value;
        }
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
            result = Name == property.Name && _propType == property._propType && object.Equals(_value, property.Value);
        }
        return result;
    }

    /// <summary>
    /// GetHashCode <see langword="override"/>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return GetType().GetHashCode() ^ Name.GetHashCode() ^ _propType.GetHashCode() ^ _value!.GetHashCode();
    }
}
#endif