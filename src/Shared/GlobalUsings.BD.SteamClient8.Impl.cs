// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005 // 删除不必要的 using 指令
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

#if NETFRAMEWORK || WINDOWS
global using System.Management;
#endif
global using System.Net.Http.Json;

global using BD.SteamClient8.Impl;
global using BD.SteamClient8.Impl.WebApi;
global using BD.SteamClient8.Impl.PInvoke;
global using BD.SteamClient8.Impl.Extensions;