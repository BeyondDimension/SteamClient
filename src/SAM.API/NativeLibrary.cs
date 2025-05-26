#if !NETCOREAPP3_0_OR_GREATER
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Runtime.InteropServices;

static partial class NativeLibrary
{
    //public static nint Load(string libraryPath)
    //{
    //    return default;
    //}

    //public static nint GetExport(nint handle, string name)
    //{
    //    return default;
    //}

    const uint LoadWithAlteredSearchPath = 8;

    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryEx", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint Load(string path, nint file = default, uint flags = LoadWithAlteredSearchPath);

    [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern nint GetExport(nint module, string name);
}
#endif