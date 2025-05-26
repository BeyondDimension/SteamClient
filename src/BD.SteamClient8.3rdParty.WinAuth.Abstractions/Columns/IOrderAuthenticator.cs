using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.WinAuth.Columns;

/// <summary>
/// 可排序的身份验证器
/// </summary>
public interface IOrderAuthenticator : IExplicitHasValue
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