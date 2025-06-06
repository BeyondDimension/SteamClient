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

using SAM.API.Types;
using SAM.API.Wrappers;
using System.Globalization;

namespace SAM.API;

public class Client : IDisposable
{
    public SteamClient018 SteamClient { get; set; } = null!;

    public SteamUser017 SteamUser { get; set; } = null!;

    public SteamUserStats011 SteamUserStats { get; set; } = null!;

    public SteamUtils007 SteamUtils { get; set; } = null!;

    public SteamApps001 SteamApps001 { get; set; } = null!;

    public SteamApps008 SteamApps008 { get; set; } = null!;

    public SteamRemoteStorage012 SteamRemoteStorage { get; set; } = null!;

    public bool IsConnectToSteam { get; set; }

    bool _IsDisposed = false;
    int _Pipe;
    int _User;

    readonly List<ICallback> _Callbacks = [];

    const string KEY_STEAM_APP_ID = "SteamAppId";

    public static bool WriteSteamAppIdTxt { get; set; }

    static string? GetProcessDirectoryPath(string? processPath)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(processPath);
#else
        if (processPath == null)
            throw new ArgumentNullException(nameof(processPath));
#endif
        var processDirPath = Path.GetDirectoryName(processPath);
        return processDirPath;
    }

    public bool Initialize(long appId, bool processPathIsReadOnly = false)
    {
        //if (string.IsNullOrEmpty(Steam.GetInstallPath()) == true)
        //{
        //    throw new ClientInitializeException(ClientInitializeFailure.GetInstallPath, "failed to get Steam install path");
        //}

        string? steam_appid_file_path = null;
        bool loadFailed = false;
        try
        {
            if (appId != 0)
            {
                Environment.SetEnvironmentVariable(KEY_STEAM_APP_ID, appId.ToString(CultureInfo.InvariantCulture));

                if (Steam.Load() == false)
                {
                    if (!processPathIsReadOnly)
                    {
                        steam_appid_file_path = "steam_appid.txt";
                        var processPath =
#if NET6_0_OR_GREATER
                            Environment.ProcessPath;
#elif NETFRAMEWORK
                            global::System.Windows.Forms.Application.ExecutablePath;
#else
                            global::System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
#endif
                        var processDirPath = GetProcessDirectoryPath(processPath);
                        if (processDirPath != null)
                        {
                            steam_appid_file_path = Path.Combine(processDirPath, steam_appid_file_path);
                        }
                        File.WriteAllText(steam_appid_file_path, appId.ToString());

                        if (Steam.Load() == false)
                        {
                            loadFailed = true;
                        }
                    }
                    else
                    {
                        loadFailed = true;
                    }

                }
            }
            else
            {
                if (Steam.Load() == false)
                {
                    loadFailed = true;
                }
            }

            if (loadFailed)
            {
                throw new ClientInitializeException(ClientInitializeFailure.Load, "failed to load Steam");
            }

            SteamClient = Steam.CreateInterface<SteamClient018>("SteamClient018")!;
            if (SteamClient == null)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamClient, "failed to create ISteamClient017");
            }

            _Pipe = SteamClient.CreateSteamPipe();
            if (_Pipe == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamPipe, "failed to create pipe");
            }

            _User = SteamClient.ConnectToGlobalUser(_Pipe);
            if (_User == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.ConnectToGlobalUser, "failed to connect to global user");
            }

            SteamUtils = SteamClient.GetSteamUtils007(_Pipe);
            var currentAppId = SteamUtils.GetAppId();
            if (appId > 0 && currentAppId != unchecked((uint)appId))
            {
                //var envAppId = Environment.GetEnvironmentVariable(KEY_STEAM_APP_ID);
                throw new ClientInitializeException(ClientInitializeFailure.AppIdMismatch, $"appID mismatch, appId: {appId}"); //, currentAppId: {currentAppId}, envAppId: {envAppId}");
            }

            SteamUser = SteamClient.GetSteamUser017(_User, _Pipe);
            SteamUserStats = SteamClient.GetSteamUserStats011(_User, _Pipe);
            SteamApps001 = SteamClient.GetSteamApps001(_User, _Pipe);
            SteamApps008 = SteamClient.GetSteamApps008(_User, _Pipe);
            SteamRemoteStorage = SteamClient.GetSteamRemoteStorage012(_User, _Pipe);
        }
        finally
        {
            if (!processPathIsReadOnly && steam_appid_file_path != null)
            {
                try
                {
                    File.Delete(steam_appid_file_path);
                }
                catch
                {
                }
            }
        }

        return true;
    }

    ~Client()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_IsDisposed == true)
        {
            return;
        }

        _IsDisposed = true;

        if (SteamClient != null && _Pipe > 0)
        {
            if (_User > 0)
            {
                SteamClient.ReleaseUser(_Pipe, _User);
                _User = 0;
            }

            SteamClient.ReleaseSteamPipe(_Pipe);
            _Pipe = 0;
        }

        _IsDisposed = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public TCallback CreateAndRegisterCallback<TCallback>()
        where TCallback : ICallback, new()
    {
        var callback = new TCallback();
        _Callbacks.Add(callback);
        return callback;
    }

    private bool _RunningCallbacks;

    public void RunCallbacks(bool server)
    {
        if (_RunningCallbacks == true)
        {
            return;
        }

        _RunningCallbacks = true;

        while (Steam.GetCallback(_Pipe, out CallbackMessage message, out int call) == true)
        {
            var callbackId = message.Id;
            foreach (ICallback callback in _Callbacks.Where(
                candidate => candidate.Id == callbackId &&
                             candidate.IsServer == server))
            {
                callback.Run(message.ParamPointer);
            }

            Steam.FreeLastCallback(_Pipe);
        }

        _RunningCallbacks = false;
    }
}