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

        var installPath = Helpers.GetSteamDirPath();
        return installPath;
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

            string? path;
            if (GetSteamClientNativeLibraryPathDelegate != null)
            {
                path = GetSteamClientNativeLibraryPathDelegate();
                if (string.IsNullOrWhiteSpace(path))
                {
                    return false;
                }
            }
            else
            {
                path = Helpers.GetSteamClientNativeLibraryPath();
                if (string.IsNullOrWhiteSpace(path))
                {
                    return false;
                }
            }

            var module = NativeLibrary.Load(path!);
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
}