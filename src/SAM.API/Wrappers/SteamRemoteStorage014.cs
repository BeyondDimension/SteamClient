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
using SAM.API.Types;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers;

public class SteamRemoteStorage012 : NativeWrapper<ISteamRemoteStorage014>
{
    #region FileWrite
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileWriteSBI(IntPtr self, IntPtr pchFile, Byte[] pvData, int cubData);

    public bool FileWrite(string pchFile, Byte[] pvData)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return GetFunction<NativeFileWriteSBI>(Functions.FileWrite)
        (ObjectAddress, nativeName.Handle, pvData, pvData.Length);
    }
    #endregion

    #region FileWrite
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeFileReadSBI(IntPtr thisptr, IntPtr pchFile, Byte[] pvData, int cubDataToRead);

    public int FileRead(string pchFile, Byte[] pvData)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return GetFunction<NativeFileReadSBI>(Functions.FileRead)
            (ObjectAddress, nativeName.Handle, pvData, pvData.Length);
    }
    #endregion

    #region FileForget
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileForgetS(IntPtr thisptr, IntPtr pchFile);

    public bool FileForget(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFileForgetS>(Functions.FileForget, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region FileDelete
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileDeleteS(IntPtr thisptr, IntPtr pchFile);

    public bool FileDelete(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFileDeleteS>(Functions.FileDelete, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region SetSyncPlatforms
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeSetSyncPlatformsSE(IntPtr thisptr, IntPtr pchFile, ERemoteStoragePlatform eRemoteStoragePlatform);

    public bool SetSyncPlatforms(string pchFile, ERemoteStoragePlatform eRemoteStoragePlatform)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeSetSyncPlatformsSE>(Functions.SetSyncPlatforms, ObjectAddress, nativeName.Handle, eRemoteStoragePlatform);
    }
    #endregion

    #region FileExists
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileExistsS(IntPtr thisptr, IntPtr pchFile);

    public bool FileExists(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFileExistsS>(Functions.FileExists, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region FilePersisted
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFilePersistedS(IntPtr thisptr, IntPtr pchFile);

    public bool FilePersisted(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFilePersistedS>(Functions.FilePersisted, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetFileSize
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetFileSizeS(IntPtr thisptr, IntPtr pchFile);

    public int GetFileSize(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<int, NativeGetFileSizeS>(Functions.GetFileSize, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetFileTimestamp
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate long NativeGetFileTimestampS(IntPtr thisptr, IntPtr pchFile);

    public long GetFileTimestamp(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<long, NativeGetFileTimestampS>(Functions.GetFileTimestamp, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetSyncPlatforms
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate ERemoteStoragePlatform NativeGetSyncPlatformsS(IntPtr thisptr, IntPtr pchFile);

    public ERemoteStoragePlatform GetSyncPlatforms(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<ERemoteStoragePlatform, NativeGetSyncPlatformsS>(Functions.GetSyncPlatforms, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetFileCount
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetFileCount(IntPtr thisptr);

    public int GetFileCount()
    {
        return Call<int, NativeGetFileCount>(Functions.GetFileCount, ObjectAddress);
    }
    #endregion

    #region GetFileNameAndSize
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetFileNameAndSizeII(IntPtr thisptr, int iFile, out int pnFileSizeInBytes);

    public string GetFileNameAndSize(int iFile, out int pnFileSizeInBytes)
    {
        return NativeStrings.PointerToString(
            GetFunction<NativeGetFileNameAndSizeII>(
                Functions.GetFileNameAndSize)(ObjectAddress, iFile, out pnFileSizeInBytes));
    }
    #endregion

    #region GetQuota
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeGetQuota(IntPtr thisptr, out ulong pnTotalBytes, out ulong puAvailableBytes);

    public bool GetQuota(out ulong pnTotalBytes, out ulong puAvailableBytes)
    {
        return GetFunction<NativeGetQuota>(Functions.GetQuota)(ObjectAddress, out pnTotalBytes, out puAvailableBytes);
    }
    #endregion

    #region IsCloudEnabledForAccount
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeIsCloudEnabledForAccount(IntPtr thisptr);


    public bool IsCloudEnabledForAccount()
    {
        return Call<bool, NativeIsCloudEnabledForAccount>(Functions.IsCloudEnabledForAccount, ObjectAddress);
    }
    #endregion

    #region IsCloudEnabledForApp
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeIsCloudEnabledForApp(IntPtr thisptr);

    public bool IsCloudEnabledForApp()
    {
        return Call<bool, NativeIsCloudEnabledForApp>(Functions.IsCloudEnabledForApp, ObjectAddress);
    }
    #endregion

    #region SetCloudEnabledForApp
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeSetCloudEnabledForAppB(IntPtr thisptr, [MarshalAs(UnmanagedType.I1)] bool bEnabled);

    public void SetCloudEnabledForApp(bool bEnabled)
    {
        Call<NativeSetCloudEnabledForAppB>(Functions.SetCloudEnabledForApp, ObjectAddress, bEnabled);
    }
    #endregion

}