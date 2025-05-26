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
public struct ISteamUtils005
{
    public nint GetSecondsSinceAppActive;
    public nint GetSecondsSinceComputerActive;
    public nint GetConnectedUniverse;
    public nint GetServerRealTime;
    public nint GetIPCountry;
    public nint GetImageSize;
    public nint GetImageRGBA;
    public nint GetCSERIPPort;
    public nint GetCurrentBatteryPower;
    public nint GetAppID;
    public nint SetOverlayNotificationPosition;
    public nint IsAPICallCompleted;
    public nint GetAPICallFailureReason;
    public nint GetAPICallResult;
    public nint RunFrame;
    public nint GetIPCCallCount;
    public nint SetWarningMessageHook;
    public nint IsOverlayEnabled;
    public nint OverlayNeedsPresent;
}