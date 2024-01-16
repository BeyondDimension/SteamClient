#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Enums;

/// <summary>
/// Steam App 类型
/// </summary>
public enum SteamAppType : byte
{
    /// <summary>
    /// 未知/无效
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 可玩的游戏，默认类型
    /// </summary>
    Game = 1,

    /// <summary>
    /// 软件应用程序
    /// </summary>
    Application = 2,

    /// <summary>
    /// SDK、编辑器和专用服务器
    /// </summary>
    Tool = 3,

    /// <summary>
    /// 游戏试用版
    /// </summary>
    Demo = 4,

    /// <summary>
    /// 遗留程序，过去用于游戏宣传片。这些宣传片现在仅为网上的视频
    /// </summary>
    Media = 5,

    /// <summary>
    /// 可下载内容
    /// </summary>
    DLC = 6,

    /// <summary>
    /// 游戏指南、PDF 等
    /// </summary>
    Guide = 7,

    /// <summary>
    /// 硬件驱动程序更新程序（ATI、Razor 等）
    /// </summary>
    Driver = 8,

    /// <summary>
    /// 隐藏应用，用于配置 Steam 功能（背包、特卖等）
    /// </summary>
    Config = 9,

    /// <summary>
    /// Steam 硬件设备（Steam 主机、Steam 控制器、Steam 流式盒等）
    /// </summary>
    Hardware = 10,

    /// <summary>
    /// 如电影、剧集、游戏等多个应用集合的中心
    /// </summary>
    Franchise = 11,

    /// <summary>
    /// 电影或剧集的视频部分（可能是电影正片、剧集内容、预览、花絮等）
    /// </summary>
    Video = 12,

    /// <summary>
    /// 其他应用的插件类型
    /// </summary>
    Plugin = 13,

    /// <summary>
    /// 音乐文件
    /// </summary>
    Music = 14,

    /// <summary>
    /// 视频系列的容器应用
    /// </summary>
    Series = 15,

    /// <summary>
    /// 只是快捷方式，仅用于客户端
    /// </summary>
    Shortcut = 16,

    /// <summary>
    /// 由于 depot 和应用共享相同的命名空间，此为占位之用
    /// </summary>
    DepotOnly = 17,

    /// <summary>
    /// 模组
    /// </summary>
    Mod = 18,

    /// <summary>
    /// 测试版本
    /// </summary>
    Beta = 19,
}
