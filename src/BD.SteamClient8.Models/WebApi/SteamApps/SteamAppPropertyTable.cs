#if !(IOS || ANDROID)
using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Models.Converters;
using BD.SteamClient8.Models.Extensions;
using System.Text;
using System.Text.Json;
using SDColor = System.Drawing.Color;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// <see cref="SteamApp"/> 属性表格
/// </summary>
[global::System.Text.Json.Serialization.JsonConverter(typeof(SteamAppPropertyTableConverter))]
public sealed class SteamAppPropertyTable : JsonModel<SteamAppProperty>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    readonly List<SteamAppProperty> _properties;

    public static implicit operator List<SteamAppProperty>(SteamAppPropertyTable table) => table._properties;

    /// <summary>
    /// 无参构造 <see cref="SteamAppPropertyTable"/>
    /// </summary>
    public SteamAppPropertyTable() : this([])
    {
    }

    internal SteamAppPropertyTable(List<SteamAppProperty> properties)
    {
        _properties = properties;
    }

    /// <summary>
    /// 通过其他 <see cref="SteamAppPropertyTable"/> 构造 <see cref="SteamAppPropertyTable"/>
    /// </summary>
    /// <param name="other"></param>
    public SteamAppPropertyTable(SteamAppPropertyTable other) :
        this([.. other._properties.Select(prop => new SteamAppProperty(prop))]) // 复制值，创建一份新的
    {
    }

    /// <summary>
    /// 属性数量
    /// </summary>
    public int Count => _properties.Count;

    /// <summary>
    /// 属性名称集合
    /// </summary>
    public IEnumerable<string> PropertyNames => _properties.Select(prop => prop.Name);

    /// <summary>
    /// 属性集合
    /// </summary>
    public IEnumerable<SteamAppProperty> Properties => _properties;

    /// <summary>
    /// 根据 <see cref="SteamAppProperty.Name"/> 获取 <see cref="SteamAppProperty"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SteamAppProperty? this[string name] => _properties.FirstOrDefault(prop => prop.Name == name);

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
        result = default;
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
    /// <param name="p"></param>
    public void AddPropertyValue(SteamAppProperty p)
    {
        _properties.Add(p);
    }

    //#if DEBUG
    //    /// <summary>
    //    /// 新增属性
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <param name="type"></param>
    //    /// <param name="value"></param>
    //    [Obsolete("use AddPropertyValue(SteamAppProperty)", true)]
    //    public void AddPropertyValue(string name, SteamAppPropertyType type, object value)
    //    {
    //        SteamAppProperty item = new SteamAppProperty(name, type, value);
    //        _properties.Add(item);
    //    }

    //    /// <summary>
    //    /// 设置属性值
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <param name="type"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    [Obsolete("use SetPropertyValue(string name, T? value)", true)]
    //    public void SetPropertyValue(string name, SteamAppPropertyType type, object? value)
    //    {
    //        throw new NotSupportedException();
    //        SteamAppProperty? property = this[name];
    //        if (property == null)
    //        {
    //            property = new SteamAppProperty(name);
    //            _properties.Add(property);
    //        }
    //        //bool result = property.PropertyType != type || !object.Equals(property.Value, value);
    //        property.PropertyType = type;
    //        //property.Value = value;
    //        //return result;
    //    }

    //    /// <summary>
    //    /// 设置属性值
    //    /// </summary>
    //    /// <param name="type"></param>
    //    /// <param name="value"></param>
    //    /// <param name="propertyPath"></param>
    //    /// <returns></returns>
    //    [Obsolete("use SetPropertyValue(string name, T? value)", true)]
    //    public void SetPropertyValue(SteamAppPropertyType type, object? value, params string[] propertyPath)
    //    {
    //        SteamAppPropertyTable? propertyTable = this;
    //        foreach (string item in propertyPath.Take(propertyPath.Length - 1))
    //        {
    //            if (propertyTable.HasProperty(item))
    //            {
    //                propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(item);
    //            }
    //            else
    //            {
    //                SteamAppPropertyTable propertyTable2 = new SteamAppPropertyTable();
    //                propertyTable.SetPropertyValue(item, SteamAppPropertyType.Table, propertyTable2);
    //                propertyTable = propertyTable2;
    //            }
    //            if (propertyTable == null)
    //            {
    //                //return false;
    //                return;
    //            }
    //        }
    //        /*return*/
    //        propertyTable.SetPropertyValue(propertyPath.Last(), type, value);
    //    }
    //#endif

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(SteamAppProperty p)
    {
        SteamAppProperty? property = this[p.Name];
        if (property == null)
        {
            property = p;
            _properties.Add(property);
        }
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string name, SteamAppPropertyTable? valueTable)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        property.PropertyType = SteamAppPropertyType.Table;
        property.ValueTable = valueTable;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(SteamAppPropertyTable? valueTable, params string[] propertyPath)
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
                propertyTable.SetPropertyValue(item, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                //return false;
                return;
            }
        }
        /*return*/
        propertyTable.SetPropertyValue(propertyPath.Last(), valueTable);
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string name, string? valueString, bool isWString = false)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        property.PropertyType = isWString ? SteamAppPropertyType.WString : SteamAppPropertyType.String;
        property.ValueString = valueString;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string? valueString, bool isWString = false, params string[] propertyPath)
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
                propertyTable.SetPropertyValue(item, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                //return false;
                return;
            }
        }
        /*return*/
        propertyTable.SetPropertyValue(propertyPath.Last(), valueString, isWString);
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string name, int valueInt32)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        property.PropertyType = SteamAppPropertyType.Int32;
        property.ValueInt32 = valueInt32;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(int valueInt32, params string[] propertyPath)
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
                propertyTable.SetPropertyValue(item, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                //return false;
                return;
            }
        }
        /*return*/
        propertyTable.SetPropertyValue(propertyPath.Last(), valueInt32);
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string name, float valueSingle)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        property.PropertyType = SteamAppPropertyType.Float;
        property.ValueSingle = valueSingle;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(float valueSingle, params string[] propertyPath)
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
                propertyTable.SetPropertyValue(item, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                //return false;
                return;
            }
        }
        /*return*/
        propertyTable.SetPropertyValue(propertyPath.Last(), valueSingle);
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string name, SDColor valueColor)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        property.PropertyType = SteamAppPropertyType.Color;
        property.ValueColor = valueColor;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(SDColor valueColor, params string[] propertyPath)
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
                propertyTable.SetPropertyValue(item, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                //return false;
                return;
            }
        }
        /*return*/
        propertyTable.SetPropertyValue(propertyPath.Last(), valueColor);
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(string name, ulong valueUInt64)
    {
        SteamAppProperty? property = this[name];
        if (property == null)
        {
            property = new SteamAppProperty(name);
            _properties.Add(property);
        }
        property.PropertyType = SteamAppPropertyType.Uint64;
        property.ValueUInt64 = valueUInt64;
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    public void SetPropertyValue(ulong valueUInt64, params string[] propertyPath)
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
                propertyTable.SetPropertyValue(item, propertyTable2);
                propertyTable = propertyTable2;
            }
            if (propertyTable == null)
            {
                //return false;
                return;
            }
        }
        /*return*/
        propertyTable.SetPropertyValue(propertyPath.Last(), valueUInt64);
    }

    /// <summary>
    /// 根据属性名称删除属性
    /// </summary>
    /// <param name="name"></param>
    public void RemoveProperty(string name)
    {
        _properties.RemoveAll(prop => prop.Name == name);
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
                propertyTable.SetPropertyValue(item, propertyTable2);
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
    [Obsolete("存在未释放的 MemoryStream")]
    public object? ExtractProperty(string name)
    {
        SteamAppProperty? property = this[name];
        if (property != null)
        {
            var propertyTable = new SteamAppPropertyTable();
            var pNew = new SteamAppProperty();
            pNew.SetCopyValue(property, isCreateNewTable: false);
            propertyTable.SetPropertyValue(pNew);
            var returnMemoryStream = new MemoryStream(); // 返回对象，不释放
            using var w = new BinaryWriter(returnMemoryStream, Encoding.UTF8, true);
            w.Write(propertyTable);
            return returnMemoryStream;
        }
        return null;
    }

    /// <summary>
    /// 将已导出的属性添加到 <see cref="SteamAppPropertyTable"/>
    /// </summary>
    /// <param name="extracted"></param>
    /// <returns></returns>
    [Obsolete("存在未释放的 MemoryStream")]
    public string AddExtractedProperty(object extracted)
    {
        if (extracted is MemoryStream memoryStream)
        {
            using var r = new BinaryReader(memoryStream, Encoding.UTF8, true);
            SteamAppProperty property = r.ReadPropertyTable()._properties[0];
            RemoveProperty(property.Name);
            _properties.Add(property);
            return property.Name;

        }
        return string.Empty;
    }

    /// <summary>
    /// 清空属性
    /// </summary>
    public void Clear()
    {
        _properties.Clear();
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
            return _properties.All(prop => other._properties.Any(otherProp => prop.Equals(otherProp)));
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

    string ToStringInternal(int indent)
    {
        StringBuilder stringBuilder = new();
        string arg = new string('\t', indent);
        foreach (SteamAppProperty property in _properties)
        {
            string arg2 = property.Name.Replace("\\", "\\\\").Replace("\"", "\\\"");
            stringBuilder.AppendFormat("{0}\"{1}\"", arg, arg2);
            switch (property.PropertyType)
            {
                case SteamAppPropertyType.Table:
                    stringBuilder.AppendFormat("\n{0}{{\n{1}{0}}}", arg,
                        property.ValueTable?.ToStringInternal(indent + 1));
                    break;
                case SteamAppPropertyType.String:
                case SteamAppPropertyType.WString:
                    stringBuilder.AppendFormat("\t\t\"{0}\"",
                        property.ValueString?.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    break;
                case SteamAppPropertyType.Int32:
                    stringBuilder.AppendFormat("\t\t\"{0}\"", property.ValueInt32);
                    break;
                case SteamAppPropertyType.Float:
                    stringBuilder.AppendFormat("\t\t\"{0}\"", property.ValueSingle);
                    break;
                case SteamAppPropertyType.Color:
                    stringBuilder.AppendFormat("\t\t\"{0}\"", property.ValueColor);
                    break;
                case SteamAppPropertyType.Uint64:
                    stringBuilder.AppendFormat("\t\t\"{0}\"", property.ValueUInt64);
                    break;
                default:
                    throw new NotImplementedException(
                        $"The property type {property.PropertyType} is invalid.");
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
