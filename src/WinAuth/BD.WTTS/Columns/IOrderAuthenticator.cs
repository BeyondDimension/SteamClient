#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Columns;

/// <summary>
/// 可排序的身份验证器
/// </summary>
public interface IOrderAuthenticator
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    ushort Id { get; set; }

    /// <summary>
    /// 索引
    /// </summary>
    int Index { get; set; }
}

[Obsolete("use IOrderAuthenticator", true)]
#pragma warning disable SA1600 // Elements should be documented
public interface IOrderGAPAuthenticator : IOrderAuthenticator
#pragma warning restore SA1600 // Elements should be documented
{
}