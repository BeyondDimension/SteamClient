using global::Microsoft.IO;

namespace BD.SteamClient8.Models;

/// <summary>
/// 内部不公开的静态内容
/// </summary>
static partial class Internals
{
    /// <summary>
    /// 当前项目使用的可回收内存流管理器
    /// </summary>
    internal static readonly RecyclableMemoryStreamManager M = new();
}
