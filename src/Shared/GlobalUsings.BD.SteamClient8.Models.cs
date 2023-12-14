// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005 // 删除不必要的 using 指令
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using BD.SteamClient8.Models;
global using BD.SteamClient8.Models.Helpers;
global using BD.SteamClient8.Models.Protobuf;
global using BD.SteamClient8.Models.Converter;
global using BD.SteamClient8.Models.WebApi;
global using BD.SteamClient8.Models.WebApi.Idle;
global using BD.SteamClient8.Models.WebApi.Market;
global using BD.SteamClient8.Models.WebApi.Profile;
global using BD.SteamClient8.Models.WebApi.SteamGridDB;
global using BD.SteamClient8.Models.WebApi.Trade;
global using BD.SteamClient8.Models.WebApi.SteamApp;
global using BD.SteamClient8.Models.WebApi.Login;
global using BD.SteamClient8.Models.WebApi.Authenticator;
global using BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

global using SteamUser = BD.SteamClient8.Models.WebApi.SteamUser;
global using SteamApps = BD.SteamClient8.Models.WebApi.SteamApp.SteamApps;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
global using SteamKit2SteamUser = SteamKit2.SteamUser;
global using SteamKit2SteamApps = SteamKit2.SteamApps;
global using SteamKit2;
global using SteamKit2ERemoteStoragePlatform = SteamKit2.ERemoteStoragePlatform;
#endif