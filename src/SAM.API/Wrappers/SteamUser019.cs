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
    private delegate void NativeGetSteamId(nint self, out ulong steamId);

    public ulong GetSteamId()
    {
        var call = GetFunction<NativeGetSteamId>(Functions.GetSteamID);
        call(ObjectAddress, out var steamId);
        return steamId;
    }

    public ulong GetSteamId3()
    {
        var call = GetFunction<NativeGetSteamId>(Functions.GetSteamID);
        call(ObjectAddress, out var steamId);
        steamId = (steamId >> (ushort)0) & 0xFFFFFFFF;
        return steamId;
    }
    #endregion

    #region GetPlayerSteamLevel
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate nint NativeGetPlayerSteamLevel(nint self);

    public nint GetPlayerSteamLevel()
    {
        var result = Call<nint, NativeGetPlayerSteamLevel>(Functions.GetPlayerSteamLevel, ObjectAddress);
        return result;
    }
    #endregion
}