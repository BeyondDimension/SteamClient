#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
namespace BD.SteamClient.Services.Implementation;

partial class SteamServiceImpl
{
    const string AppInfoEditsFileName = "steam-app-edits.json";
    readonly object appInfoEditStore = new();

    public IReadOnlyDictionary<uint, SteamAppEdit> GetAppInfoEdits()
    {
        lock (appInfoEditStore)
        {
            try
            {
                return ReadAppInfoEdits(Path.Combine(IOPath.AppDataDirectory, AppInfoEditsFileName));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to read Steam app edits.");
                return ImmutableDictionary<uint, SteamAppEdit>.Empty;
            }
        }
    }

    public Task SaveLocalAppInfoEditsAsync(IReadOnlyDictionary<uint, SteamAppEdit> changes) => Task.Run(() =>
    {
        if (changes.Count == 0) return;
        lock (appInfoEditStore)
        {
            var path = Path.Combine(IOPath.AppDataDirectory, AppInfoEditsFileName);
            var edits = ReadAppInfoEdits(path);
            foreach (var (id, edit) in changes)
                edits[id] = edit;
            WriteAppInfoEdits(path, edits);
        }
    });

    Dictionary<uint, SteamAppEdit> ReadAppInfoEdits(string path)
    {
        if (File.Exists(path))
        {
            using var source = File.OpenRead(path);
            return SteamAppEditBackup.ReadEdits(source);
        }
        var legacy = Path.Combine(IOPath.AppDataDirectory, ModifiedFileName);
        if (!File.Exists(legacy))
            return [];

        Dictionary<uint, SteamAppEdit> migrated;
        using (var source = File.OpenRead(legacy))
            migrated = SteamAppEditBackup.ReadEdits(source);
        // Preserve the previous file for users downgrading to an older application build.
        WriteAppInfoEdits(path, migrated);
        return migrated;
    }

    static void WriteAppInfoEdits(string path, Dictionary<uint, SteamAppEdit> edits)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                SteamAppEditBackup.Write(destination, edits);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    public Task<bool> SaveAppInfosToSteam()
    {
        var steamDirectory = SteamDirPath;
        if (steamDirectory == null)
            return Task.FromResult(false);
        bool CanWrite() => !IsRunningSteamProcess && string.Equals(steamDirectory, SteamDirPath, StringComparison.Ordinal);
        return Task.Run(() =>
        {
            try
            {
                if (!CanWrite()) return false;
                return RewriteAppInfoCache(steamDirectory, GetAppInfoEdits(), CanWrite);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to update Steam appinfo cache.");
                return false;
            }
        });
    }
}
#endif
