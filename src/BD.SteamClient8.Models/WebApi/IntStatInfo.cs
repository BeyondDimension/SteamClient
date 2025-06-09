using BD.Common8.Models.Abstractions;
using System.Text.Json;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 整数类型统计
/// </summary>
public sealed record class IntStatInfo : StatInfo, IJsonModel<IntStatInfo>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 原始数值
    /// </summary>
    public int OriginalValue { get; set; }

    /// <summary>
    /// 整型数值
    /// </summary>
    public int IntValue { get; set; }

    /// <summary>
    /// 最小数值
    /// </summary>
    public int MinValue { get; set; }

    /// <summary>
    /// 最大数值
    /// </summary>
    public int MaxValue { get; set; }

    /// <summary>
    /// 最大修改量
    /// </summary>
    public int MaxChange { get; set; }

    /// <summary>
    /// 仅允许增量
    /// </summary>
    public bool IncrementOnly { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public int DefaultValue { get; set; }

    /// <summary>
    /// 当前数值
    /// </summary>
    [global::System.Runtime.Serialization.IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override object Value
    {
        get => IntValue;
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
            var b = int.TryParse(strValue, out int i);
            if (b)
            {
                if ((Permission & 2) != 0 && IntValue != i)
                {
                    //this.IntValue = this.IntValue;
                }
                else
                {
                    IntValue = i;
                }
            }
        }
    }

    /// <summary>
    /// 是否已修改
    /// </summary>
    public override bool IsModified => IntValue != OriginalValue;
}