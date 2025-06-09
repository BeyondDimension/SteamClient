using BD.Common8.Models.Abstractions;
using System.Text.Json;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 浮点类型统计
/// </summary>
public sealed record class FloatStatInfo : StatInfo, IJsonModel<FloatStatInfo>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 原始数值
    /// </summary>
    public float OriginalValue { get; set; }

    /// <summary>
    /// 浮点数值
    /// </summary>
    public float FloatValue { get; set; }

    /// <summary>
    /// 最小数值
    /// </summary>
    public float MinValue { get; set; }

    /// <summary>
    /// 最大数值
    /// </summary>
    public float MaxValue { get; set; }

    /// <summary>
    /// 最大修改量
    /// </summary>
    public float MaxChange { get; set; }

    /// <summary>
    /// 仅允许增量
    /// </summary>
    public bool IncrementOnly { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public float DefaultValue { get; set; }

    /// <summary>
    /// 当前数值
    /// </summary>
    [global::System.Runtime.Serialization.IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override object Value
    {
        get => FloatValue;
        set
        {
            string? strValue;
            if (value is string s)
            {
                strValue = s;
            }
            else if (value is JsonElement element)
            {
                strValue = element.GetString();
            }
            else
            {
                strValue = value?.ToString();
            }
            var b = float.TryParse(strValue, out float f);
            if (b)
            {
                if ((Permission & 2) != 0 && FloatValue != f)
                {
                    //this.FloatValue = this.FloatValue;
                }
                else
                {
                    FloatValue = f;
                }
            }
        }
    }

    /// <summary>
    /// 是否已修改
    /// </summary>
    public override bool IsModified => !FloatValue.Equals(OriginalValue);
}