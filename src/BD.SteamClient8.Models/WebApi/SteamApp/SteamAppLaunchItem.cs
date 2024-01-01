#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// <see cref="SteamApp"/> 已启动的游戏项
/// </summary>
public class SteamAppLaunchItem
{
    /// <summary>
    /// 描述
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 可执行
    /// </summary>
    public string? Executable { get; set; }

    /// <summary>
    /// 参数
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// 工作目录
    /// </summary>
    public string? WorkingDir { get; set; }

    /// <summary>
    /// 系统平台
    /// </summary>
    public string? Platform { get; set; }
}
