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

#pragma warning disable SA1600 // Elements should be documented

public class SteamUser017 : NativeWrapper<ISteamUser019>
{
    #region GetSteamID
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate ulong NativeGetSteamId(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeGetSteamId2(IntPtr self, out ulong steamId);

    public ulong GetSteamId()
    {
        if (OperatingSystem.IsWindows())
        {
            var call = GetFunction<NativeGetSteamId2>(Functions.GetSteamID);
            ulong steamId;
            call(ObjectAddress, out steamId);
            return steamId;
        }
        else
        {
            var steamId = Call<ulong, NativeGetSteamId>(Functions.GetSteamID, ObjectAddress);
            return steamId;
        }
    }

    public ulong GetSteamId3()
    {
        if (OperatingSystem.IsWindows())
        {
            var call = GetFunction<NativeGetSteamId2>(Functions.GetSteamID);
            ulong steamId;
            call(ObjectAddress, out steamId);
            steamId = (steamId >> (ushort)0) & 0xFFFFFFFF;
            return steamId;
        }
        else
        {
            var steamId = Call<ulong, NativeGetSteamId>(Functions.GetSteamID, ObjectAddress);
            steamId = (steamId >> (ushort)0) & 0xFFFFFFFF;
            return steamId;
        }
    }

    #endregion

    #region GetPlayerSteamLevel
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetPlayerSteamLevel(IntPtr self);

    public IntPtr GetPlayerSteamLevel()
    {
        var result = Call<IntPtr, NativeGetPlayerSteamLevel>(Functions.GetPlayerSteamLevel, ObjectAddress);
        return result;
    }
    #endregion
}