namespace SAM.API.Types;

#pragma warning disable SA1600 // Elements should be documented

public enum ERemoteStoragePlatform : int
{
    k_ERemoteStoragePlatformNone = 0,
    k_ERemoteStoragePlatformWindows = 1,
    k_ERemoteStoragePlatformOSX = 2,
    k_ERemoteStoragePlatformPS3 = 4,
    k_ERemoteStoragePlatformLinux = 8,
    k_ERemoteStoragePlatformReserved2 = 16,
    k_ERemoteStoragePlatformAll = -1,
}
