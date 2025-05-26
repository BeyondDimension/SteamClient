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
public struct ISteamClient018
{
    public nint CreateSteamPipe;
    public nint ReleaseSteamPipe;
    public nint ConnectToGlobalUser;
    public nint CreateLocalUser;
    public nint ReleaseUser;
    public nint GetISteamUser;
    public nint GetISteamGameServer;
    public nint SetLocalIPBinding;
    public nint GetISteamFriends;
    public nint GetISteamUtils;
    public nint GetISteamMatchmaking;
    public nint GetISteamMatchmakingServers;
    public nint GetISteamGenericInterface;
    public nint GetISteamUserStats;
    public nint GetISteamGameServerStats;
    public nint GetISteamApps;
    public nint GetISteamNetworking;
    public nint GetISteamRemoteStorage;
    public nint GetISteamScreenshots;
    public nint GetISteamGameSearch;
    public nint RunFrame;
    public nint GetIPCCallCount;
    public nint SetWarningMessageHook;
    public nint ShutdownIfAllPipesClosed;
    public nint GetISteamHTTP;
    public nint DEPRECATED_GetISteamUnifiedMessages;
    public nint GetISteamController;
    public nint GetISteamUGC;
    public nint GetISteamAppList;
    public nint GetISteamMusic;
    public nint GetISteamMusicRemote;
    public nint GetISteamHTMLSurface;
    public nint DEPRECATED_Set_SteamAPI_CPostAPIResultInProcess;
    public nint DEPRECATED_Remove_SteamAPI_CPostAPIResultInProcess;
    public nint Set_SteamAPI_CCheckCallbackRegisteredInProcess;
    public nint GetISteamInventory;
    public nint GetISteamVideo;
    public nint GetISteamParentalSettings;
    public nint GetISteamInput;
    public nint GetISteamParties;
}