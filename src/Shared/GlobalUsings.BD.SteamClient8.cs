// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005 // 删除不必要的 using 指令
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using BD.SteamClient8;
global using BD.SteamClient8.Helpers;
global using BD.SteamClient8.Services;

global using BD.SteamClient8.Models.Abstractions;

global using Org.BouncyCastle.Crypto;
global using Org.BouncyCastle.Crypto.Digests;
global using Org.BouncyCastle.Crypto.Engines;
global using Org.BouncyCastle.Crypto.Macs;
global using Org.BouncyCastle.Crypto.Paddings;
global using Org.BouncyCastle.Crypto.Parameters;