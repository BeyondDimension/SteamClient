// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
global using SteamKit2;
#endif

#if !PROTOBUF_LIB
global using static BD.SteamClient.Constants.SteamApiUrls;
global using static BD.SteamClient.Constants.SteamGridDBApiUrls;
#endif

#if !PROTOBUF_LIB
global using SteamUser = BD.SteamClient.Models.SteamUser;
global using SteamApps = BD.SteamClient.Models.SteamApps;
#endif

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
global using SteamKit2SteamUser = SteamKit2.SteamUser;
global using SteamKit2SteamApps = SteamKit2.SteamApps;

global using SteamKit2ERemoteStoragePlatform = SteamKit2.ERemoteStoragePlatform;
#endif
#if USE_STMCLIENT_LIB
global using ERemoteStoragePlatform = SAM.API.Types.ERemoteStoragePlatform;
#endif