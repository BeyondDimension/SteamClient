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
public struct ISteamRemoteStorage014
{
    public IntPtr FileWrite;
    public IntPtr FileRead;
    public IntPtr FileForget;
    public IntPtr FileDelete;
    public IntPtr FileShare4;
    public IntPtr SetSyncPlatforms;
    public IntPtr FileWriteStreamOpen6;
    public IntPtr FileWriteStreamWriteChunk7;
    public IntPtr FileWriteStreamClose8;
    public IntPtr FileWriteStreamCancel9;
    public IntPtr FileExists;
    public IntPtr FilePersisted;
    public IntPtr GetFileSize;
    public IntPtr GetFileTimestamp;
    public IntPtr GetSyncPlatforms;
    public IntPtr GetFileCount;
    public IntPtr GetFileNameAndSize;
    public IntPtr GetQuota;
    public IntPtr IsCloudEnabledForAccount;
    public IntPtr IsCloudEnabledForApp;
    public IntPtr SetCloudEnabledForApp;
    public IntPtr UGCDownload;
    public IntPtr GetUGCDownloadProgress;
    public IntPtr GetUGCDetails;
    public IntPtr UGCRead;
    public IntPtr GetCachedUGCCount25;
    public IntPtr GetCachedUGCHandle26;
    public IntPtr PublishWorkshopFile27;
    public IntPtr CreatePublishedFileUpdateRequest28;
    public IntPtr UpdatePublishedFileFile29;
    public IntPtr UpdatePublishedFilePreviewFile30;
    public IntPtr UpdatePublishedFileTitle31;
    public IntPtr UpdatePublishedFileDescription32;
    public IntPtr UpdatePublishedFileVisibility33;
    public IntPtr UpdatePublishedFileTags34;
    public IntPtr CommitPublishedFileUpdate35;
    public IntPtr GetPublishedFileDetails36;
    public IntPtr DeletePublishedFile37;
    public IntPtr EnumerateUserPublishedFiles38;
    public IntPtr SubscribePublishedFile39;
    public IntPtr EnumerateUserSubscribedFiles40;
    public IntPtr UnsubscribePublishedFile41;
    public IntPtr UpdatePublishedFileSetChangeDescription42;
    public IntPtr GetPublishedItemVoteDetails43;
    public IntPtr UpdateUserPublishedItemVote44;
    public IntPtr GetUserPublishedItemVoteDetails45;
    public IntPtr EnumerateUserSharedWorkshopFiles46;
    public IntPtr PublishVideo47;
    public IntPtr SetUserPublishedFileAction48;
    public IntPtr EnumeratePublishedFilesByUserAction49;
    public IntPtr EnumeratePublishedWorkshopFiles50;
    public IntPtr UGCDownloadToLocation51;
    private IntPtr DTorISteamRemoteStorage01252;
}