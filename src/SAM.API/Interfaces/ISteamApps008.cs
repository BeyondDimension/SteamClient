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

namespace SAM.API.Interfaces;

#pragma warning disable SA1600 // Elements should be documented

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamApps008
{
    public nint IsSubscribed;
    public nint IsLowViolence;
    public nint IsCybercafe;
    public nint IsVACBanned;
    public nint GetCurrentGameLanguage;
    public nint GetAvailableGameLanguages;
    public nint IsSubscribedApp;
    public nint IsDlcInstalled;
    public nint GetEarliestPurchaseUnixTime;
    public nint IsSubscribedFromFreeWeekend;
    public nint GetDLCCount;
    public nint GetDLCDataByIndex;
    public nint InstallDLC;
    public nint UninstallDLC;
    public nint RequestAppProofOfPurchaseKey;
    public nint GetCurrentBetaName;
    public nint MarkContentCorrupt;
    public nint GetInstalledDepots;
    public nint GetAppInstallDir;
    public nint IsAppInstalled;
    public nint GetAppOwner;
    public nint GetLaunchQueryParam;
    public nint GetDlcDownloadProgress;
    public nint GetAppBuildId;
    public nint RequestAllProofOfPurchaseKeys;
    public nint GetFileDetails;
    public nint GetLaunchCommandLine;
    public nint IsSubscribedFromFamilySharing;
}