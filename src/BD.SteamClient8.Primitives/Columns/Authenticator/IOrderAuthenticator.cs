#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
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