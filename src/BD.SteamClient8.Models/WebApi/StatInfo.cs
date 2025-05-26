using BD.Common8.Models.Abstractions;
using System.ComponentModel;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 统计抽象类
/// </summary>
public abstract record class StatInfo : JsonRecordModel<StatInfo>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否已修改
    /// </summary>
    public abstract bool IsModified { get; }

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 展示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 数值
    /// </summary>
    public abstract object Value { get; set; }

    /// <summary>
    /// 仅允许增量
    /// </summary>
    public bool IsIncrementOnly { get; set; }

    /// <summary>
    /// 许可
    /// </summary>
    public int Permission { get; set; }

    /// <summary>
    /// 提取统计类型 Flags
    /// </summary>
    public string Extra
    {
        get
        {
            var flags = StatFlags.None;
            flags |= IsIncrementOnly == false ? 0 : StatFlags.IncrementOnly;
            flags |= (Permission & 2) != 0 == false ? 0 : StatFlags.Protected;
            flags |= (Permission & ~2) != 0 == false ? 0 : StatFlags.UnknownPermission;
            return flags.ToString();
        }
    }
}

/// <summary>
/// 统计类型 Flags 枚举
/// </summary>
[Flags]
public enum StatFlags
{
    /// <summary>
    /// 默认
    /// </summary>
    [Description("默认")]
    None = 0,

    /// <summary>
    /// 仅增加
    /// </summary>
    [Description("仅增加")]
    IncrementOnly = 1 << 0,

    /// <summary>
    /// 受保护
    /// </summary>
    [Description("受保护")]
    Protected = 1 << 1,

    /// <summary>
    /// 未知权限
    /// </summary>
    [Description("未知权限")]
    UnknownPermission = 1 << 2,
}