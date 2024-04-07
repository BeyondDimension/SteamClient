namespace BD.SteamClient8.Models;

/// <summary>
/// 浮点类型统计
/// </summary>
public record class FloatStatInfo : StatInfo
{
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
    public override object Value
    {
        get => FloatValue;
        set
        {
            var b = float.TryParse((string)value, out float f);
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