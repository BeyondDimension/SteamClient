/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using Microsoft.Win32;
using SAM.API.Types;
using System.Runtime.InteropServices;

namespace SAM.API;

public static partial class Steam
{
    static TDelegate? GetExportFunction<TDelegate>(nint module, string name)
      where TDelegate : class
    {
        nint address = NativeLibrary.GetExport(module, name);
        return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
    }

    static nint _Handle;

    public static string? GetInstallPath()
    {
        if (GetInstallPathDelegate != null)
        {
            return GetInstallPathDelegate();
        }
#if WINDOWS
#else
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#else
#endif
#else
        if (OperatingSystem.IsWindows())
#endif
#endif
        {
            return Registry.GetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                "SteamPath",
                null)?.ToString();
        }
#if !WINDOWS
        else
        {
            throw new PlatformNotSupportedException();
        }
#endif
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate nint NativeCreateInterface(string version, nint returnCode);

    static NativeCreateInterface? _CallCreateInterface;

    public static TClass? CreateInterface<TClass>(string version)
        where TClass : INativeWrapper, new()
    {
        var address = _CallCreateInterface!(version, IntPtr.Zero);

        if (address == IntPtr.Zero)
        {
            return default;
        }

        var rez = new TClass();
        rez.SetupFunctions(address);
        return rez;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    delegate bool NativeSteamGetCallback(int pipe, out CallbackMessage message, out int call);

    static NativeSteamGetCallback? _CallSteamBGetCallback;

    public static bool GetCallback(int pipe, out CallbackMessage message, out int call)
    {
        return _CallSteamBGetCallback!(pipe, out message, out call);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    delegate bool NativeSteamFreeLastCallback(int pipe);

    static NativeSteamFreeLastCallback? _CallSteamFreeLastCallback;

    public static bool FreeLastCallback(int pipe)
    {
        return _CallSteamFreeLastCallback!(pipe);
    }

    public static bool Load()
    {
        try
        {
            if (_Handle != IntPtr.Zero)
            {
                return true;
            }

            var path = GetInstallPath();
            if (path == null)
            {
                return false;
            }

            if (GetSteamClientNativeLibraryPathDelegate != null)
            {
                path = GetSteamClientNativeLibraryPathDelegate(path);
            }
            else
            {
#if WINDOWS
                path = GetSteamClientNativeLibraryPathByWindows(path);
#elif MACOS
                path = GetSteamClientNativeLibraryPathByMacOS(path);
#else
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
#else
#endif
#else
                if (OperatingSystem.IsMacOS())
#endif
                {
                    path = GetSteamClientNativeLibraryPathByMacOS(path);
                }
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#else
#endif
#else
                else if (OperatingSystem.IsWindows())
#endif
                {
                    path = GetSteamClientNativeLibraryPathByWindows(path);
                }
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
#else
#endif
#else
                else if (OperatingSystem.IsLinux())
#endif
                {
                    path = GetSteamClientNativeLibraryPathByLinux(path);
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
#endif
            }

            var module = NativeLibrary.Load(path);
            if (module == IntPtr.Zero)
            {
                return false;
            }

            _CallCreateInterface = GetExportFunction<NativeCreateInterface>(module, "CreateInterface");
            if (_CallCreateInterface == null)
            {
                return false;
            }

            _CallSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(module, "Steam_BGetCallback");
            if (_CallSteamBGetCallback == null)
            {
                return false;
            }

            _CallSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (_CallSteamFreeLastCallback == null)
            {
                return false;
            }

            _Handle = module;
            return true;
        }
        catch
        {
            return false;
        }
    }

#if !MACOS
    static string GetSteamClientNativeLibraryPathByWindows(string path)
    {
        // C:\Program Files (x86)\Steam\steamclient64.dll
        path = Path.Combine(path,
            Environment.Is64BitProcess ?
                "steamclient64.dll" :
                "steamclient.dll");
        return path;
    }
#endif
#if !WINDOWS
#if !MACOS
    static string GetSteamClientNativeLibraryPathByLinux(string path)
    {
        // /home/{0}/.local/share/Steam/linux64/steamclient.so
        path = Path.Combine(path,
            Environment.Is64BitProcess ?
                "linux64" :
                "linux32",
            "steamclient.so");
        return path;
    }
#endif
    static string GetSteamClientNativeLibraryPathByMacOS(string path)
    {
        path = Path.Combine(path, "steamclient.dylib");
        return path;
    }
#endif
}