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

namespace SAM.API.Wrappers;

using ERemoteStoragePlatform = SAM.API.Types.ERemoteStoragePlatform;

public class SteamRemoteStorage012 : NativeWrapper<ISteamRemoteStorage014>
{
    #region FileWrite
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileWriteSBI(nint self, nint pchFile, byte[] pvData, int cubData);

    public bool FileWrite(string pchFile, byte[] pvData)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return GetFunction<NativeFileWriteSBI>(Functions.FileWrite)(
        ObjectAddress, nativeName.Handle, pvData, pvData.Length);
    }
    #endregion

    #region FileWrite
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeFileReadSBI(nint thisptr, nint pchFile, byte[] pvData, int cubDataToRead);

    public int FileRead(string pchFile, byte[] pvData)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return GetFunction<NativeFileReadSBI>(Functions.FileRead)(
            ObjectAddress, nativeName.Handle, pvData, pvData.Length);
    }
    #endregion

    #region FileForget
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileForgetS(nint thisptr, nint pchFile);

    public bool FileForget(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFileForgetS>(Functions.FileForget, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region FileDelete
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileDeleteS(nint thisptr, nint pchFile);

    public bool FileDelete(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFileDeleteS>(Functions.FileDelete, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region SetSyncPlatforms
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeSetSyncPlatformsSE(nint thisptr, nint pchFile, ERemoteStoragePlatform eRemoteStoragePlatform);

    public bool SetSyncPlatforms(string pchFile, ERemoteStoragePlatform eRemoteStoragePlatform)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeSetSyncPlatformsSE>(Functions.SetSyncPlatforms, ObjectAddress, nativeName.Handle, eRemoteStoragePlatform);
    }
    #endregion

    #region FileExists
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFileExistsS(nint thisptr, nint pchFile);

    public bool FileExists(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFileExistsS>(Functions.FileExists, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region FilePersisted
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeFilePersistedS(nint thisptr, nint pchFile);

    public bool FilePersisted(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<bool, NativeFilePersistedS>(Functions.FilePersisted, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetFileSize
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetFileSizeS(nint thisptr, nint pchFile);

    public int GetFileSize(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<int, NativeGetFileSizeS>(Functions.GetFileSize, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetFileTimestamp
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate long NativeGetFileTimestampS(nint thisptr, nint pchFile);

    public long GetFileTimestamp(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<long, NativeGetFileTimestampS>(Functions.GetFileTimestamp, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetSyncPlatforms
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate ERemoteStoragePlatform NativeGetSyncPlatformsS(nint thisptr, nint pchFile);

    public ERemoteStoragePlatform GetSyncPlatforms(string pchFile)
    {
        using var nativeName = NativeStrings.StringToStringHandle(pchFile);
        return Call<ERemoteStoragePlatform, NativeGetSyncPlatformsS>(Functions.GetSyncPlatforms, ObjectAddress, nativeName.Handle);
    }
    #endregion

    #region GetFileCount
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetFileCount(nint thisptr);

    public int GetFileCount()
    {
        return Call<int, NativeGetFileCount>(Functions.GetFileCount, ObjectAddress);
    }
    #endregion

    #region GetFileNameAndSize
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate nint NativeGetFileNameAndSizeII(nint thisptr, int iFile, out int pnFileSizeInBytes);

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
    private delegate bool NativeGetQuota(nint thisptr, out ulong pnTotalBytes, out ulong puAvailableBytes);

    public bool GetQuota(out ulong pnTotalBytes, out ulong puAvailableBytes)
    {
        return GetFunction<NativeGetQuota>(Functions.GetQuota)(ObjectAddress, out pnTotalBytes, out puAvailableBytes);
    }
    #endregion

    #region IsCloudEnabledForAccount
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeIsCloudEnabledForAccount(nint thisptr);

    public bool IsCloudEnabledForAccount()
    {
        return Call<bool, NativeIsCloudEnabledForAccount>(Functions.IsCloudEnabledForAccount, ObjectAddress);
    }
    #endregion

    #region IsCloudEnabledForApp
    [return: MarshalAs(UnmanagedType.I1)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate bool NativeIsCloudEnabledForApp(nint thisptr);

    public bool IsCloudEnabledForApp()
    {
        return Call<bool, NativeIsCloudEnabledForApp>(Functions.IsCloudEnabledForApp, ObjectAddress);
    }
    #endregion

    #region SetCloudEnabledForApp
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeSetCloudEnabledForAppB(nint thisptr, [MarshalAs(UnmanagedType.I1)] bool bEnabled);

    public void SetCloudEnabledForApp(bool bEnabled)
    {
        Call<NativeSetCloudEnabledForAppB>(Functions.SetCloudEnabledForApp, ObjectAddress, bEnabled);
    }
    #endregion

}