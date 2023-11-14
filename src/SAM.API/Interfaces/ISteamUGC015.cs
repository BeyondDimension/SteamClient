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

using System.Runtime.InteropServices;

namespace SAM.API.Interfaces;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamUGC015
{
    public IntPtr CreateQueryUserUGCRequest0;
    public IntPtr CreateQueryAllUGCRequest1;
    public IntPtr SendQueryUGCRequest2;
    public IntPtr GetQueryUGCResult3;
    public IntPtr ReleaseQueryUGCRequest4;
    public IntPtr AddRequiredTag5;
    public IntPtr AddExcludedTag6;
    public IntPtr SetReturnLongDescription7;
    public IntPtr SetReturnTotalOnly8;
    public IntPtr SetAllowCachedResponse9;
    public IntPtr SetCloudFileNameFilter10;
    public IntPtr SetMatchAnyTag11;
    public IntPtr SetSearchText12;
    public IntPtr SetRankedByTrendDays13;
    public IntPtr RequestUGCDetails14;
    public IntPtr CreateItem15;
    public IntPtr StartItemUpdate16;
    public IntPtr SetItemTitle17;
    public IntPtr SetItemDescription18;
    public IntPtr SetItemVisibility19;
    public IntPtr SetItemTags20;
    public IntPtr SetItemContent21;
    public IntPtr SetItemPreview22;
    public IntPtr SubmitItemUpdate23;
    public IntPtr GetItemUpdateProgress24;
    public IntPtr SubscribeItem25;
    public IntPtr UnsubscribeItem26;
    public IntPtr GetNumSubscribedItems27;
    public IntPtr GetSubscribedItems28;
    public IntPtr GetItemInstallInfo29;
    public IntPtr GetItemUpdateInfo30;
    private IntPtr DTorISteamUGC00331;
}