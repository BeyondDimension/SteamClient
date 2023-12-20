/*
 * Copyright (C) 2015 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#pragma warning disable SA1600 // Elements should be documented

using ReactiveUI;

namespace WinAuth.WinAuth;

/// <summary>
/// SteamClient for logging and getting/accepting/rejecting trade confirmations
/// </summary>
[Obsolete("use ISteamAuthenticatorService or ISteamAccountService")]
public partial class SteamClient : IDisposable
{
    /// <summary>
    /// URLs for all mobile services
    /// </summary>
    const string COMMUNITY_DOMAIN = "steamcommunity.com";
    const string COMMUNITY_BASE = "https://" + COMMUNITY_DOMAIN;
    const string WEBAPI_BASE = "https://api.steampowered.com";
    const string API_GETWGTOKEN = WEBAPI_BASE + "/IMobileAuthService/GetWGToken/v0001";
    const string API_LOGOFF = WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logoff/v0001";
    //const string API_LOGON = WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logon/v0001";
    const string SYNC_URL = "https://api.steampowered.com/ITwoFactorService/QueryTime/v0001";

    /// <summary>
    /// Time for http request when calling Sync in ms
    /// </summary>
    const int SYNC_TIMEOUT = 30000;

    /// <summary>
    /// Default mobile user agent
    /// </summary>
    const string USERAGENT = "Mozilla/5.0 (Linux; Android 8.1.0; Nexus 5X Build/OPM7.181205.001) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Mobile Safari/537.36";

    /// <summary>
    /// Number of Confirmation retries
    /// </summary>
    const int DEFAULT_CONFIRMATIONPOLLER_RETRIES = 3;

    /// <summary>
    /// Delay between trade confirmation events
    /// </summary>
    public const int CONFIRMATION_EVENT_DELAY = 1000;

    /// <summary>
    /// Action for Confirmation polling
    /// </summary>
    public enum PollerAction
    {
        None = 0,
        Notify = 1,
        AutoConfirm = 2,
        SilentAutoConfirm = 3,
    }

    internal static class Utils
    {
        #region 弃用方法
        //public static string SelectTokenValueNotNull(string response, JsonNode token, string path, string? msg = null, Func<string, string, Exception?, Exception>? getWinAuthException = null)
        //{
        //    var valueToken = token;
        //    //临时修复NJ转SJ的访问子节点格式问题
        //    var nodes = path.Split('.');
        //    for (int i = 0; i < nodes.Length; i++)
        //    {
        //        valueToken = valueToken[nodes[i]];
        //    }

        //    if (valueToken != null)
        //    {
        //        var value = valueToken.GetValue<string>();
        //        if (value != null)
        //        {
        //            return value;
        //        }
        //    }
        //    getWinAuthException ??= GetWinAuthException;
        //    throw getWinAuthException(response, msg ?? "SelectTokenValueNotNull", new ArgumentNullException(path));
        //}

        //public static JsonNode SelectTokenNotNull(string response, JsonNode token, string path, string? msg = null, Func<string, string, Exception?, Exception>? getWinAuthException = null)
        //{
        //    var valueToken = token[path];
        //    if (valueToken != null)
        //    {
        //        return valueToken;
        //    }
        //    getWinAuthException ??= GetWinAuthException;
        //    throw getWinAuthException(response, msg ?? "SelectTokenNotNull", new ArgumentNullException(path));
        //}

        #endregion

        public static WinAuthException GetWinAuthException(string response, string msg, Exception? innerException = null)
        {
            return new WinAuthException(
                $"{msg}, response: {response}", innerException);
        }

        public static WinAuthException GetWinAuthInvalidEnrollResponseException(string response, string msg, Exception? innerException = null)
        {
            return new WinAuthInvalidEnrollResponseException(
                $"{msg}, response: {response}", innerException);
        }

        public const string donotache_value = "-62135596800000"; // default(DateTime).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();
    }

    /// <summary>
    /// Hold the Confirmation polling data
    /// </summary>
    public sealed class ConfirmationPoller
    {
        /// <summary>
        /// Seconds between polls
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Action for new Confirmation
        /// </summary>
        public PollerAction Action { get; set; }

        /// <summary>
        /// List of current Confirmations ids
        /// </summary>
        public List<string>? Ids { get; set; }

        /// <summary>
        /// Create new ConfirmationPoller object
        /// </summary>
        public ConfirmationPoller()
        {
        }

        /// <summary>
        /// Create a JSON string of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Duration == 0)
            {
                return "null";
            }
            else
            {
                var props = new List<string>
                {
                    "\"duration\":" + Duration,
                    "\"action\":" + (int)Action,
                };
                if (Ids != null)
                {
                    props.Add("\"ids\":[" + (Ids.Count != 0 ? "\"" + string.Join("\",\"", Ids.ToArray()) + "\"" : string.Empty) + "]");
                }

                return "{" + string.Join(",", props.ToArray()) + "}";
            }
        }

        #region 弃用方法

        ///// <summary>
        ///// Create a new ConfirmationPoller from a JSON string
        ///// </summary>
        ///// <param name="json">JSON string</param>
        ///// <returns>new ConfirmationPoller or null</returns>
        //public static ConfirmationPoller? FromJSON(string json)
        //{
        //    if (string.IsNullOrEmpty(json) == true || json == "null")
        //    {
        //        return null;
        //    }
        //    var poller = FromJSON(JsonSerializer.Deserialize(json, SteamJsonContext.Default.ConfirmationPoller));
        //    return poller?.Duration != 0 ? poller : null;
        //}

        ///// <summary>
        ///// Create a new ConfirmationPoller from a JToken
        ///// </summary>
        ///// <param name="tokens">existing JKToken</param>
        ///// <returns></returns>
        //public static ConfirmationPoller? FromJSON(ConfirmationPoller? tokens)
        //{
        //    if (tokens == null)
        //    {
        //        return null;
        //    }

        //    var poller = new ConfirmationPoller();

        //    poller.Duration = tokens.Duration;
        //    poller.Action = tokens.Action;
        //    poller.Ids = tokens.Ids;

        //    return poller.Duration != 0 ? poller : null;
        //}
        #endregion
    }

    /// <summary>
    /// A class for a single confirmation
    /// </summary>
    public sealed class Confirmation : ReactiveObject
    {
        public string Id { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public bool Offline { get; set; }

        public bool IsNew { get; set; }

        public string Image { get; set; } = string.Empty;

        private bool _ButtonEnable = true;

        public bool ButtonEnable
        {
            get => _ButtonEnable;
            set => this.RaiseAndSetIfChanged(ref _ButtonEnable, value);
        }

        private int _IsOperate;

        public int IsOperate
        {
            get => _IsOperate;
            set => this.RaiseAndSetIfChanged(ref _IsOperate, value);
        }

        private bool _NotChecked;

        /// <summary>
        /// 未勾选
        /// </summary>
        public bool NotChecked
        {
            get => _NotChecked;
            set => this.RaiseAndSetIfChanged(ref _NotChecked, value);
        }

        public string Details { get; set; } = string.Empty;

        public string Traded { get; set; } = string.Empty;

        public string When { get; set; } = string.Empty;
    }

    /// <summary>
    /// Session state to remember logins
    /// </summary>
    public sealed partial class SteamSession
    {
        public SteamSession()
        {
        }

        public SteamSession(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            try
            {
                var session = SystemTextJsonSerializer.Deserialize<SteamSession>(json);
                if (session == null)
                    throw new ArgumentNullException(nameof(session));

                this.SteamID = session.SteamID;
                this.AccessToken = session.AccessToken;
                this.RefreshToken = session.RefreshToken;
                this.SessionID = session.SessionID;
            }
            catch
            { }
        }

        [JsonPropertyName("steamid")]
        public ulong SteamID { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("sessionid")]
        public string? SessionID { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? Language { get; set; }

        [Obsolete("use ISteamAuthenticatorService.RefreshAccessToken")]
        public Task RefreshAccessToken(SteamClient client)
        {
            throw new NotImplementedException();
        }

        public bool IsAccessTokenExpired()
        {
            if (string.IsNullOrEmpty(this.AccessToken))
                return true;

            return IsTokenExpired(this.AccessToken);
        }

        public bool IsRefreshTokenExpired()
        {
            if (string.IsNullOrEmpty(this.RefreshToken))
                return true;

            return IsTokenExpired(this.RefreshToken);
        }

        private bool IsTokenExpired(string token)
        {
            var tokenComponents = token.Split('.');
            // Fix up base64url to normal base64
            var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

            if (base64.Length % 4 != 0)
            {
                base64 += new string('=', 4 - (base64.Length % 4));
            }

            var payloadBytes = Convert.FromBase64String(base64);
            var jwt = JsonConvert.DeserializeObject<SteamAccessToken>(System.Text.Encoding.UTF8.GetString(payloadBytes));

            // Compare expire time of the token to the current time
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > jwt.Exp;
        }

        public CookieContainer GetCookies()
        {
            if (this.SessionID == null)
                this.SessionID = GenerateSessionID();

            var cookies = new CookieContainer();
            cookies.Add(new Cookie("steamLoginSecure", this.GetSteamLoginSecure(), "/", "steamcommunity.com"));
            cookies.Add(new Cookie("sessionid", this.SessionID, "/", "steamcommunity.com"));
            cookies.Add(new Cookie("mobileClient", "android", "/", "steamcommunity.com"));
            cookies.Add(new Cookie("mobileClientVersion", "777777 3.6.1", "/", "steamcommunity.com"));
            cookies.Add(new Cookie("Steam_Language", string.IsNullOrEmpty(Language) ? "english" : Language, "/", "steamcommunity.com"));
            return cookies;
        }

        private string GetSteamLoginSecure()
        {
            return this.SteamID.ToString() + "%7C%7C" + this.AccessToken;
        }

        private static string GenerateSessionID()
        {
            return GetRandomHexNumber(32);
        }

        private static string GetRandomHexNumber(int digits)
        {
            Random random = new Random();
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        public override string ToString()
        {
            return SystemTextJsonSerializer.Serialize(this);
        }

        private class SteamAccessToken
        {
            [JsonProperty("exp")]
            public long Exp { get; set; }
        }

        private class GenerateAccessTokenForAppResponse
        {
            [JsonProperty("response")]
            public GenerateAccessTokenForAppResponseResponse? Response;
        }

        private class GenerateAccessTokenForAppResponseResponse
        {
            [JsonProperty("access_token")]
            public string? AccessToken { get; set; }
        }
    }

    /// <summary>
    /// Login state fields
    /// </summary>
    public bool InvalidLogin;
    //public bool RequiresCaptcha;
    //public string? CaptchaId;
    //public string? CaptchaUrl;
    public bool Requires2FA;
    public bool RequiresEmailAuth;
    public string? EmailDomain;
    public string? Error;

    /// <summary>
    /// Current session
    /// </summary>
    public SteamSession? Session;

    /// <summary>
    /// Current authenticator
    /// </summary>
    public SteamAuthenticator Authenticator;

    // /// <summary>
    // /// Saved Html from GetConfirmations used as template for GetDetails
    // /// </summary>
    // string? ConfirmationsHtml;

    public string? SteamUserImageUrl;

    /// <summary>
    /// Query string from GetConfirmations used in GetDetails
    /// </summary>
    string? ConfirmationsQuery;

    /// <summary>
    /// Cancellation token for poller
    /// </summary>
    CancellationTokenSource? _pollerCancellation;

    /// <summary>
    /// Number of Confirmation retries
    /// </summary>
    public int ConfirmationPollerRetries = DEFAULT_CONFIRMATIONPOLLER_RETRIES;

    HttpClient? _httpClient;

    /// <summary>
    /// Delegate for Confirmation event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="newconfirmation">new Confirmation</param>
    /// <param name="action">action to be taken</param>
    public delegate void ConfirmationDelegate(object sender, SteamMobileTradeConf newconfirmation, PollerAction action);

    /// <summary>
    /// Delegate for Confirmation error
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message">error message</param>
    /// <param name="action"></param>
    /// <param name="ex">optional exception</param>
    public delegate void ConfirmationErrorDelegate(object sender, string message, PollerAction action, Exception ex);

    /// <summary>
    /// Event fired for new Confirmation
    /// </summary>
    public event ConfirmationDelegate? ConfirmationEvent;

    /// <summary>
    /// Event fired for error on polling
    /// </summary>
    public event ConfirmationErrorDelegate? ConfirmationErrorEvent;

    /// <summary>
    /// Create a new SteamClient
    /// </summary>
    public SteamClient(SteamAuthenticator auth)
    {
        Authenticator = auth;
    }

    /// <summary>
    /// Finalizer
    /// </summary>
    ~SteamClient()
    {
        _httpClient?.Dispose();
        Dispose(false);
    }

    /// <summary>
    /// Dispose the object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose this object
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // clear resources
        }

        if (_pollerCancellation != null)
        {
            _pollerCancellation.Cancel();
            _pollerCancellation = null;
        }
    }

    #region Public

    /// <summary>
    /// Clear the client state
    /// </summary>
    public void Clear()
    {
        InvalidLogin = false;
        RequiresEmailAuth = false;
        EmailDomain = null;
        Requires2FA = false;
        Error = null;

        Session = null;
    }

    /// <summary>
    /// Session Set
    /// </summary>
    /// <param name="session"></param>
    /// <param name="language"></param>
    public void SessionSet(string? session = null, string? language = null)
    {
        SocketsHttpHandler handler = new()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            MaxAutomaticRedirections = 1000,
        };

        if (!string.IsNullOrEmpty(session))
        {
            Session = new SteamSession(session)
            {
                Language = language
            };
            handler.UseCookies = true;
            handler.CookieContainer = Session.GetCookies();
        }
        else
            Session = new SteamSession();

        _httpClient = new HttpClient(handler);
        //_httpClient.DefaultRequestHeaders.Add("Accept", "text/javascript, text/html, application/xml, text/xml, */*");
        //_httpClient.DefaultRequestHeaders.Add("Referer", COMMUNITY_BASE);
        //_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", USERAGENT);
        _httpClient.Timeout = new TimeSpan(0, 0, 45);
        _httpClient.DefaultRequestHeaders.ExpectContinue = false;
    }

    /// <summary>
    /// Check if user is logged in
    /// </summary>
    /// <returns></returns>
    public bool IsLoggedIn() => string.IsNullOrEmpty(Session?.AccessToken) == false;

    /// <summary>
    /// Logout of the current session
    /// </summary>
    //[Obsolete("use LogoutAsync")]
    public void Logout()
    {
        if (string.IsNullOrEmpty(Session?.AccessToken) == false)
        {
            PollConfirmationsStop();

            // if (string.IsNullOrEmpty(Session.UmqId) == false)
            // {
            //     var data = new NameValueCollection
            //     {
            //         { "access_token", Session.OAuthToken },
            //         { "umqid", Session.UmqId },
            //     };
            //     GetString(API_LOGOFF, "POST", data);
            // }
        }

        Clear();
    }

    /// <summary>
    /// Get the current trade Confirmations
    /// </summary>
    /// <returns>list of Confirmation objects</returns>
    [Obsolete("use ISteamTradeService.GetConfirmations")]
    public Task<IEnumerable<SteamMobileTradeConf>?> GetConfirmations()
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamTradeService.GetConfirmationImages")]
    public Task<(IEnumerable<string> receiveItems, IEnumerable<string> sendItems)> GetConfirmationItemImageUrls(string tradeId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Confirm or reject a specific trade confirmation
    /// </summary>
    /// <param name="trades">Id and Key</param>
    /// <param name="accept">true to accept, false to reject</param>
    /// <returns>true if successful</returns>
    [Obsolete("use ISteamTradeService.BatchSendConfirmation")]
    public Task<bool> ConfirmTrade(Dictionary<string, string> trades, bool accept)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAccountService.GetRSAKeyAsync")]
    public Task<string> GetRSAKeyAsync(string donotache, string username, CookieContainer? cookies)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.AddAuthenticatorAsync")]
    public Task<string> AddAuthenticatorAsync(string? steamid, string authenticator_time, string? device_identifier, string access_token, string authenticator_type = "1", string sms_phone_id = "1")
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.FinalizeAddAuthenticatorAsync")]
    public Task<string> FinalizeAddAuthenticatorAsync(string? steamid, string? activation_code, string authenticator_code, string authenticator_time, string access_token, string validate_sms_code = "1")
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.GetUserCountry")]
    public Task<string> GetUserCountry(string access_token, string steamid)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.AddPhoneNumberAsync")]
    public Task<string> AddPhoneNumberAsync(string phone_number, string? contury_code, string access_token)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.AccountWaitingForEmailConfirmation")]
    public Task<string> AccountWaitingForEmailConfirmation(string access_token)
    {
        throw new NotImplementedException();
    }

    public Task<string> SendPhoneVerificationCode(string access_token)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.RemoveAuthenticatorAsync")]
    public Task<string> RemoveAuthenticatorAsync(string? revocation_code, string steamguard_scheme, string access_token, string revocation_reason = "1")
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.RemoveAuthenticatorViaChallengeStartSync")]
    public Task<SteamKit2.Internal.CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response> RemoveAuthenticatorViaChallengeStartSync(string access_token)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.RemoveAuthenticatorViaChallengeContinueSync")]
    public Task<SteamKit2.Internal.CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response> RemoveAuthenticatorViaChallengeContinueSync(string? sms_code, string access_token, bool generate_new_token = true)
    {
        throw new NotImplementedException();
    }

    [Obsolete("use ISteamAuthenticatorService.TwoFAQueryTime")]
    public Task<string> TwoFAQueryTime()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ToolMethod

    /// <summary>
    /// Stop the current poller
    /// </summary>
    protected void PollConfirmationsStop()
    {
        // kill any existing poller
        if (_pollerCancellation != null)
        {
            _pollerCancellation.Cancel();
            _pollerCancellation = null;
        }
    }
    #endregion
}