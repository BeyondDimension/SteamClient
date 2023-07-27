// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Avalonia.Metadata;

[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Models")]
#if !PROTOBUF_LIB
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Constants")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Enums")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Enums.SteamGridDB")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Helpers")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Helpers")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.SteamClient.Models.SteamGridDB")]
[assembly: XmlnsDefinition("https://steampp.net/services", "BD.SteamClient.Services")]
[assembly: XmlnsDefinition("https://steampp.net/services", "BD.SteamClient.Services.Mvvm")]
#endif