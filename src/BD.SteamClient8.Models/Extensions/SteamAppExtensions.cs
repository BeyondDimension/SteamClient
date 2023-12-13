#pragma warning disable IDE0130
namespace BD.SteamClient8.Models.WebApi.SteamApp;

#pragma warning disable SA1600 // Elements should be documented
public static class SteamAppExtensions
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public static bool SetEditProperty(this SteamApp steamApp, SteamApp appInfo)
    {
        if (steamApp._properties != null)
        {
            // steam_edit Data
            //SteamAppPropertyTable? propertyValue = _properties.GetPropertyValue<SteamAppPropertyTable>(null, NodeAppInfo, NodeCommon);
            //if (propertyValue != null)
            //{
            //    string? text = Name;
            //    if (text != null)
            //    {
            //        SteamAppPropertyTable? propertyValue2 = propertyValue.GetPropertyValue<SteamAppPropertyTable>(null, "name_localized");
            //        if (propertyValue2 != null)
            //        {
            //            _properties.SetPropertyValue(SteamAppPropertyType.Table, propertyValue2, NodeAppInfo, "steam_edit", "base_name_localized");
            //            propertyValue.RemoveProperty("name_localized");
            //        }
            //        _properties.SetPropertyValue(SteamAppPropertyType.String, text, NodeAppInfo, "steam_edit", "base_name");
            //        //if (SteamData.UseCompleted && this.IsCompleted)
            //        //{
            //        //    text += SteamData.CompletedSuffix;
            //        //}
            //        propertyValue.SetPropertyValue("name", SteamAppPropertyType.String, text);
            //    }
            //    else
            //    {
            //        Log.Info("SteamApp Write Error", $"AppInfo {AppId:X8} has null name!");
            //    }

            //    //string? text2 = propertyValue.GetPropertyValue<string>("type", "game");
            //    //if (text2 != null)
            //    //{
            //    //    _properties.SetPropertyValue(SteamAppPropertyType.String, text2, NodeAppInfo, "steam_edit", "base_type");
            //    //    //if (SteamData.UseHidden && this.IsHidden)
            //    //    //{
            //    //    //    text2 = "hidden_" + text2;
            //    //    //}
            //    //    propertyValue.SetPropertyValue("type", SteamAppPropertyType.String, text2);
            //    //}
            //    //else
            //    //{
            //    //    Log.Info("SteamApp Write Error", $"AppInfo {AppId:X8} has null type!");
            //    //}
            //}

            //SortAs = appInfo.SortAs;
            //Developer = appInfo.Developer;
            //Publisher = appInfo.Publisher;
            //LaunchItems = appInfo.LaunchItems;

            if (!string.IsNullOrEmpty(appInfo.Name) && appInfo.Name != appInfo.BaseName)
            {
                steamApp.Name = appInfo.Name;

                SteamAppPropertyTable? propertyValue2 = steamApp._properties.GetPropertyValue<SteamAppPropertyTable>(null, steamApp.NodeAppInfo, steamApp.NodeCommon, "name_localized");
                if (propertyValue2 != null)
                {
                    steamApp._properties.SetPropertyValue(SteamAppPropertyType.Table, propertyValue2, steamApp.NodeAppInfo, "steam_edit", "base_name_localized");

                    //_properties.RemoveProperty(NodeAppInfo, NodeCommon, "name_localized");
                    steamApp._properties.SetPropertyValue(SteamAppPropertyType.Table, new SteamAppPropertyTable(), steamApp.NodeAppInfo, steamApp.NodeCommon, "name_localized");
                }
                steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.BaseName, steamApp.NodeAppInfo, "steam_edit", "base_name");

                steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Name, steamApp.NodeAppInfo, steamApp.NodeCommon, steamApp.NodeName);
            }
            else
            {
                Log.Info("SteamApp Write Error", $"AppInfo {steamApp.AppId:X8} has null name!");
            }

            //_properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Name, NodeAppInfo,
            //    NodeCommon,
            //    NodeName);

            steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.SortAs, steamApp.NodeAppInfo,
                steamApp.NodeCommon,
                steamApp.NodeSortAs);

            steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Developer, steamApp.NodeAppInfo,
                steamApp.NodeExtended,
                steamApp.NodeDeveloper);

            steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Developer, steamApp.NodeAppInfo,
                steamApp.NodeCommon,
                "associations", "0", "name");

            steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Publisher, steamApp.NodeAppInfo,
                steamApp.NodeExtended,
                steamApp.NodePublisher);

            steamApp._properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Publisher, steamApp.NodeAppInfo,
                steamApp.NodeCommon,
                "associations", "1", "name");

            if (appInfo.LaunchItems.Any_Nullable())
            {
                var launchTable = new SteamAppPropertyTable();

                foreach (var item in appInfo.LaunchItems)
                {
                    var propertyTable = new SteamAppPropertyTable();

                    propertyTable.SetPropertyValue("executable", SteamAppPropertyType.String, item.Executable);

                    if (!string.IsNullOrEmpty(item.Label))
                    {
                        propertyTable.SetPropertyValue("description", SteamAppPropertyType.String, item.Label);
                    }
                    if (!string.IsNullOrEmpty(item.Arguments))
                    {
                        propertyTable.SetPropertyValue("arguments", SteamAppPropertyType.String, item.Arguments);
                    }
                    if (!string.IsNullOrEmpty(item.WorkingDir))
                    {
                        propertyTable.SetPropertyValue("workingdir", SteamAppPropertyType.String, item.WorkingDir);
                    }
                    if (!string.IsNullOrEmpty(item.Platform))
                    {
                        propertyTable.SetPropertyValue(SteamAppPropertyType.String, item.Platform, steamApp.NodeConfig, steamApp.NodePlatforms);
                    }
                    launchTable.SetPropertyValue(launchTable.Count.ToString(), SteamAppPropertyType.Table, propertyTable);
                }

                steamApp._properties.SetPropertyValue(SteamAppPropertyType.Table, launchTable, steamApp.NodeAppInfo, steamApp.NodeConfig, steamApp.NodeLaunch);
            }

            return true;
        }
        return false;
    }

#endif

    public static string GetIdAndName(this SteamApp steamApp)
    {
        return $"{steamApp.AppId} | {steamApp.DisplayName}";
    }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    public static Process? StartSteamAppProcess(this SteamApp steamApp, SteamAppRunType runType = SteamAppRunType.Idle)
    {
        var arg = runType switch
        {
            SteamAppRunType.UnlockAchievement => "-achievement",
            SteamAppRunType.CloudManager => "-cloudmanager",
            _ => "-silence",
        };
        string arguments = $"-clt app {arg} -id {steamApp.AppId}";
        var processPath = Environment.ProcessPath;
        processPath.ThrowIsNull();
        if (OperatingSystem.IsWindows())
        {
            return steamApp.Process = Process2.Start(processPath, arguments);
        }
        else
        {
            if (OperatingSystem.IsLinux())
            {
                var psi = new ProcessStartInfo
                {
                    Arguments = arguments,
                    FileName = Path.Combine(AppContext.BaseDirectory, "Steam++.sh"),
                    UseShellExecute = true,
                };
                Console.WriteLine(psi.FileName);
                psi.Environment.Add("SteamAppId", steamApp.AppId.ToString());
                return steamApp.Process = Process.Start(psi);
            }
            else
            {
                return steamApp.Process = Process2.Start(
                processPath,
                arguments,
                environment: new Dictionary<string, string>() {
                    {
                        "SteamAppId",
                        steamApp.AppId.ToString()
                    }
                });
            }
        }
    }

    public static void RunOrStopSteamAppProcess(this SteamApp steamApp)
    {
        if (steamApp.Process != null && !steamApp.Process.HasExited)
        {
            steamApp.Process.KillEntireProcessTree();
            steamApp.Process = null;
        }
        else
        {
            steamApp.StartSteamAppProcess();
        }
    }

    public static SteamApp ExtractReaderProperty(this SteamApp steamApp, SteamAppPropertyTable properties, uint[]? installedAppIds = null)
    {
        if (properties != null)
        {
            //var installpath = properties.GetPropertyValue<string>(null, NodeAppInfo, NodeConfig, "installdir");

            //if (!string.IsNullOrEmpty(installpath))
            //{
            //    app.InstalledDir = Path.Combine(ISteamService.Instance.SteamDirPath, ISteamService.dirname_steamapps, NodeCommon, installpath);
            //}

            steamApp.Name = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, steamApp.NodeCommon, steamApp.NodeName);
            steamApp.SortAs = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, steamApp.NodeCommon, steamApp.NodeSortAs);
            if (!steamApp.SortAs.Any_Nullable())
            {
                steamApp.SortAs = steamApp.Name;
            }
            steamApp.ParentId = properties.GetPropertyValue<uint>(0, steamApp.NodeAppInfo, steamApp.NodeCommon, steamApp.NodeParentId);
            steamApp.Developer = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, steamApp.NodeExtended, steamApp.NodeDeveloper);
            steamApp.Publisher = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, steamApp.NodeExtended, steamApp.NodePublisher);
            //SteamReleaseDate = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, "steam_release_date");
            //OriginReleaseDate = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, "original_release_date");

            var type = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, steamApp.NodeCommon, steamApp.NodeAppType);
            if (Enum.TryParse(type, true, out SteamAppType apptype))
            {
                steamApp.Type = apptype;
            }
            else
            {
                steamApp.Type = SteamAppType.Unknown;
                Debug.WriteLineIf(!string.IsNullOrEmpty(type), string.Format("AppInfo: New AppType '{0}'", type));
            }

            steamApp.OSList = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, steamApp.NodeCommon, steamApp.NodePlatforms);

            if (installedAppIds != null)
            {
                if (installedAppIds.Contains(steamApp.AppId) &&
                    (steamApp.Type == SteamAppType.Application ||
                    steamApp.Type == SteamAppType.Game ||
                    steamApp.Type == SteamAppType.Tool ||
                    steamApp.Type == SteamAppType.Demo))
                {
                    // This is an installed app.
                    steamApp.State = 4;
                }
            }

            if (steamApp.IsInstalled)
            {
                var launchTable = properties.GetPropertyValue<SteamAppPropertyTable?>(null, steamApp.NodeAppInfo, steamApp.NodeConfig, steamApp.NodeLaunch);

                if (launchTable != null)
                {
                    var launchItems = from table in from prop in (from prop in launchTable.Properties
                                                                  where prop.PropertyType == SteamAppPropertyType.Table
                                                                  select prop).OrderBy((SteamAppProperty prop) => prop.Name, StringComparer.OrdinalIgnoreCase)
                                                    select prop.GetValue<SteamAppPropertyTable>()
                                      select new SteamAppLaunchItem
                                      {
                                          Label = table.GetPropertyValue<string?>("description"),
                                          Executable = table.GetPropertyValue<string?>("executable"),
                                          Arguments = table.GetPropertyValue<string?>("arguments"),
                                          WorkingDir = table.GetPropertyValue<string?>("workingdir"),
                                          Platform = table.TryGetPropertyValue<SteamAppPropertyTable>(steamApp.NodeConfig, out var propertyTable) ?
                                          propertyTable!.TryGetPropertyValue<string>(steamApp.NodePlatforms, out var os) ? os : null : null,
                                      };

                    steamApp.LaunchItems = new ObservableCollection<SteamAppLaunchItem>(launchItems.ToList());
                }
            }

            steamApp.CloudQuota = properties.GetPropertyValue(0, steamApp.NodeAppInfo, "ufs", "quota");
            steamApp.CloudMaxnumFiles = properties.GetPropertyValue(0, steamApp.NodeAppInfo, "ufs", "maxnumfiles");

            var savefilesTable = properties.GetPropertyValue<SteamAppPropertyTable?>(null, steamApp.NodeAppInfo, "ufs", "savefiles");

            if (savefilesTable != null)
            {
                var savefiles = from table in from prop in (from prop in savefilesTable.Properties
                                                            where prop.PropertyType == SteamAppPropertyType.Table
                                                            select prop).OrderBy((SteamAppProperty prop) => prop.Name, StringComparer.OrdinalIgnoreCase)
                                              select prop.GetValue<SteamAppPropertyTable>()
                                select new SteamAppSaveFile(
                                    steamApp.AppId,
                                    table.GetPropertyValue<string?>("root"),
                                    table.GetPropertyValue<string?>("path"),
                                    table.GetPropertyValue<string?>("pattern")
                                )
                                {
                                    Recursive = table.GetPropertyValue(false, "recursive"),
                                };

                steamApp.SaveFiles = new ObservableCollection<SteamAppSaveFile>(savefiles.ToList());
            }

            steamApp.BaseName = properties.GetPropertyValue(string.Empty, steamApp.NodeAppInfo, "steam_edit", "base_name");

            if (string.IsNullOrEmpty(steamApp.BaseName))
            {
                steamApp.BaseName = steamApp.Name;
            }
        }
        return steamApp;
    }

    public static SteamApp? FromReader(BinaryReader reader, uint[]? installedAppIds = null, bool isSaveProperties = false, bool is0x07564428 = true)
    {
        uint id = reader.ReadUInt32();
        if (id == 0)
        {
            return null;
        }
        SteamApp app = new()
        {
            AppId = id,
        };
        try
        {
            int count = reader.ReadInt32();
            byte[] array = reader.ReadBytes(count);
            using BinaryReader binaryReader = new(new MemoryStream(array));
            app._stuffBeforeHash = binaryReader.ReadBytes(16);
            binaryReader.ReadBytes(20);
            app._changeNumber = binaryReader.ReadUInt32();

            if (is0x07564428)
                binaryReader.ReadBytes(20);

            var properties = binaryReader.ReadPropertyTable();

            if (properties == null)
                return app;

            if (isSaveProperties)
            {
                app._properties = properties;
                app._originalData = array;
            }
            app.ExtractReaderProperty(properties, installedAppIds);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SteamApp), ex, string.Format("Failed to load entry with appId {0}", app.AppId));
        }
        return app;
    }

    public static void Write(this SteamApp steamApp, BinaryWriter writer)
    {
        if (steamApp._properties == null)
            throw new ArgumentNullException($"SteamApp Write Failed. {nameof(steamApp._properties)} is null.");
        SteamAppPropertyTable propertyTable = new SteamAppPropertyTable(steamApp._properties);
        string s = propertyTable.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        byte[] buffer = SHA1.HashData(bytes);
        writer.Write((int)steamApp.AppId);
        using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
        binaryWriter.Write(steamApp._stuffBeforeHash.ThrowIsNull());
        binaryWriter.Write(buffer);
        binaryWriter.Write(steamApp._changeNumber);
        binaryWriter.Write(propertyTable);
        MemoryStream memoryStream = (MemoryStream)binaryWriter.BaseStream;
        writer.Write((int)memoryStream.Length);
        writer.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }
#endif
}
