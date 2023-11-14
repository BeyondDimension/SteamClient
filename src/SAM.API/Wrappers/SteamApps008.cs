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

using SAM.API.Interfaces;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers;

public class SteamApps008 : NativeWrapper<ISteamApps008>
{
    #region IsSubscribed
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedApp(IntPtr self, uint appID);

    public bool IsSubscribedApp(uint appID)
    {
        return Call<bool, NativeIsSubscribedApp>(Functions.IsSubscribedApp, ObjectAddress, appID);
    }
    #endregion

    #region GetCurrentGameLanguage
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetCurrentGameLanguage(IntPtr self);

    public string GetCurrentGameLanguage()
    {
        var languagePointer = Call<IntPtr, NativeGetCurrentGameLanguage>(
            Functions.GetCurrentGameLanguage,
            ObjectAddress);
        return NativeStrings.PointerToString(languagePointer);
    }
    #endregion

    #region GetAvailableGameLanguages
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetAvailableGameLanguages(IntPtr self);

    public string GetAvailableGameLanguages()
    {
        var languagePointer = Call<IntPtr, NativeGetAvailableGameLanguages>(
            Functions.GetAvailableGameLanguages,
            ObjectAddress);
        return NativeStrings.PointerToString(languagePointer);
    }

    #endregion

    #region IsDlcInstalled
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsDlcInstalled(IntPtr self, uint appid);
    public bool IsDlcInstalled(uint appid)
    {
        return Call<bool, NativeIsDlcInstalled>(Functions.IsDlcInstalled, ObjectAddress, appid);
    }
    #endregion

    #region IsAppInstalled
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsAppInstalled(IntPtr self, uint appid);
    public bool IsAppInstalled(uint appid)
    {
        return Call<bool, NativeIsAppInstalled>(Functions.IsAppInstalled, ObjectAddress, appid);
    }
    #endregion

    #region IsSubscribedFromFamilySharing
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedFromFamilySharing(IntPtr self);
    public bool IsSubscribedFromFamilySharing()
    {
        return Call<bool, NativeIsSubscribedFromFamilySharing>(Functions.IsSubscribedFromFamilySharing, ObjectAddress);
    }
    #endregion

    #region IsSubscribedFromFreeWeekend
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedFromFreeWeekend(IntPtr self);
    public bool IsSubscribedFromFreeWeekend()
    {
        return Call<bool, NativeIsSubscribedFromFreeWeekend>(Functions.IsSubscribedFromFreeWeekend, ObjectAddress);
    }
    #endregion

    #region GetAppInstallDir
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetAppInstallDir(IntPtr self, uint appid, IntPtr pchFolder, uint cchFolderBufferSize);
    public string GetAppInstallDir(uint appID)
    {
        IntPtr mempchFolder = Helpers.TakeMemory();
        string pchFolder;
        GetFunction<NativeGetAppInstallDir>(Functions.GetAppInstallDir)(ObjectAddress, appID, mempchFolder, (1024 * 32));
        pchFolder = Helpers.MemoryToString(mempchFolder);
        return pchFolder;
    }
    #endregion
}