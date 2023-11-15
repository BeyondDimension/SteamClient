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
public struct ISteamUGC015
{
    public nint CreateQueryUserUGCRequest0;
    public nint CreateQueryAllUGCRequest1;
    public nint SendQueryUGCRequest2;
    public nint GetQueryUGCResult3;
    public nint ReleaseQueryUGCRequest4;
    public nint AddRequiredTag5;
    public nint AddExcludedTag6;
    public nint SetReturnLongDescription7;
    public nint SetReturnTotalOnly8;
    public nint SetAllowCachedResponse9;
    public nint SetCloudFileNameFilter10;
    public nint SetMatchAnyTag11;
    public nint SetSearchText12;
    public nint SetRankedByTrendDays13;
    public nint RequestUGCDetails14;
    public nint CreateItem15;
    public nint StartItemUpdate16;
    public nint SetItemTitle17;
    public nint SetItemDescription18;
    public nint SetItemVisibility19;
    public nint SetItemTags20;
    public nint SetItemContent21;
    public nint SetItemPreview22;
    public nint SubmitItemUpdate23;
    public nint GetItemUpdateProgress24;
    public nint SubscribeItem25;
    public nint UnsubscribeItem26;
    public nint GetNumSubscribedItems27;
    public nint GetSubscribedItems28;
    public nint GetItemInstallInfo29;
    public nint GetItemUpdateInfo30;
    private nint DTorISteamUGC00331;
}