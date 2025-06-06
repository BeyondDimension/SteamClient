#if !(IOS || ANDROID || MACCATALYST)
using System.Runtime.InteropServices;
#endif

namespace BD.SteamClient8.Services.Abstractions.PInvoke;

/// <summary>
/// Steamworks 本地 API 服务，使用 PInvoke 调用 Steam 安装目录中的本机库，需要当前进程的 ABI 与 Steam 客户端内提供的本机库 ABI 一致，目前 Steam 客户端仅在 macOS 上支持 ARM64 架构，其他平台仅支持 x86 和 x64 架构
/// <para>steamclient64.dll, steamclient.dll, steamclient.dylib, steamclient.so</para>
/// <para>https://partner.steamgames.com/doc/home</para>
/// </summary>
public partial interface ISteamworksLocalApiService
{
    /// <summary>
    /// 当前平台是否支持
    /// </summary>
    static bool IsSupported =>
#if !(IOS || ANDROID || MACCATALYST)
        IsSupported_.V;
#else
        false;
#endif
}

#if !(IOS || ANDROID || MACCATALYST)
public static class SteamworksLocalApiServiceExtensions
{
    /// <summary>
    /// 可设置当前平台是否支持 <see cref="ISteamworksLocalApiService"/>
    /// </summary>
    /// <param name="v"></param>
    public static void SetIsSupported(bool? v) => IsSupported_.v = v;
}

file static class IsSupported_
{
    internal static bool? v;

    internal static bool V
    {
        get
        {
            if (!v.HasValue)
            {
                v = ReadOnlyIsSupported_.v;
            }
            return v.Value;
        }
    }

}

file static class ReadOnlyIsSupported_
{
    internal static readonly bool v;

    static ReadOnlyIsSupported_()
    {
        v = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => true,
            Architecture.X64 => true,
#if MACOS
            Architecture.Arm64 => true, // Steam 的 ARM64 仅支持 Apple M 系列
#endif
            _ => false,
        };
    }
}
#endif