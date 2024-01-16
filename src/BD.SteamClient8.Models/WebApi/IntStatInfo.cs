#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 整数类型统计
/// </summary>
public record class IntStatInfo : StatInfo
{
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
    public override object Value
    {
        get => IntValue;
        set
        {
            var b = int.TryParse((string)value, out int i);
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