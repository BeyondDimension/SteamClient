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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamRemoteStorage014
{
    public nint FileWrite;
    public nint FileRead;
    public nint FileForget;
    public nint FileDelete;
    public nint FileShare4;
    public nint SetSyncPlatforms;
    public nint FileWriteStreamOpen6;
    public nint FileWriteStreamWriteChunk7;
    public nint FileWriteStreamClose8;
    public nint FileWriteStreamCancel9;
    public nint FileExists;
    public nint FilePersisted;
    public nint GetFileSize;
    public nint GetFileTimestamp;
    public nint GetSyncPlatforms;
    public nint GetFileCount;
    public nint GetFileNameAndSize;
    public nint GetQuota;
    public nint IsCloudEnabledForAccount;
    public nint IsCloudEnabledForApp;
    public nint SetCloudEnabledForApp;
    public nint UGCDownload;
    public nint GetUGCDownloadProgress;
    public nint GetUGCDetails;
    public nint UGCRead;
    public nint GetCachedUGCCount25;
    public nint GetCachedUGCHandle26;
    public nint PublishWorkshopFile27;
    public nint CreatePublishedFileUpdateRequest28;
    public nint UpdatePublishedFileFile29;
    public nint UpdatePublishedFilePreviewFile30;
    public nint UpdatePublishedFileTitle31;
    public nint UpdatePublishedFileDescription32;
    public nint UpdatePublishedFileVisibility33;
    public nint UpdatePublishedFileTags34;
    public nint CommitPublishedFileUpdate35;
    public nint GetPublishedFileDetails36;
    public nint DeletePublishedFile37;
    public nint EnumerateUserPublishedFiles38;
    public nint SubscribePublishedFile39;
    public nint EnumerateUserSubscribedFiles40;
    public nint UnsubscribePublishedFile41;
    public nint UpdatePublishedFileSetChangeDescription42;
    public nint GetPublishedItemVoteDetails43;
    public nint UpdateUserPublishedItemVote44;
    public nint GetUserPublishedItemVoteDetails45;
    public nint EnumerateUserSharedWorkshopFiles46;
    public nint PublishVideo47;
    public nint SetUserPublishedFileAction48;
    public nint EnumeratePublishedFilesByUserAction49;
    public nint EnumeratePublishedWorkshopFiles50;
    public nint UGCDownloadToLocation51;
    private nint DTorISteamRemoteStorage01252;
}