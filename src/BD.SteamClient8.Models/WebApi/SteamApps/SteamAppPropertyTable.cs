#if !(IOS || ANDROID)
namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// <see cref="SteamApp"/> 属性表格
/// </summary>
public class SteamAppPropertyTable
{
    private List<SteamAppProperty> _properties = [];

    /// <summary>
    /// 属性数量
    /// </summary>
    public int Count => _properties.Count;

    /// <summary>
    /// 属性名称集合
    /// </summary>
    public IEnumerable<string> PropertyNames => _properties.Select((SteamAppProperty prop) => prop.Name);

    /// <summary>
    /// 属性集合
    /// </summary>
    public IEnumerable<SteamAppProperty> Properties => _properties;

    /// <summary>
    /// 根据 <see cref="SteamAppProperty.Name"/> 获取 <see cref="SteamAppProperty"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SteamAppProperty? this[string name] => _properties.FirstOrDefault((SteamAppProperty prop) => prop.Name == name);

    /// <summary>
    /// 是否含有属性
    /// </summary>
    /// <param name="propertyPath"></param>
    /// <returns></returns>
    public bool HasProperty(params string[] propertyPath)
    {
        SteamAppPropertyTable? propertyTable = this;
        foreach (string name in propertyPath)
        {
            if (propertyTable == null || propertyTable[name] == null)
            {
                return false;
            }
            propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(name);
        }
        return true;
    }

    /// <summary>
    /// 根据属性名获取指定类型值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="defValue"></param>
    /// <returns></returns>
    public T? GetPropertyValue<T>(string name, T? defValue = default(T))
    {
        if (!TryGetPropertyValue<T>(name, out var result))
        {
            return defValue;
        }
        return result;
    }

    /// <summary>
    /// 尝试根据属性名获取指定类型值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryGetPropertyValue<T>(string name, out T? result)
    {
        bool result2 = false;
        result = default(T);
        SteamAppProperty? property = this[name];
        if (property != null)
        {
            result2 = property.TryGetValue<T>(out result);
        }
        return result2;
    }

    /// <summary>
    /// 根据名称获取属性的 <see cref="object"/> 类型
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object? GetPropertyAsObject(string name)
    {
        object? result = null;
        SteamAppProperty? property = this[name];
        if (property != null && property.Value != null)
        {
            result = property.Value;
        }
        return result;
    }

    /// <summary>
    /// 根据属性路径获取指定数据类型的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <param name="propertyPath"></param>
    /// <returns></returns>
    public T? GetPropertyValue<T>(T? defaultValue, params string[] propertyPath)
    {
        SteamAppPropertyTable? propertyTable = this;
        foreach (string item in propertyPath.Take(propertyPath.Length - 1))
        {
            SteamAppProperty? property = propertyTable[item];
            if (property != null)
            {
                propertyTable = property.GetValue<SteamAppPropertyTable>();
            }
            if (property == null || propertyTable == null)
            {
                return defaultValue;
            }
        }
        return propertyTable.GetPropertyValue(propertyPath.Last(), defaultValue);
    }

    /// <summary>
    /// 根据名称获取属性数据类型
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SteamAppPropertyType GetPropertyType(string name)
    {
        SteamAppPropertyType result = SteamAppPropertyType._Invalid_;
        SteamAppProperty? property = this[name];
        if (property != null)
        {
            result = property.PropertyType;
        }
        return result;
    }

    /// <summary>
    /// 新增属性
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AddPropertyValue(string name, SteamAppPropertyType type, object value)
    {
        SteamAppProperty item = new SteamAppProperty(name, type, value);
        _properties.Add(item);
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetPropertyValue(string name, SteamAppPropertyType type, object? value)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        bool result = property.PropertyType != type || !object.Equals(property.Value, value);
        property.PropertyType = type;
        property.Value = value;
        return result;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="propertyPath"></param>
    /// <returns></returns>
    public bool SetPropertyValue(SteamAppPropertyType type, object? value, params string[] propertyPath)
    {
        SteamAppPropertyTable? propertyTable = this;
        foreach (string item in propertyPath.Take(propertyPath.Length - 1))
        {
            if (propertyTable.HasProperty(item))
            {
                propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(item);
            }
            else
            {
                SteamAppPropertyTable propertyTable2 = new SteamAppPropertyTable();
                propertyTable.SetPropertyValue(item, SteamAppPropertyType.Table, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                return false;
            }
        }
        return propertyTable.SetPropertyValue(propertyPath.Last(), type, value);
    }

    /// <summary>
    /// 根据属性名称删除属性
    /// </summary>
    /// <param name="name"></param>
    public void RemoveProperty(string name)
    {
        _properties.RemoveAll((SteamAppProperty prop) => prop.Name == name);
    }

    /// <summary>
    /// 根据属性路径 删除属性
    /// </summary>
    /// <param name="propertyPath"></param>
    public void RemoveProperty(params string[] propertyPath)
    {
        SteamAppPropertyTable? propertyTable = this;
        foreach (string item in propertyPath.Take(propertyPath.Length - 1))
        {
            if (propertyTable.HasProperty(item))
            {
                propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(item);
            }
            else
            {
                SteamAppPropertyTable propertyTable2 = new SteamAppPropertyTable();
                propertyTable.SetPropertyValue(item, SteamAppPropertyType.Table, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                return;
            }
        }
        RemoveProperty(propertyPath.Last());
    }

    /// <summary>
    /// 通过属性名称 导出属性
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object? ExtractProperty(string name)
    {
        SteamAppProperty? property = this[name];
        if (property != null)
        {
            SteamAppPropertyTable propertyTable = new SteamAppPropertyTable();
            propertyTable.SetPropertyValue(property!.Name, property!.PropertyType, property?.Value);
            MemoryStream memoryStream = new MemoryStream();
            new BinaryWriter(memoryStream).Write(propertyTable);
            return memoryStream;
        }
        return null;
    }

    /// <summary>
    /// 将已导出的属性添加到 <see cref="SteamAppPropertyTable"/>
    /// </summary>
    /// <param name="extracted"></param>
    /// <returns></returns>
    public string AddExtractedProperty(object extracted)
    {
        SteamAppProperty property = new BinaryReader((MemoryStream)extracted).ReadPropertyTable()._properties[0];
        RemoveProperty(property.Name);
        _properties.Add(property);
        return property.Name;
    }

    /// <summary>
    /// 清空属性
    /// </summary>
    public void Clear()
    {
        _properties.Clear();
    }

    /// <summary>
    /// 无参构造 <see cref="SteamAppPropertyTable"/>
    /// </summary>
    public SteamAppPropertyTable()
    {
    }

    /// <summary>
    /// 通过其他 <see cref="SteamAppPropertyTable"/> 构造 <see cref="SteamAppPropertyTable"/>
    /// </summary>
    /// <param name="other"></param>
    public SteamAppPropertyTable(SteamAppPropertyTable other)
    {
        _properties.AddRange(other._properties.Select((SteamAppProperty prop) => new SteamAppProperty(prop)));
    }

    /// <summary>
    /// custom equals for <see cref="SteamAppPropertyTable"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(SteamAppPropertyTable other)
    {
        if (other != null && Count == other.Count)
        {
            return _properties.All((SteamAppProperty prop) => other._properties.Any((SteamAppProperty otherProp) => prop.Equals(otherProp)));
        }
        return false;
    }

    /// <summary>
    /// Equals <see langword="override"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        return Equals((obj as SteamAppPropertyTable)!);
    }

    /// <summary>
    /// GetHashCode <see langword="override"/>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        int num = GetType().GetHashCode();
        foreach (SteamAppProperty property in _properties)
        {
            num ^= property.GetHashCode();
        }
        return num;
    }

    private string ToStringInternal(int indent)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string arg = new string('\t', indent);
        foreach (SteamAppProperty property in _properties)
        {
            string arg2 = property.Name.Replace("\\", "\\\\").Replace("\"", "\\\"");
            stringBuilder.AppendFormat("{0}\"{1}\"", arg, arg2);
            switch (property.PropertyType)
            {
                case SteamAppPropertyType.Table:
                    stringBuilder.AppendFormat("\n{0}{{\n{1}{0}}}", arg, property.GetValue<SteamAppPropertyTable>()?.ToStringInternal(indent + 1));
                    break;
                case SteamAppPropertyType.String:
                case SteamAppPropertyType.WString:
                    stringBuilder.AppendFormat("\t\t\"{0}\"", property.GetValue<string>()?.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    break;
                case SteamAppPropertyType.Int32:
                case SteamAppPropertyType.Float:
                case SteamAppPropertyType.Color:
                case SteamAppPropertyType.Uint64:
                    stringBuilder.AppendFormat("\t\t\"{0}\"", property.Value);
                    break;
                default:
                    throw new NotImplementedException("The property type " + property.PropertyType.ToString() + " is invalid.");
            }
            stringBuilder.Append('\n');
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// ToString <see langword="override"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ToStringInternal(0);
    }
}
#endif
