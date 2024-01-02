#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Columns;

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