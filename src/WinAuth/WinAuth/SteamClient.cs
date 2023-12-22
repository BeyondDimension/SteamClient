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

using ReactiveUI;

namespace WinAuth.WinAuth;

/// <summary>
/// SteamClient 用于记录和获取/接受/拒绝交易确认
/// </summary>
[Obsolete("use ISteamAuthenticatorService or ISteamAccountService")]
public partial class SteamClient : IDisposable
{
    /// <summary>
    /// 所有移动服务的 url
    /// </summary>
    const string COMMUNITY_DOMAIN = "steamcommunity.com";
    const string COMMUNITY_BASE = "https://" + COMMUNITY_DOMAIN;
    const string WEBAPI_BASE = "https://api.steampowered.com";
    const string API_GETWGTOKEN = WEBAPI_BASE + "/IMobileAuthService/GetWGToken/v0001";
    const string API_LOGOFF = WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logoff/v0001";
    //const string API_LOGON = WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logon/v0001";
    const string SYNC_URL = "https://api.steampowered.com/ITwoFactorService/QueryTime/v0001";

    /// <summary>
    /// 调用同步时 http 请求的时间(毫秒)
    /// </summary>
    const int SYNC_TIMEOUT = 30000;

    /// <summary>
    /// 默认移动用户代理
    /// </summary>
    const string USERAGENT = "Mozilla/5.0 (Linux; Android 8.1.0; Nexus 5X Build/OPM7.181205.001) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Mobile Safari/537.36";

    /// <summary>
    /// 确认重试次数
    /// </summary>
    const int DEFAULT_CONFIRMATIONPOLLER_RETRIES = 3;

    /// <summary>
    /// 交易确认事件之间的延迟
    /// </summary>
    public const int CONFIRMATION_EVENT_DELAY = 1000;

    /// <summary>
    /// 确认轮询的操作
    /// </summary>
    public enum PollerAction
    {
        None = 0,
        Notify = 1,
        AutoConfirm = 2,
        SilentAutoConfirm = 3,
    }

    /// <summary>
    /// 用于处理身份验证的异常
    /// </summary>
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

        /// <summary>
        /// 获取 WinAuth 异常
        /// </summary>
        /// <param name="response">响应字符串</param>
        /// <param name="msg">错误消息</param>
        /// <param name="innerException">内部异常</param>
        /// <returns>WinAuth 异常实例</returns>
        public static WinAuthException GetWinAuthException(string response, string msg, Exception? innerException = null)
        {
            return new WinAuthException(
                $"{msg}, response: {response}", innerException);
        }

        /// <summary>
        /// 获取无效的 WinAuth 注册响应异常
        /// </summary>
        /// <param name="response">响应字符串</param>
        /// <param name="msg">错误消息</param>
        /// <param name="innerException">内部异常</param>
        /// <returns>无效的 WinAuth 注册响应异常实例</returns>
        public static WinAuthException GetWinAuthInvalidEnrollResponseException(string response, string msg, Exception? innerException = null)
        {
            return new WinAuthInvalidEnrollResponseException(
                $"{msg}, response: {response}", innerException);
        }

        /// <summary>
        /// 此标志表示不应缓存从此查询检索到的名称
        /// </summary>
        public const string donotache_value = "-62135596800000"; // default(DateTime).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();
    }

    /// <summary>
    /// 保留确认轮询数据
    /// </summary>
    public sealed class ConfirmationPoller
    {
        /// <summary>
        /// 两次轮询之间的秒数
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 新确认的操作
        /// </summary>
        public PollerAction Action { get; set; }

        /// <summary>
        /// 当前确认 ID 列表
        /// </summary>
        public List<string>? Ids { get; set; }

        /// <summary>
        /// 创建新的 ConfirmationPoller 对象
        /// </summary>
        public ConfirmationPoller()
        {
        }

        /// <summary>
        /// 创建对象的 JSON 字符串
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
    /// 用于单个确认的类
    /// </summary>
    public sealed class Confirmation : ReactiveObject
    {
        /// <summary>
        /// 确认的 Id
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 确认的键
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 是否离线确认
        /// </summary>
        public bool Offline { get; set; }

        /// <summary>
        /// 是否为新确认
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 确认的图片
        /// </summary>
        public string Image { get; set; } = string.Empty;

        private bool _ButtonEnable = true;

        /// <summary>
        /// 按钮是否可用
        /// </summary>
        public bool ButtonEnable
        {
            get => _ButtonEnable;
            set => this.RaiseAndSetIfChanged(ref _ButtonEnable, value);
        }

        private int _IsOperate;

        /// <summary>
        /// 操作状态
        /// </summary>
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

        /// <summary>
        /// 确认的详细信息
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// 已交易信息
        /// </summary>
        public string Traded { get; set; } = string.Empty;

        /// <summary>
        /// 确认时间
        /// </summary>
        public string When { get; set; } = string.Empty;
    }

    /// <summary>
    /// 会话状态以记住登录
    /// </summary>
    public sealed partial class SteamSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SteamSession"/> class.
        /// </summary>
        public SteamSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamSession"/> class.
        /// </summary>
        /// <param name="json"></param>
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

        /// <summary>
        /// Steam 用户的唯一标识符
        /// </summary>
        [JsonPropertyName("steamid")]
        public ulong SteamID { get; set; }

        /// <summary>
        /// 访问令牌，用于验证用户访问权限
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// 刷新令牌，用于获取新的访问令牌
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// 会话 Id，用于维持会话状态
        /// </summary>
        [JsonPropertyName("sessionid")]
        public string? SessionID { get; set; }

        /// <summary>
        /// 语言设置
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string? Language { get; set; }

        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        /// <param name="client"></param>
        /// <exception cref="NotImplementedException"></exception>
        [Obsolete("use ISteamAuthenticatorService.RefreshAccessToken")]
        public Task RefreshAccessToken(SteamClient client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 检查访问令牌是否已过期
        /// </summary>
        /// <returns>如果访问令牌已过期，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
        public bool IsAccessTokenExpired()
        {
            if (string.IsNullOrEmpty(this.AccessToken))
                return true;

            return IsTokenExpired(this.AccessToken);
        }

        /// <summary>
        /// 检查刷新令牌是否已过期
        /// </summary>
        /// <returns>如果刷新令牌已过期，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
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

        /// <summary>
        /// 获取会话的 Cookie 容器
        /// </summary>
        /// <returns>包含会话 Cookie 的 CookieContainer 对象</returns>
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

        /// <summary>
        /// 将当前实例转换为 JSON 字符串
        /// </summary>
        /// <returns>包含当前实例信息的 JSON 字符串</returns>
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
    /// 登录状态字段
    /// </summary>
    public bool InvalidLogin;
    //public bool RequiresCaptcha;
    //public string? CaptchaId;
    //public string? CaptchaUrl;

    /// <summary>
    /// 登录是否无效
    /// </summary>
    public bool Requires2FA;

    /// <summary>
    /// 是否需要二步验证
    /// </summary>
    public bool RequiresEmailAuth;

    /// <summary>
    /// 电子邮件域
    /// </summary>
    public string? EmailDomain;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? Error;

    /// <summary>
    /// 当前会话
    /// </summary>
    public SteamSession? Session;

    /// <summary>
    /// 当前的身份
    /// </summary>
    public SteamAuthenticator Authenticator;

    // /// <summary>
    // /// Saved Html from GetConfirmations used as template for GetDetails
    // /// </summary>
    // string? ConfirmationsHtml;

    /// <summary>
    /// Steam 用户头像链接
    /// </summary>
    public string? SteamUserImageUrl;

    /// <summary>
    /// 从 GetDetails 中使用的 getconfirations 查询字符串
    /// </summary>
    string? ConfirmationsQuery;

    /// <summary>
    /// 轮询器的取消令牌
    /// </summary>
    CancellationTokenSource? _pollerCancellation;

    /// <summary>
    /// 确认重试次数
    /// </summary>
    public int ConfirmationPollerRetries = DEFAULT_CONFIRMATIONPOLLER_RETRIES;

    HttpClient? _httpClient;

    /// <summary>
    /// 确认事件代表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="newconfirmation">新确认</param>
    /// <param name="action">应采取的行动</param>
    public delegate void ConfirmationDelegate(object sender, SteamMobileTradeConf newconfirmation, PollerAction action);

    /// <summary>
    /// 委托确认错误
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message">error message</param>
    /// <param name="action"></param>
    /// <param name="ex">optional exception</param>
    public delegate void ConfirmationErrorDelegate(object sender, string message, PollerAction action, Exception ex);

    /// <summary>
    /// 为新的确认触发事件
    /// </summary>
    public event ConfirmationDelegate? ConfirmationEvent;

    /// <summary>
    /// 事件因轮询错误而触发
    /// </summary>
    public event ConfirmationErrorDelegate? ConfirmationErrorEvent;

    /// <summary>
    /// 创建一个新的 SteamClient
    /// </summary>
    public SteamClient(SteamAuthenticator auth)
    {
        Authenticator = auth;
    }

    /// <summary>
    /// 终结器
    /// </summary>
    ~SteamClient()
    {
        _httpClient?.Dispose();
        Dispose(false);
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放
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
    /// 清除客户端状态
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
    /// 会话组
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
    /// 检查用户是否已登录
    /// </summary>
    /// <returns></returns>
    public bool IsLoggedIn() => string.IsNullOrEmpty(Session?.AccessToken) == false;

    /// <summary>
    /// 退出当前会话
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

    /// <inheritdoc cref="ISteamTradeService.GetConfirmations"/>
    [Obsolete("use ISteamTradeService.GetConfirmations")]
    public Task<IEnumerable<SteamMobileTradeConf>?> GetConfirmations()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamTradeService.GetConfirmationImages"/>
    [Obsolete("use ISteamTradeService.GetConfirmationImages")]
    public Task<(IEnumerable<string> receiveItems, IEnumerable<string> sendItems)> GetConfirmationItemImageUrls(string tradeId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamTradeService.BatchSendConfirmation"/>
    [Obsolete("use ISteamTradeService.BatchSendConfirmation")]
    public Task<bool> ConfirmTrade(Dictionary<string, string> trades, bool accept)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取 RSA 密钥
    /// </summary>
    [Obsolete("use ISteamAccountService.GetRSAKeyAsync")]
    public Task<string> GetRSAKeyAsync(string donotache, string username, CookieContainer? cookies)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.AddAuthenticatorAsync"/>
    [Obsolete("use ISteamAuthenticatorService.AddAuthenticatorAsync")]
    public Task<string> AddAuthenticatorAsync(string? steamid, string authenticator_time, string? device_identifier, string access_token, string authenticator_type = "1", string sms_phone_id = "1")
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.FinalizeAddAuthenticatorAsync"/>
    [Obsolete("use ISteamAuthenticatorService.FinalizeAddAuthenticatorAsync")]
    public Task<string> FinalizeAddAuthenticatorAsync(string? steamid, string? activation_code, string authenticator_code, string authenticator_time, string access_token, string validate_sms_code = "1")
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.GetUserCountry"/>
    [Obsolete("use ISteamAuthenticatorService.GetUserCountry")]
    public Task<string> GetUserCountry(string access_token, string steamid)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.AddPhoneNumberAsync"/>
    [Obsolete("use ISteamAuthenticatorService.AddPhoneNumberAsync")]
    public Task<string> AddPhoneNumberAsync(string phone_number, string? contury_code, string access_token)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.AccountWaitingForEmailConfirmation"/>
    [Obsolete("use ISteamAuthenticatorService.AccountWaitingForEmailConfirmation")]
    public Task<string> AccountWaitingForEmailConfirmation(string access_token)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 发送手机验证码
    /// </summary>
    public Task<string> SendPhoneVerificationCode(string access_token)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.RemoveAuthenticatorAsync"/>
    [Obsolete("use ISteamAuthenticatorService.RemoveAuthenticatorAsync")]
    public Task<string> RemoveAuthenticatorAsync(string? revocation_code, string steamguard_scheme, string access_token, string revocation_reason = "1")
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.RemoveAuthenticatorViaChallengeStartSync"/>
    [Obsolete("use ISteamAuthenticatorService.RemoveAuthenticatorViaChallengeStartSync")]
    public Task<SteamKit2.Internal.CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response> RemoveAuthenticatorViaChallengeStartSync(string access_token)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.RemoveAuthenticatorViaChallengeContinueSync"/>
    [Obsolete("use ISteamAuthenticatorService.RemoveAuthenticatorViaChallengeContinueSync")]
    public Task<SteamKit2.Internal.CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response> RemoveAuthenticatorViaChallengeContinueSync(string? sms_code, string access_token, bool generate_new_token = true)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISteamAuthenticatorService.TwoFAQueryTime"/>
    [Obsolete("use ISteamAuthenticatorService.TwoFAQueryTime")]
    public Task<string> TwoFAQueryTime()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ToolMethod

    /// <summary>
    /// 停止当前轮询器
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