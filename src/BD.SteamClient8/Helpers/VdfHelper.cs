#if !(IOS || ANDROID)
using BD.SteamClient8.Enums.WebApi;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.Extensions;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using System.Buffers;
using System.Extensions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ValveKeyValue;

namespace BD.SteamClient8.Helpers;

/// <summary>
/// Valve Data File 格式助手类
/// </summary>
public static partial class VdfHelper
{
    const string TAG = nameof(VdfHelper);

    static readonly KVSerializerOptions options = new()
    {
        HasEscapeSequences = true,
    };

    /// <summary>
    /// 当前项目使用的可回收内存流管理器
    /// </summary>
    internal static readonly global::Microsoft.IO.RecyclableMemoryStreamManager M = new();

    /// <summary>
    /// 根据路径读取 Valve Data File 内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="isBinary"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KVObject? Read(string filePath, bool isBinary = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var kv = KVSerializer.Create(isBinary ? KVSerializationFormat.KeyValues1Binary : KVSerializationFormat.KeyValues1Text);
            var data = kv.Deserialize(fileStream, options);
            return data;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "Read vdf file error, filePath: {filePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 根据路径写入 Valve Data File 内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(string filePath, KVObject content)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            kv.Serialize(stream, content, options);
            stream.Flush();
            stream.SetLength(stream.Position);
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "Write vdf file error, filePath: {filePath}", filePath);
        }
    }

    public static bool UpdateAuthorizedDeviceList(IEnumerable<AuthorizedDevice> items)
    {
        string? configVdfPath = null;
        try
        {
            configVdfPath = SteamPathHelper.GetConfigVdfPath();
            if (!string.IsNullOrWhiteSpace(configVdfPath) && File.Exists(configVdfPath))
            {
                var v = Read(configVdfPath) ??
                    // C:\Program Files (x86)\Steam\config\config.vdf
                    new KVObject("InstallConfigStore", (KVValue)new KVCollectionValue());
                var lists = new KVCollectionValue();
                foreach (var item in items.OrderBy(static x => x.Index))
                {
                    var itemTemp = new KVObject(item.SteamId3_Int.ToString(),
                    [
                        new("timeused", (KVValue)item.Timeused),
                        new("description", (KVValue)(item.Description ?? "")),
                        new("tokenid", (KVValue)(item.Tokenid ?? "")),
                    ]);
                    lists.Add(itemTemp);
                }
                v.Set("AuthorizedDevice", lists);
                Write(configVdfPath, v);
                return true;
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "UpdateAuthorizedDeviceList fail, configVdfPath: {configVdfPath}",
                configVdfPath);
        }
        return false;
    }

    public static bool RemoveAuthorizedDeviceList(AuthorizedDevice item)
    {
        string? configVdfPath = null;
        try
        {
            configVdfPath = SteamPathHelper.GetConfigVdfPath();
            if (!string.IsNullOrWhiteSpace(configVdfPath) && File.Exists(configVdfPath))
            {
                var v = Read(configVdfPath);
                if (v != null)
                {
                    if (v["AuthorizedDevice"] is KVCollectionValue authorizedDevices)
                    {
                        authorizedDevices.Remove(item.SteamId3_Int.ToString());
                        //v["AuthorizedDevice"] = authorizedDevices;
                        Write(configVdfPath, v);
                        return true;
                    }
                }
            }
            // 删除操作在文件不存在或读取失败时也视作成功
            return true;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "RemoveAuthorizedDeviceList fail, configVdfPath: {configVdfPath}",
                configVdfPath);
        }
        return false;
    }

    public static void SetPersonaState(string steamId32, PersonaState ePersonaState)
    {
        if (ePersonaState == PersonaState.Default)
        {
            return;
        }

        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (!string.IsNullOrWhiteSpace(steamDirPath) && Directory.Exists(steamDirPath))
        {
            var localConfigFilePath = Path.Combine(steamDirPath, "userdata", steamId32, "config", "localconfig.vdf");

            // Values:
            // 0: Offline, 1: Online, 2: Busy, 3: Away, 4: Snooze, 5: Looking to Trade, 6: Looking to Play, 7: Invisible
            string? localConfigText;
            try
            {
                localConfigText = File.ReadAllText(localConfigFilePath); // Read relevant localconfig.vdf
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }

            // Find index of range needing to be changed.
            var positionOfVar = localConfigText.IndexOf("ePersonaState", StringComparison.Ordinal); // Find where the variable is being set
            if (positionOfVar == -1)
            {
                return;
            }

            var indexOfBefore = localConfigText.IndexOf(':', positionOfVar) + 1; // Find where the start of the variable's value is
            var indexOfAfter = localConfigText.IndexOf(',', positionOfVar); // Find where the end of the variable's value is

            // The variable is now in-between the above numbers. Remove it and insert something different here.
            var sb = new StringBuilder(localConfigText);
            sb.Remove(indexOfBefore, indexOfAfter - indexOfBefore);
            sb.Insert(indexOfBefore, (int)ePersonaState);
            var localConfigText2 = sb.ToString();

            // Output
            File.WriteAllText(localConfigFilePath, localConfigText2);
        }
    }

    public static bool DeleteLocalUserData(SteamUser user, bool isDeleteUserData = false)
    {
        string? userVdfPath = null;
        try
        {
            userVdfPath = SteamPathHelper.GetUserVdfPath();
            if (!string.IsNullOrWhiteSpace(userVdfPath) && File.Exists(userVdfPath))
            {
                var v = Read(userVdfPath);
                if (v != null)
                {
                    var item = v.Children?.FirstOrDefault(s => s.Name == user.SteamId64.ToString());
                    if (item != null)
                    {
                        v.Remove(user.SteamId64.ToString());
                        Write(userVdfPath, v);

                        if (isDeleteUserData)
                        {
                            var steamDirPath = SteamPathHelper.GetSteamDirPath();
                            if (!string.IsNullOrWhiteSpace(steamDirPath))
                            {
                                var userdataDirPath = Path.Combine(steamDirPath, "userdata", user.SteamId32.ToString());
                                if (Directory.Exists(userdataDirPath))
                                {
                                    Directory.Delete(userdataDirPath, true);
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            // 删除操作在文件不存在或读取失败时也视作成功
            return true;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "DeleteLocalUserData fail, userVdfPath: {userVdfPath}",
                userVdfPath);
        }
        return false;
    }

    public static bool UpdateLocalUserData(IEnumerable<SteamUser> users)
    {
        string? userVdfPath = null;
        try
        {
            userVdfPath = SteamPathHelper.GetUserVdfPath();
            if (!string.IsNullOrWhiteSpace(userVdfPath) && File.Exists(userVdfPath))
            {
                var v = Read(userVdfPath);
                if (v != null)
                {
                    var children = v.Children;
                    if (children.Any_Nullable())
                    {
                        foreach (var it in children)
                        {
                            try
                            {
                                var itemUser = users.FirstOrDefault(x => x.SteamId64.ToString() == it.Name);
                                if (itemUser == null)
                                {
                                    it["MostRecent"] = 0;
                                    continue;
                                }
                                it["AllowAutoLogin"] = 1;
                                it["MostRecent"] = Convert.ToInt16(itemUser.MostRecent);
                                it["WantsOfflineMode"] = Convert.ToInt16(itemUser.WantsOfflineMode);
                                it["SkipOfflineModeWarning"] = Convert.ToInt16(itemUser.SkipOfflineModeWarning);
                            }
                            catch (Exception e)
                            {
                                Log.Error(TAG, e,
                                    "UpdateLocalUserData item fail, userVdfPath: {userVdfPath}, name: {name}",
                                    userVdfPath,
                                    it.Name);
                            }
                        }
                    }
                }

                // 关闭 Steam 询问
                UpdateAlwaysShowUserChooser();
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "UpdateLocalUserData fail, userVdfPath: {userVdfPath}",
                userVdfPath);
        }
        return false;
    }

    /// <summary>
    /// 关闭 Steam 每次启动 Steam 时询问使用哪个账户
    /// </summary>
    public static bool UpdateAlwaysShowUserChooser()
    {
        string? configVdfPath = null;
        try
        {
            configVdfPath = SteamPathHelper.GetConfigVdfPath();
            if (!string.IsNullOrWhiteSpace(configVdfPath) && File.Exists(configVdfPath))
            {
                var v = Read(configVdfPath);
                if (v != null)
                {
                    var webStorage = v.Children.FirstOrDefault(x => x.Name == "WebStorage");
                    if (webStorage != null)
                    {
                        var auth = webStorage.Children.FirstOrDefault(x => x.Name == "Auth");
                        if (auth != null)
                        {
                            auth.Set("AlwaysShowUserChooser", 0);
                            Write(configVdfPath, v);
                            return true;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "UpdateAlwaysShowUserChooser fail, configVdfPath: {configVdfPath}",
                configVdfPath);
        }
        return false;
    }

    public static IDisposable? GetLocalUserDataWatcher(Action<object?, FileSystemEventArgs> onChanged)
    {
        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (steamDirPath != null)
        {
            return new LocalUserDataWatcher(steamDirPath, onChanged);
        }
        return null;
    }

    static SteamApp ConvertSteamApp(KVCollectionValue c, uint appid, string dirPath, string installdir, int state)
    {
        SteamApp m = new()
        {
            AppId = appid,
            Name = c.GetString("name"),
            InstalledDir = Path.Combine(dirPath, "common", installdir),
            State = state,
            SizeOnDisk = c.TryParse<long>("SizeOnDisk", out var s) ? s : default,
            LastOwner = c.TryParse<long>("LastOwner", out var l) ? l : default,
            BytesToDownload = c.TryParse<long>("BytesToDownload", out var bt) ? bt : default,
            BytesDownloaded = c.TryParse<long>("BytesDownloaded", out var bd) ? bd : default,
            BytesToStage = c.TryParse<long>("BytesToStage", out var bts) ? bts : default,
            BytesStaged = c.TryParse<long>("BytesStaged", out var bs) ? bs : default,
            LastUpdated = c.TryParseDateTimeS("LastUpdated"),
        };
        if (string.IsNullOrWhiteSpace(m.Name))
        {
            m.Name = installdir;
        }
        return m;
    }

    public static SteamApp? FileToAppInfo(string acfFilePath, Func<int, bool>? filter = null)
    {
        var result = FileToAppInfo(acfFilePath, ConvertSteamApp, filter);
        return result;
    }

    /// <summary>
    /// acf 文件转 <see cref="SteamApp"/>
    /// </summary>
    public static T? FileToAppInfo<T>(string acfFilePath, Func<KVCollectionValue, uint, string, string, int, T?> convert, Func<int, bool>? filter = null) where T : notnull
    {
        try
        {
            //string[] content = File.ReadAllLines(filename);
            // Skip if file contains only NULL bytes (this can happen sometimes, example: download crashes, resulting in a corrupted file)
            //if (content.Length == 1 && string.IsNullOrWhiteSpace(content[0].TrimStart('\0'))) return null;

            var fistLine = File.ReadLines(acfFilePath).FirstOrDefault();
            if (fistLine == null || fistLine.AsSpan().TrimStart('\0').IsWhiteSpace())
            {
                return default;
            }

            var v = Read(acfFilePath);
            if (v?.Value is KVCollectionValue c)
            {
                var installdir = c.GetString("installdir");
                if (!string.IsNullOrWhiteSpace(installdir))
                {
                    var dirPath = Path.GetDirectoryName(acfFilePath);
                    if (!string.IsNullOrEmpty(dirPath))
                    {
                        if (c.TryParse<uint>("appid", out var appid, StringComparison.OrdinalIgnoreCase))
                        {
                            var state = c.TryParseInt32("StateFlags");
                            if (filter?.Invoke(state) ?? true)
                            {
                                var m = convert(c, appid, dirPath, installdir, state);
                                return m;
                            }
                        }
                    }
                }
            }
        }
        catch (FileNotFoundException)
        {
            Log.Error(TAG, "FileToAppInfo failed, file not found: {acfFilePath}", acfFilePath);
            return default;
        }
        catch (DirectoryNotFoundException)
        {
            Log.Error(TAG, "FileToAppInfo failed, directory not found: {acfFilePath}", acfFilePath);
            return default;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "FileToAppInfo failed for {acfFilePath}", acfFilePath);
        }
        return default;
    }

    public static IReadOnlyList<string>? GetLibraryPaths()
    {
        const string dirname_steamapps = "steamapps"; // 文件夹名，linux上区分大小写

        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (string.IsNullOrWhiteSpace(steamDirPath) || !Directory.Exists(steamDirPath))
        {
            return null;
        }

        var defPath = Path.Combine(steamDirPath, dirname_steamapps);
        var defPathExists = Directory.Exists(defPath);
        List<string>? paths;
        if (defPathExists)
        {
            paths = [Path.Combine(steamDirPath, dirname_steamapps),];
        }
        else
        {
            return null;
        }

        try
        {
            string libraryFoldersPath = Path.Combine(defPath, "libraryfolders.vdf");
            if (File.Exists(libraryFoldersPath))
            {
                var v = Read(libraryFoldersPath);
                if (v != null)
                {
                    for (int i = 1; ; i++)
                    {
                        try
                        {
                            var pathNode = v[i.ToString()];

                            if (pathNode == null)
                            {
                                break;
                            }

                            var path = pathNode["path"].ToString();

                            if (!string.IsNullOrEmpty(path))
                            {
                                // New format
                                // Valve introduced a new format for the "libraryfolders.vdf" file
                                // In the new format, the node "1" not only contains a single value (the path),
                                // but multiple values: path, label, mounted, contentid

                                // If a library folder is removed in the Steam settings, the path persists, but its 'mounted' value is set to 0 (disabled)
                                // We consider only the value '1' as that the path is actually enabled.
                                if (pathNode["mounted"] != null && pathNode["mounted"].ToString() != "1")
                                {
                                    continue;
                                }

                                path = Path.Combine(path, dirname_steamapps);

                                if (Directory.Exists(path))
                                {
                                    paths.Add(path);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(TAG, e, "GetLibraryPaths for library folder {i} failed", i);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "GetLibraryPaths failed while reading libraryfolders.vdf");
        }
        return paths;
    }

    public static IDisposable? GetSteamDownloadingWatcher(Action<object?, FileSystemEventArgs, SteamApp> onChanged, Action<object?, FileSystemEventArgs, uint> onDeleted)
    {
        var libraryPaths = GetLibraryPaths();
        if (libraryPaths != null && libraryPaths.Count != 0)
        {
            var ws = libraryPaths.Select(
                libraryFolder => new SteamDownloadingWatcher(libraryFolder, onChanged, onDeleted));
            return new CompositeDisposable(ws);
        }
        return null;
    }

    const uint MagicNumber = 123094055U;
    const uint MagicNumberV2 = 123094056U;
    const uint MagicNumberV3 = 123094057U;

    /// <summary>
    /// 调用 <see cref="GetAppInfos"/> 时读取的值，用于保存时写入，保存函数调用之前必须先读取
    /// </summary>
    static uint? univeseNumber;

    static bool ContainsMagicNumbers(uint v) => v switch
    {
        MagicNumber or MagicNumberV2 or MagicNumberV3 => true,
        _ => false,
    };

    static string ReadNullTermUtf8String(Stream stream)
    {
        // https://github.com/SteamDatabase/SteamAppInfo/blob/d0ed9c5542f1ccdad8efcf456dd7f6665dbaca40/SteamAppInfoParser/AppInfo.cs#L113-L148
        var buffer = ArrayPool<byte>.Shared.Rent(32);
        try
        {
            var position = 0;

            do
            {
                var b = stream.ReadByte();

                if (b <= 0) // null byte or stream ended
                {
                    break;
                }

                if (position >= buffer.Length)
                {
                    var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = newBuffer;
                }

                buffer[position++] = (byte)b;
            }
            while (true);

            return Encoding.UTF8.GetString(buffer[..position]);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// 最大读取 SteamApp 数量
    /// </summary>
    const uint MAX_STEAMAPP_LENGTH = 10_0000;

    static bool ReadUniveseNumber(out uint value)
    {
        if (univeseNumber.HasValue)
        {
            value = univeseNumber.Value;
            return true;
        }
        else
        {
            GetAppInfos(null, default, default, true);
            if (univeseNumber.HasValue)
            {
                value = univeseNumber.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    public static List<SteamApp>? GetAppInfos(
        ICollection<uint>? hideGameKeys,
        bool isSaveProperties = false,
        uint maxAppLength = MAX_STEAMAPP_LENGTH,
        bool onlyReadUniveseNumber = false,
        string? appInfoPath = null)
    {
        appInfoPath ??= SteamPathHelper.GetAppInfoPath();
        if (string.IsNullOrWhiteSpace(appInfoPath))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(appInfoPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using BinaryReader binaryReader = new(stream, Encoding.UTF8, true);
            uint num = binaryReader.ReadUInt32();
            if (!ContainsMagicNumbers(num))
            {
                Log.Error(TAG, "GetAppInfos failed, magic number not found: {num}", num);
                return null;
            }

            univeseNumber = binaryReader.ReadUInt32();
            if (onlyReadUniveseNumber)
            {
                // 仅执行到读取 univeseNumber 的位置跳出
                return null;
            }

            string[]? stringPoolA = null;
            Span<string> stringPool = default;
            try
            {
                if (num == MagicNumberV3)
                {
                    var stringTableOffset = binaryReader.ReadInt64();
                    var offset = binaryReader.BaseStream.Position;
                    binaryReader.BaseStream.Position = stringTableOffset;
                    var stringCount = binaryReader.ReadUInt32();
                    if (stringCount <= int.MaxValue)
                    {
                        var len = unchecked((int)stringCount);
                        stringPoolA = ArrayPool<string>.Shared.Rent(len);
                        stringPool = stringPoolA.AsSpan(0, len);
                    }
                    else
                    {
                        stringPool = new string[stringCount];
                    }
                    for (var i = 0; i < stringCount; i++)
                    {
                        stringPool[i] = ReadNullTermUtf8String(binaryReader.BaseStream);
                    }
                    binaryReader.BaseStream.Position = offset;
                }
            }
            finally
            {
                if (stringPoolA != null)
                {
                    ArrayPool<string>.Shared.Return(stringPoolA);
                }
            }

            var installAppIds = GetInstalledAppIds();
            var apps = new List<SteamApp>();
            for (uint i = 0; i < maxAppLength; i++) // 最多读取 10W 个
            {
                var app = SteamApp.FromReader(binaryReader, stringPool, installAppIds, isSaveProperties, num == MagicNumberV2, num == MagicNumberV3);
                if (app == null)
                {
                    break;
                }
                else if (app.AppId > 0)
                {
                    if (!isSaveProperties)
                    {
                        //if (GameLibrarySettings.DefaultIgnoreList.Value.Contains(app.AppId))
                        //    continue;
                        if (hideGameKeys != null && hideGameKeys.Contains(app.AppId))
                        {
                            continue;
                        }
                        //if (app.ParentId > 0)
                        //{
                        //    var parentApp = apps.FirstOrDefault(f => f.AppId == app.ParentId);
                        //    if (parentApp != null)
                        //        parentApp.ChildApp.Add(app.AppId);
                        //    //continue;
                        //}
                    }
                    apps.Add(app);
                    //app.Modified += (s, e) =>
                    //{
                    //};
                }
            }
            return apps;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "GetAppInfos failed while opening appinfo.vdf");
            return null;
        }
    }

    /// <summary>
    /// 获取已安装程序的 AppId
    /// </summary>
    /// <returns></returns>
    public static uint[] GetInstalledAppIds()
    {
        var result = GetDownloadingAppListCore(static (_, appid, _, _, _) => appid, static state =>
        {
            var isInstalled = SteamApp.IsInstalledByState(state);
            return isInstalled;
        });
        return [.. result];
    }

    public static IEnumerable<T> GetDownloadingAppListCore<T>(Func<KVCollectionValue, uint, string, string, int, T?> convert, Func<int, bool>? filter = null) where T : notnull
    {
        var libraryPaths = GetLibraryPaths();
        if (libraryPaths != null && libraryPaths.Count != 0)
        {
            foreach (string libraryPath in libraryPaths)
            {
                IEnumerable<string>? files;
                try
                {
                    files = Directory.EnumerateFiles(libraryPath, "*.acf");
                }
                catch (DirectoryNotFoundException)
                {
                    continue;
                }
                foreach (var it in files)
                {
                    var fileInfo = new FileInfo(it);
                    // Skip if file is empty
                    if (fileInfo.Length == 0)
                    {
                        continue;
                    }
                    var ai = FileToAppInfo(fileInfo.FullName, convert, filter);
                    if (ai is not null)
                    {
                        continue;
                    }
                    yield return ai!;
                }
            }
        }
    }

    public static IReadOnlyList<SteamApp>? GetDownloadingAppList(Func<int, bool>? filter = null)
    {
        try
        {
            var list = GetDownloadingAppListCore(ConvertSteamApp, filter).ToList();
            return list;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "GetDownloadingAppList failed");
        }
        return default;
    }

    /// <summary>
    /// 修改的文件名
    /// </summary>
    internal const string ModifiedFileName = "modifications.vdf";

    internal const string ModifiedFileNameV2 = "modifications.json";

    public static async Task<bool> SaveAppInfosAsync(ICollection<uint>? hideGameKeys, IEnumerable<SteamApp> editApps)
    {
        try
        {
            var appInfoPath = SteamPathHelper.GetAppInfoPath();
            if (!string.IsNullOrWhiteSpace(appInfoPath))
            {
                var applist = GetAppInfos(hideGameKeys);
                if (applist != null && applist.Count != 0)
                {
                    if (ReadUniveseNumber(out var univeseNumber))
                    {
                        using var m = M.GetStream();
                        using BinaryWriter binaryWriter = new(m, Encoding.UTF8, true);
                        binaryWriter.Write(MagicNumberV3);
                        binaryWriter.Write(univeseNumber);

                        var modifiedApps = new List<ModifiedApp>();
                        foreach (var app in applist)
                        {
                            var editApp = editApps.FirstOrDefault(s => s.AppId == app.AppId);
                            if (editApp != null)
                            {
                                app.SetEditProperty(editApp);
                                modifiedApps.Add(new ModifiedApp(app));
                            }
                            app.Write(binaryWriter);
                        }

                        binaryWriter.Write(0);

                        // 修改的内容写入 appinfo.vdf
                        using FileStream fileStream = new FileStream(appInfoPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                        binaryWriter.BaseStream.Position = 0L;
                        await binaryWriter.BaseStream.CopyToAsync(fileStream);

                        // 修改的配置写入 modifications.json
                        var configFilePathV2 = Path.Combine(IOPath.AppDataDirectory, ModifiedFileNameV2);
                        using var configFileStreamV2 = new FileStream(configFilePathV2, FileMode.Open, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                        await JsonSerializer.SerializeAsync(configFileStreamV2, modifiedApps, DefaultJsonSerializerContext_.Default.ModifiedApp);
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "SaveAppInfosAsync failed");
        }
        return false;
    }
}

file sealed class CompositeDisposable(IDisposable[] disposables) : IDisposable
{
    IDisposable[]? disposables = disposables;

    public CompositeDisposable(IEnumerable<IDisposable> disposables) : this([.. disposables])
    {

    }

    bool disposedValue;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (disposables != null)
                {
                    foreach (var it in disposables)
                    {
                        it?.Dispose();
                    }
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposables = null;
            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

file class FileSystemWatcherWrapper : IDisposable
{
    const NotifyFilters DefaultNotifyFilters = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;

    protected bool DisposedValue { get; private set; }

    FileSystemWatcher? w;

    public FileSystemWatcherWrapper(FileSystemWatcher w, NotifyFilters notifyFilters = DefaultNotifyFilters)
    {
        this.w = w;
        w.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
        RegisterEvent(w);
        w.EnableRaisingEvents = true;
    }

    protected virtual void RegisterEvent(FileSystemWatcher w)
    {
    }

    protected virtual void UnregisterEvent(FileSystemWatcher w)
    {
    }

    public FileSystemWatcherWrapper(string path, NotifyFilters notifyFilters = DefaultNotifyFilters) : this(new FileSystemWatcher(path), notifyFilters)
    {

    }

    public FileSystemWatcherWrapper(string path, string filter, NotifyFilters notifyFilters = DefaultNotifyFilters) : this(new FileSystemWatcher(path, filter), notifyFilters)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                //onChanged = null!;
                // 释放托管状态(托管对象)
                if (w != null)
                {
                    UnregisterEvent(w);
                    w.Dispose();
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            w = null;
            DisposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

file sealed class LocalUserDataWatcher(string steamDirPath, Action<object?, FileSystemEventArgs> onChanged) : FileSystemWatcherWrapper(Path.Combine(steamDirPath, "config", "loginusers.vdf"))
{
    Action<object?, FileSystemEventArgs>? onChanged = onChanged;

    void OnChanged(object? sender, FileSystemEventArgs e)
    {
        onChanged?.Invoke(sender, e);
    }

    protected override void RegisterEvent(FileSystemWatcher w)
    {
        w.Created += OnChanged;
        w.Renamed += OnChanged;
        w.Changed += OnChanged;
    }

    protected override void UnregisterEvent(FileSystemWatcher w)
    {
        w.Created -= OnChanged;
        w.Renamed -= OnChanged;
        w.Changed -= OnChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                onChanged = null;
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
        }

        base.Dispose(disposing);
    }
}

file sealed class SteamDownloadingWatcher(string libraryFolder, Action<object?, FileSystemEventArgs, SteamApp> onChanged, Action<object?, FileSystemEventArgs, uint> onDeleted) : FileSystemWatcherWrapper(libraryFolder, "*.acf")
{
    Action<object?, FileSystemEventArgs, SteamApp>? onChanged = onChanged;
    Action<object?, FileSystemEventArgs, uint>? onDeleted = onDeleted;

    void OnChanged(object? sender, FileSystemEventArgs e)
    {
        SteamApp? app = null;
        try
        {
            // This is necessary because sometimes the file is still accessed by steam, so let's wait for 10 ms and try again.
            // Maximum 5 times
            int counter = 1;
            do
            {
                try
                {
                    app = VdfHelper.FileToAppInfo(e.FullPath);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(99);
                }
            }
            while (counter++ <= 5);
        }
        catch
        {
            return;
        }

        // Shouldn't happen, but might occur if Steam holds the acf file too long
        if (app == null)
        {
            return;
        }

        onChanged?.Invoke(sender, e, app);
    }

    /// <summary>
    /// acf 文件名格式中提取 appid
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="appid"></param>
    /// <returns></returns>
    static bool IdFromAcfFilename(ReadOnlySpan<char> filename, out uint appid)
    {
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        int loc = filenameWithoutExtension.IndexOf('_');
        return uint.TryParse(filenameWithoutExtension[(loc + 1)..], out appid);
    }

    void OnDeleted(object? sender, FileSystemEventArgs e)
    {
        if (IdFromAcfFilename(e.FullPath, out var appid))
        {
            onDeleted?.Invoke(sender, e, appid);
        }
    }

    protected override void RegisterEvent(FileSystemWatcher w)
    {
        w.Renamed += OnChanged;
        w.Changed += OnChanged;
        w.Deleted += OnDeleted;
    }

    protected override void UnregisterEvent(FileSystemWatcher w)
    {
        w.Renamed -= OnChanged;
        w.Changed -= OnChanged;
        w.Deleted -= OnDeleted;
    }

    protected override void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                onDeleted = null;
                onChanged = null;
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
        }

        base.Dispose(disposing);
    }
}
#endif