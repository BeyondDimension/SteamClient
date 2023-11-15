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

namespace SAM.API;

#pragma warning disable SA1600 // Elements should be documented

public static class Steam
{
    static TDelegate? GetExportFunction<TDelegate>(nint module, string name)
      where TDelegate : class
    {
        nint address = NativeLibrary.GetExport(module, name);
        return address == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
    }

    static nint _Handle;

    public static Func<string?>? GetInstallPathDelegate { private get; set; }

    public static string? GetInstallPath()
    {
        if (GetInstallPathDelegate != null)
            return GetInstallPathDelegate();
        if (OperatingSystem.IsWindows())
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null)?.ToString();
        else
            throw new PlatformNotSupportedException();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate nint NativeCreateInterface(string version, nint returnCode);

    static NativeCreateInterface? _CallCreateInterface;

    public static TClass? CreateInterface<TClass>(string version)
        where TClass : INativeWrapper, new()
    {
        var address = _CallCreateInterface!(version, nint.Zero);

        if (address == nint.Zero)
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
            if (_Handle != nint.Zero)
            {
                return true;
            }

            var path = GetInstallPath();
            if (path == null)
            {
                return false;
            }

            if (OperatingSystem.IsMacOS())
            {
                path = Path.Combine(path, "steamclient.dylib");
            }
            else if (OperatingSystem.IsWindows())
            {
                // C:\Program Files (x86)\Steam\steamclient64.dll
                path = Path.Combine(path,
                    Environment.Is64BitProcess ?
                        "steamclient64.dll" :
                        "steamclient.dll");
            }
            else if (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid())
            {
                // /home/{0}/.local/share/Steam/linux64/steamclient.so
                path = Path.Combine(path,
                    Environment.Is64BitProcess ?
                        "linux64" :
                        "linux32",
                    "steamclient.so");
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            var module = NativeLibrary.Load(path);
            if (module == nint.Zero)
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