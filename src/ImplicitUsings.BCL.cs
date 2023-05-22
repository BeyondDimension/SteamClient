// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Microsoft.Win32;
global using System.Collections.Concurrent;
global using System.Collections.ObjectModel;
#if !NETFRAMEWORK
global using System.Collections.Immutable;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
#endif
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.IO.Pipes;
global using System.IO.Compression;
global using System.IO.FileFormats;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Web;
global using System.Net;
global using System.Net.Security;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Net.Sockets;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.Serialization;
global using System.Security;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Security.Principal;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Text.Unicode;
global using System.Text.RegularExpressions;
global using System.Runtime;
global using DeploymentMode = System.Runtime.DeploymentMode;
global using System.Runtime.Devices;
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;
global using System.Runtime.Serialization.Formatters;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

#if WINDOWS7_0_OR_GREATER
global using System.Management;
global using WPFMessageBox = MS.Win32.MessageBox;
global using WPFMessageBoxButton = MS.Win32.MessageBoxButton;
global using WPFMessageBoxImage = MS.Win32.MessageBoxImage;
global using WPFMessageBoxResult = MS.Win32.MessageBoxResult;
#endif

global using System.Xml;
global using System.Xml.Serialization;
global using System.Security.Cryptography.X509Certificates;
global using IPAddress = System.Net.IPAddress;
global using Ioc = System.Ioc;
global using DateTimeFormat = System.DateTimeFormat;
global using SerializationDateTimeFormat = System.Runtime.Serialization.DateTimeFormat;

global using HttpHandlerCategory = System.Net.Http.Client.HttpHandlerCategory;
global using IHttpClientFactory = System.Net.Http.Client.IHttpClientFactory;
#if ANDROID
global using HttpHandlerType = Xamarin.Android.Net.AndroidMessageHandler;
#elif IOS || MACCATALYST
global using HttpHandlerType = System.Net.Http.NSUrlSessionHandler;
#elif NETFRAMEWORK
global using HttpHandlerType = System.Net.Http.HttpClientHandler;
#else
global using HttpHandlerType = System.Net.Http.SocketsHttpHandler;
#endif