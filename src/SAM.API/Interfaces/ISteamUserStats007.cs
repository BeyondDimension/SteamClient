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
public class ISteamUserStats007
{
    public nint RequestCurrentStats;
    public nint GetStatFloat;
    public nint GetStatInteger;
    public nint SetStatFloat;
    public nint SetStatInteger;
    public nint UpdateAvgRateStat;
    public nint GetAchievement;
    public nint SetAchievement;
    public nint ClearAchievement;
    public nint GetAchievementAndUnlockTime;
    public nint StoreStats;
    public nint GetAchievementIcon;
    public nint GetAchievementDisplayAttribute;
    public nint IndicateAchievementProgress;
    public nint RequestUserStats;
    public nint GetUserStatFloat;
    public nint GetUserStatInt;
    public nint GetUserAchievement;
    public nint GetUserAchievementAndUnlockTime;
    public nint ResetAllStats;
    public nint FindOrCreateLeaderboard;
    public nint FindLeaderboard;
    public nint GetLeaderboardName;
    public nint GetLeaderboardEntryCount;
    public nint GetLeaderboardSortMethod;
    public nint GetLeaderboardDisplayType;
    public nint DownloadLeaderboardEntries;
    public nint GetDownloadedLeaderboardEntry;
    public nint UploadLeaderboardScore;
    public nint GetNumberOfCurrentPlayers;
    public nint GetAchievementAchievedPercent;
    public nint RequestGlobalAchievementPercentages;
}