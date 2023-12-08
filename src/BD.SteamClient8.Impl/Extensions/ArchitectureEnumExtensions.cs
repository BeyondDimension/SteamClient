namespace BD.SteamClient8.Impl.Extensions;

/// <summary>
/// Enum 扩展 System.Runtime.InteropServices.Architecture
/// </summary>
public static class ArchitectureEnumExtensions
{
    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.Architecture.X86 或 System.Runtime.InteropServices.Architecture.X64
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsX86OrX64(this Architecture value)
    {
        if (value != Architecture.X64)
        {
            return value == Architecture.X86;
        }

        return true;
    }

    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.Architecture.Arm 或 System.Runtime.InteropServices.Architecture.Arm64
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsArmOrArm64(this Architecture value)
    {
        if (value != Architecture.Arm)
        {
            return value == Architecture.Arm64;
        }

        return true;
    }

    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.Architecture.Arm 或 System.Runtime.InteropServices.Architecture.Arm64
    ///     或 System.Runtime.InteropServices.ArchitectureEnumExtensions.Architecture_Armv6
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsArmOrArm64OrArmv6(this Architecture value)
    {
        if (value != Architecture.Arm && value != Architecture.Arm64)
        {
            return value == Architecture.Armv6;
        }

        return true;
    }

    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.ArchitectureEnumExtensions.Architecture_Wasm
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWasm(this Architecture value)
    {
        return value == Architecture.Wasm;
    }

    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.ArchitectureEnumExtensions.Architecture_S390x
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsS390x(this Architecture value)
    {
        return value == Architecture.S390x;
    }

    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.ArchitectureEnumExtensions.Architecture_LoongArch64
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLoongArch64(this Architecture value)
    {
        return value == Architecture.LoongArch64;
    }

    /// <summary>
    /// 处理器体系结构是否为 System.Runtime.InteropServices.Architecture.Armv6
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsArmv6(this Architecture value)
    {
        return value == Architecture.Armv6;
    }
}
