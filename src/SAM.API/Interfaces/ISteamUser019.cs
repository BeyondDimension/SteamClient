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
public struct ISteamUser019
{
    public nint GetHSteamUser;
    public nint LoggedOn;
    public nint GetSteamID;
    public nint InitiateGameConnection;
    public nint TerminateGameConnection;
    public nint TrackAppUsageEvent;
    public nint GetUserDataFolder;
    public nint StartVoiceRecording;
    public nint StopVoiceRecording;
    public nint GetCompressedVoice;
    public nint DecompressVoice;
    public nint GetAuthSessionTicket;
    public nint BeginAuthSession;
    public nint EndAuthSession;
    public nint CancelAuthTicket;
    public nint UserHasLicenseForApp;
    public nint GetPlayerSteamLevel;
}