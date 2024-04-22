using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BD.SteamClient.Models.Profile;
using Google.Protobuf;
using Polly;
using Polly.Retry;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.SteamClient.Services;

public sealed partial class SteamAccountService : HttpClientUseCookiesWithDynamicProxyServiceImpl, ISteamAccountService
{
    public const string default_donotache = "-62135596800000"; // default(DateTime).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();

    readonly IRandomGetUserAgentService uas;

    readonly ISteamSessionService sessions;

    [ActivatorUtilitiesConstructor]
    public SteamAccountService(
        IServiceProvider s,
        IRandomGetUserAgentService uas,
        ISteamSessionService ssessions,
        ILogger<SteamAccountService> logger) : base(s, logger)
    {
        this.uas = uas;
        this.sessions = ssessions;
    }

    public SteamAccountService(
        Func<HttpHandlerType, HttpClient>? func,
        IRandomGetUserAgentService uas,
        ISteamSessionService ssession,
        ILogger<SteamAccountService> logger) : base(func, logger)
    {
        this.uas = uas;
        this.sessions = ssession;
    }

    public SteamAccountService(
        IServiceProvider s,
        Func<CookieContainer, HttpMessageHandler> func) : base(func, s.GetRequiredService<ILogger<SteamAccountService>>())
    {
        uas = s.GetRequiredService<IRandomGetUserAgentService>();
        sessions = s.GetRequiredService<ISteamSessionService>();
    }

    public bool UseRetry { get; set; } = true;

    /// <summary>
    /// 等待一个时间并且重试
    /// </summary>
    /// <returns></returns>
    AsyncRetryPolicy WaitAndRetryAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0) => Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), }, (ex, _, i, _) =>
        {
            logger.LogError(ex, "第 {i} 次重试，MemberName：{memberName}，FilePath：{sourceFilePath}，LineNumber：{sourceLineNumber}", i, memberName, sourceFilePath, sourceLineNumber);
        });

    [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync<T>(JsonSerializerOptions, CancellationToken)")]
    public async Task<(string encryptedPassword64, string timestamp)> GetRSAkeyAsync(string username, string password)
    {
        var data = new Dictionary<string, string>()
        {
            { "donotache", default_donotache },
            { "username", username },
        };
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync<T>(JsonSerializerOptions, CancellationToken)")]
        async Task<(string encryptedPassword64, string timestamp)> GetRSAkeyAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetRSAkeyUrl)
            {
                Content = new FormUrlEncodedContent(data),
            };
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            var result = await client.SendAsync(request);

            if (!result.IsSuccessStatusCode)
                throw new Exception($"获取 RSAKey 出现错误: {result.StatusCode}");

            var jsonObj = await result.Content.ReadFromJsonAsync<JsonObject>();

            if (jsonObj == null)
                throw new Exception($"获取 RSAKey 出现错误: 无效的{nameof(jsonObj)}");

            var success = jsonObj["success"]?.GetValue<bool>();
            var publickey_exp = jsonObj["publickey_exp"]?.GetValue<string>();
            var publickey_mod = jsonObj["publickey_mod"]?.GetValue<string>();
            var timestamp = jsonObj["timestamp"]?.GetValue<string>();
            if (!(success.HasValue && success.Value) ||
                string.IsNullOrEmpty(publickey_exp) ||
                string.IsNullOrEmpty(publickey_mod) ||
                string.IsNullOrEmpty(timestamp))
                throw new Exception($"获取 RSAKey 出现错误: " + jsonObj.ToJsonString());

            // 使用 RSA 密钥加密密码
            using var rsa = new RSACryptoServiceProvider();
            var passwordBytes = Encoding.ASCII.GetBytes(password);
            var p = rsa.ExportParameters(false);
            p.Exponent = Convert.FromHexString(publickey_exp);
            p.Modulus = Convert.FromHexString(publickey_mod);
            rsa.ImportParameters(p);
            byte[] encryptedPassword = rsa.Encrypt(passwordBytes, false);
            var encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
            return (encryptedPassword64, timestamp);
        }
        var result = UseRetry ? await WaitAndRetryAsync().ExecuteAsync(GetRSAkeyAsync) : await GetRSAkeyAsync();
        return result;
    }

    /// <summary>
    /// Steam 会从用户名和密码中删除所有非 ASCII 字符
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("[^\\u0000-\\u007F]")]
    private static partial Regex SteamUNPWDRegex();

    [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync<T>(JsonSerializerOptions, CancellationToken)")]
    public async Task DoLoginAsync(SteamLoginState loginState, bool isTransfer = false, bool isDownloadCaptchaImage = false)
    {
        loginState.Success = false;

        if (string.IsNullOrEmpty(loginState.Username) ||
            string.IsNullOrEmpty(loginState.Password))
        {
            loginState.Message = "请填写正确的 Steam 用户名密码";
            return;
        }

        loginState.Username = SteamUNPWDRegex().Replace(loginState.Username, string.Empty);
        loginState.Password = SteamUNPWDRegex().Replace(loginState.Password, string.Empty);

        if (string.IsNullOrEmpty(cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid")))
        {
            // 访问一次登录页获取SessionId
            await client.GetAsync(SteamLoginUrl);
        }

        var (encryptedPassword64, timestamp) = await GetRSAkeyAsync(loginState.Username, loginState.Password);

        var data = new Dictionary<string, string>()
        {
            { "password", encryptedPassword64 },
            { "username", loginState.Username },
            { "twofactorcode", loginState.TwofactorCode ?? string.Empty },
            { "emailauth", loginState.EmailCode ?? string.Empty },
            { "loginfriendlyname", string.Empty },
            { "captchagid", string.IsNullOrEmpty(loginState.CaptchaId) == false ? loginState.CaptchaId : "-1" },
            { "captcha_text", string.IsNullOrEmpty(loginState.CaptchaText) == false ? loginState.CaptchaText : string.Empty },
            { "emailsteamid", (string.IsNullOrEmpty(loginState.EmailCode) == false ? loginState.SteamIdString ?? string.Empty : string.Empty) },
            { "rsatimestamp", timestamp.ToString() },
            { "remember_login", "false" },
            { "donotache", default_donotache },
        };

        var respone = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, DologinUrl)
            {
                Content = new FormUrlEncodedContent(data),
            };

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

            if (loginState.Cookies != null)
                cookieContainer.Add(loginState.Cookies);

            var respone = await client.SendAsync(request);
            return respone;
        });

        if (respone.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = respone.Headers.RetryAfter?.ToString();
            loginState.Message = $"{HttpStatusCode.TooManyRequests} 请求过于频繁，请稍后再试，{retryAfter}";
            logger.LogError(loginState.Message);
            loginState.Requires2FA = false;
            loginState.RequiresCaptcha = false;
            loginState.RequiresEmailAuth = false;
            loginState.Success = false;
            return;
        }

        JsonObject? jsonObj = null;
        if (respone.IsSuccessStatusCode)
            jsonObj = await respone.Content.ReadFromJsonAsync<JsonObject>();

        if (jsonObj == null)
        {
            try
            {
                var errorStr = await respone.Content.ReadAsStringAsync();
                logger.LogError("login fail, error: {errorStr}, statusCode: {statusCode}.", errorStr, respone.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "login fail, statusCode: {statusCode}.", respone.StatusCode);
            }
            loginState.Message = $"登录错误: 无效的 {nameof(jsonObj)}";
            loginState.Success = false;
            return;
        }

        var message = jsonObj["message"]?.GetValue<string>();

        //var success = jsonObj["success"]?.GetValue<bool>();
        //loginState.Success = success.HasValue && success.Value;
        //if (loginState.Success)
        //{
        //    throw new Exception($"doLogin 出现错误: 无效的用户名");
        //}

        var emailsteamid = jsonObj["emailsteamid"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(emailsteamid))
        {
            loginState.SteamIdString = emailsteamid;
        }

        var captcha_needed = jsonObj["captcha_needed"]?.GetValue<bool>();
        loginState.RequiresCaptcha = captcha_needed.HasValue && captcha_needed.Value;
        if (loginState.RequiresCaptcha)
        {
            if (message?.Contains("验证码中的字符", StringComparison.OrdinalIgnoreCase) == true)
            {
                loginState.CaptchaId = jsonObj["captcha_gid"]?.GetValue<string>();
                loginState.CaptchaUrl = CaptchaImageUrl + loginState.CaptchaId;
                if (isDownloadCaptchaImage && !string.IsNullOrEmpty(loginState.CaptchaId))
                {
                    loginState.CaptchaImageBase64 = await GetCaptchaImageBase64(loginState.CaptchaId);
                }
                loginState.Message = $"登录错误: " + message;
                return;
            }
            else
            {
                loginState.RequiresCaptcha = false;
                loginState.CaptchaId = null;
                loginState.CaptchaUrl = null;
                loginState.CaptchaText = null;
                loginState.Message = $"登录错误: " + message;
                return;
            }
        }
        else
        {
            loginState.RequiresCaptcha = false;
            loginState.CaptchaId = null;
            loginState.CaptchaUrl = null;
            loginState.CaptchaText = null;
        }

        // require email auth
        var emailauth_needed = jsonObj["emailauth_needed"]?.GetValue<bool>();
        loginState.RequiresEmailAuth = emailauth_needed.HasValue && emailauth_needed.Value;
        if (loginState.RequiresEmailAuth)
        {
            var emaildomain = jsonObj["emaildomain"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(emaildomain))
            {
                loginState.EmailDomain = emaildomain;
            }
            loginState.Message = $"登录错误: 需要邮箱验证码";
        }
        else
        {
            loginState.EmailDomain = null;
        }

        // require 2fa auth
        var requires_twofactor = jsonObj["requires_twofactor"]?.GetValue<bool>();
        loginState.Requires2FA = requires_twofactor.HasValue && requires_twofactor.Value;
        if (loginState.Requires2FA)
        {
            loginState.Message = $"登录错误: 需要输入令牌";
            return;
        }

        // 登录因为其它原因失败
        var login_complete = jsonObj["login_complete"]?.GetValue<bool>();
        //var oauth = jsonObj["oauth"]?.GetValue<string>();
        loginState.Success = login_complete.HasValue && login_complete.Value;
        if (!loginState.Success)
        {
            //if (string.IsNullOrEmpty(oauth))
            //{
            //    throw new Exception($"doLogin 出现错误: Invalid response from Steam (No OAuth token)");
            //}
            if (!string.IsNullOrEmpty(message))
            {
                loginState.Message = $"登录错误: " + message;
                return;
            }
        }
        else
        {
            // oauth登录成功
            //var oauthjson = JObject.Parse(oauth);
            //loginState.OAuthToken = SelectTokenValueNotNull<string>(oauth, oauthjson, "oauth_token");
            //if (oauthjson.SelectToken("steamid") != null)
            //{
            //    loginState.SteamId = SelectTokenValueNotNull<string>(oauth, oauthjson, "steamid");
            //}

            // 登录成功
            var doLoginRespone = JsonSerializer.Deserialize(jsonObj.ToString(), DoLoginRespone_.Default.DoLoginRespone);
            if (doLoginRespone != null)
            {
                var session = new SteamSession();
                session.CookieContainer = cookieContainer;
                session.SteamId = loginState.SteamId.ToString();
                loginState.Cookies = cookieContainer.GetAllCookies();
                //loginState.Cookies.Add(new Cookie("sessionid", "", "/", "steamcommunity.com"));
                if (doLoginRespone.TransferParameters != null && doLoginRespone.TransferUrls != null)
                {
                    session.AccessToken = doLoginRespone.TransferParameters.Auth;
                    loginState.SteamIdString = doLoginRespone.TransferParameters.Steamid;

                    _ = ulong.TryParse(loginState.SteamIdString, out var steamid);
                    loginState.SteamId = steamid;

                    if (isTransfer)
                    {
                        foreach (var url in doLoginRespone.TransferUrls)
                        {
                            await WaitAndRetryAsync().ExecuteAsync(async () =>
                            {
                                var req = new HttpRequestMessage(HttpMethod.Post, url)
                                {
                                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                                    {
                                          { "steamid", doLoginRespone.TransferParameters.Steamid },
                                          { "token_secure", doLoginRespone.TransferParameters.TokenSecure },
                                          { "auth", doLoginRespone.TransferParameters.Auth },
                                          { "remember_login", doLoginRespone.TransferParameters.RememberLogin.ToString() },
                                          { "webcookie", doLoginRespone.TransferParameters.Webcookie },
                                    }),
                                };

                                req.Headers.UserAgent.Clear();
                                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

                                (await client.SendAsync(req)).EnsureSuccessStatusCode();
                            });
                        }
                    }
                }
                sessions.AddOrSetSeesion(session);
            }
        }
    }

    [GeneratedRegex("<input type=\"hidden\" name=\"(.*?)\" value=\"(.*?)\" />")]
    private static partial Regex OpenIdLoginRegex();

    public async Task<CookieCollection?> OpenIdLoginAsync(string openidparams, string nonce, CookieCollection cookie)
    {
        var result = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, OpenIdloginUrl)
            {
                Content = new MultipartFormDataContent
                {
                    { new ByteArrayContent(Encoding.UTF8.GetBytes("steam_openid_login")), "action" },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes("checkid_setup")), "openid.mode" },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(openidparams)), "openidparams" },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(nonce)), "nonce" },
                },
            };

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            request.Headers.ConnectionClose = true;
            cookieContainer.Add(cookie);

            var respone = await client.SendAsync(request);

            if (respone.IsSuccessStatusCode)
            {
                var html = await respone.Content.ReadAsStringAsync();

                var matches = OpenIdLoginRegex().Matches(html);

                openidparams = matches.FirstOrDefault(f => f.Groups[1].Value == "openidparams")?.Groups[2].Value ?? string.Empty;
                nonce = matches.FirstOrDefault(f => f.Groups[1].Value == "nonce")?.Groups[2].Value ?? string.Empty;

                if (!string.IsNullOrEmpty(openidparams) && !string.IsNullOrEmpty(nonce))
                {
                    // 登录出现其它错误
                    return null;
                }

                return cookieContainer.GetAllCookies();
            }
            return null;
        });
        return result;
    }

    public async Task<(string encryptedPassword64, ulong timestamp)> GetRSAkeyV2Async(string username, string password)
    {
        var data = UrlEncoder.Default.Encode(new CAuthentication_GetPasswordRSAPublicKey_Request()
        {
            AccountName = username,
        }.ToByteString().ToBase64());

        var requestUri = new Uri("https://api.steampowered.com/IAuthenticationService/GetPasswordRSAPublicKey/v1?input_protobuf_encoded=" + data);

        var result = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var respone = await client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception($"获取 RSAKey 出现错误: {respone.StatusCode}");
            }

            var result = CAuthentication_GetPasswordRSAPublicKey_Response.Parser.ParseFrom(
                await respone.Content.ReadAsStreamAsync());
            try
            {
                // 使用 RSA 密钥加密密码
                using var rsa = new RSACryptoServiceProvider();
                var passwordBytes = Encoding.ASCII.GetBytes(password);
                var p = rsa.ExportParameters(false);
                p.Exponent = Convert.FromHexString(result.PublickeyExp);
                p.Modulus = Convert.FromHexString(result.PublickeyMod);
                rsa.ImportParameters(p);
                byte[] encryptedPassword = rsa.Encrypt(passwordBytes, false);
                var encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
                return (encryptedPassword64, result.Timestamp);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RSA 加密密码失败");
                throw;
            }
        });

        return result;
    }

    public async Task DoLoginV2Async(SteamLoginState loginState)
    {
        loginState.Success = false;

        if (loginState.ClientId != null)
        {
            if (loginState.Requires2FA || loginState.RequiresEmailAuth)
            {
                await UpdateAuthSessionWithSteamGuardAsync(loginState);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(loginState.Username) ||
                string.IsNullOrEmpty(loginState.Password))
            {
                loginState.Message = "请填写正确的 Steam 用户名密码";
                return;
            }

            // Steam 会从用户名和密码中删除所有非 ASCII 字符
            loginState.Username = SteamUNPWDRegex().Replace(loginState.Username, string.Empty);
            loginState.Password = SteamUNPWDRegex().Replace(loginState.Password, string.Empty);

            loginState.SeesionId = cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid");
            if (string.IsNullOrEmpty(loginState.SeesionId))
            {
                // 访问一次登录页获取 SessionId
                await WaitAndRetryAsync().ExecuteAsync(async () =>
                {
                    await client.GetAsync("https://store.steampowered.com/login/");
                });
                loginState.SeesionId = cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid");
            }

            var (encryptedPassword64, timestamp) = await GetRSAkeyV2Async(loginState.Username, loginState.Password);

            if (string.IsNullOrEmpty(encryptedPassword64))
            {
                loginState.Message = "登录失败，获取 RSAkey 出现错误，请重试";
                loginState.ResetStatus();
                return;
            }

            var input_protobuf_encoded = new CAuthentication_BeginAuthSessionViaCredentials_Request()
            {
                AccountName = loginState.Username,
                DeviceFriendlyName = UserAgent.Default,
                EncryptedPassword = encryptedPassword64,
                EncryptionTimestamp = timestamp,
                WebsiteId = "Community",
                PlatformType = EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser,
                RememberLogin = false,
                Persistence = ESessionPersistence.KEsessionPersistencePersistent,
            }.ToByteString().ToBase64();

            var (result, respone) = await WaitAndRetryAsync().ExecuteAsync(async () =>
            {
                var data = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", input_protobuf_encoded },
                });

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1")
                {
                    Content = data,
                };

                request.Headers.UserAgent.Clear();
                request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

                if (loginState.Cookies != null)
                    cookieContainer.Add(loginState.Cookies);

                var respone = await client.SendAsync(request);
                using var responeStream = await respone.Content.ReadAsStreamAsync();
                var result = CAuthentication_BeginAuthSessionViaCredentials_Response.Parser.ParseFrom(responeStream);
                return (result, respone);
            });

            loginState.ClientId = result.ClientId;
            loginState.SteamId = result.Steamid;
            loginState.RequestId = result.RequestId.ToByteArray();

            if (loginState.SteamId == 0)
            {
                var eResult = respone.Headers.GetValues("X-eresult").FirstOrDefault();

                loginState.Message = eResult switch
                {
                    "5" => "请核对您的密码和账户名称并重试。",
                    "20" => "与 Steam 通信时出现问题。请稍后重试。",
                    "84" => "短期内来自您所在位置的失败登录过多。请15分钟后再试。",
                    _ => $"{eResult} 登录遇到未知错误，请稍后重试。",
                };

                // 短期内来自您所在位置的失败登录过多。请稍后再试。
                // 登录遇到问题，请检查账号名或密码是否正确。
                loginState.ResetStatus();
                return;
            }

            if (result.AllowedConfirmations.Any())
            {
                if (result.AllowedConfirmations[0].ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode)
                {
                    loginState.Message = "需要2FA验证码";
                    loginState.Requires2FA = true;
                    loginState.Success = false;
                    return;
                }
                else if (result.AllowedConfirmations[0].ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode)
                {
                    await WaitAndRetryAsync().ExecuteAsync(async () =>
                    {
                        var req = new HttpRequestMessage(HttpMethod.Post, "https://login.steampowered.com/jwt/checkdevice/" + loginState.SteamId)
                        {
                            Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                            {
                                { "clientid", loginState.ClientId.ToString() },
                                { "steamid", loginState.SteamId.ToString() },
                            }),
                        };

                        req.Headers.UserAgent.Clear();
                        req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

                        (await client.SendAsync(req)).EnsureSuccessStatusCode();
                    });

                    loginState.Message = "需要邮箱验证码";
                    loginState.RequiresEmailAuth = true;
                    loginState.Success = false;
                    return;
                }
            }
        }

        var pollAuthSessionStatusResponse = await PollAuthSessionStatusAsync(loginState.ClientId!.Value, loginState.RequestId!);
        loginState.AccessToken = pollAuthSessionStatusResponse.AccessToken;
        loginState.RefreshToken = pollAuthSessionStatusResponse.RefreshToken;

        if (string.IsNullOrEmpty(pollAuthSessionStatusResponse.RefreshToken))
        {
            loginState.Message = "登录失败，请确认输入验证码是否正确。";
            loginState.ResetStatus();
            return;
        }

        var tokens = await FinalizeLoginAsync(pollAuthSessionStatusResponse.RefreshToken, loginState.SeesionId);

        if (string.IsNullOrEmpty(tokens?.SteamId))
        {
            loginState.Message = "FinalizeLoginAsync 登录失败";
            loginState.ResetStatus();
            return;
        }

        loginState.Success = true;
        loginState.RequiresCaptcha = false;
        loginState.Requires2FA = false;
        loginState.RequiresEmailAuth = false;
        loginState.Message = null;

        if (tokens.TransferInfo?.Any() == true)
        {
            foreach (var transfer in tokens.TransferInfo)
            {
                if (transfer.Url?.Contains("help.steampowered.com") == true ||
                    transfer.Url?.Contains("steam.tv") == true)
                {
                    //跳过暂时用不到的域名 节约带宽
                    continue;
                }

                await WaitAndRetryAsync().ExecuteAsync(async () =>
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, transfer.Url)
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                        {
                              { "nonce", transfer.Params?.Nonce },
                              { "auth", transfer.Params?.Auth },
                              { "steamID", loginState.SteamId.ToString() },
                        }),
                    };

                    req.Headers.UserAgent.Clear();
                    req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

                    (await client.SendAsync(req)).EnsureSuccessStatusCode();
                    //var res = data.Content.ReadAsStringAsync();
                });
            }

            //await client.GetAsync("https://store.steampowered.com/");
            //await client.GetAsync("https://steamcommunity.com/");
            loginState.Cookies = cookieContainer.GetAllCookies();
        }
        var session = new SteamSession();
        session.SteamId = loginState.SteamId.ToString();
        session.AccessToken = loginState.AccessToken;
        session.RefreshToken = loginState.RefreshToken;
        session.CookieContainer = cookieContainer;
        sessions.AddOrSetSeesion(session);
    }

    async Task<CAuthentication_PollAuthSessionStatus_Response> PollAuthSessionStatusAsync(ulong client_id, byte[] request_id)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/PollAuthSessionStatus/v1")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", new CAuthentication_PollAuthSessionStatus_Request()
                        {
                            ClientId = client_id,
                            RequestId = ByteString.CopyFrom(request_id),
                        }.ToByteString().ToBase64()
                    },
                }),
            };

            var respone = await client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception($"PollAuthSessionStatus 出现错误: {respone.StatusCode}");
            }

            var result = CAuthentication_PollAuthSessionStatus_Response.Parser.ParseFrom(await respone.Content.ReadAsStreamAsync());
            return result;
        });
        return r;
    }

    async Task<bool> UpdateAuthSessionWithSteamGuardAsync(SteamLoginState loginState)
    {
        ArgumentNullException.ThrowIfNull(loginState.ClientId);

        var input_protobuf_encoded = new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request()
        {
            ClientId = loginState.ClientId.Value,
            Steamid = loginState.SteamId,
            Code = loginState.Requires2FA ? loginState.TwofactorCode : loginState.EmailCode,
            CodeType = loginState.Requires2FA ? EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode : EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode,
        }.ToByteString().ToBase64();

        var result = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", input_protobuf_encoded },
                }),
            };

            var respone = await client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        });

        return result;
    }

    async Task<FinalizeLoginStatus> FinalizeLoginAsync(string nonce, string? sessionid)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://login.steampowered.com/jwt/finalizelogin")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "nonce", nonce },
                    { "sessionid", sessionid },
                    { "redir", "https://steamcommunity.com/login/home/?goto=" },
                }),
            };

            var respone = await client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception($"FinalizeLoginAsync 出现错误: {respone.StatusCode}");
            }

            var result = await respone.Content.ReadFromJsonAsync(FinalizeLoginStatus_.Default.FinalizeLoginStatus);

            if (result == null)
            {
                throw new Exception($"FinalizeLoginAsync 出现错误: {result}");
            }

            return result;
        });
        return r;
    }

    public async Task<(bool IsSuccess, string? Message, HistoryParseResponse? History)> GetAccountHistoryDetail(SteamLoginState loginState)
    {
        (bool IsSuccess, string? Message, HistoryParseResponse? History) result = (false, "获取 Steam 账号消费历史记录出现错误", null);

        if (loginState.Cookies == null)
        {
            return result;
        }

        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SteamStoreAccountHistoryDetailUrl);

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            cookieContainer.Add(loginState.Cookies);

            var respone = await client.SendAsync(request);

            if (respone.IsSuccessStatusCode)
            {
                var html = await respone.Content.ReadAsStringAsync();

                //var historyCursorString = Regex.Match(html, @"var g_historyCursor = (?<grp0>[^;]+);").Groups.Values.LastOrDefault()?.Value;

                //var historyCursor = JsonSerializer.Deserialize<CursorData>(historyCursorString);

                //var request1 = new HttpRequestMessage(HttpMethod.Post, SteamStoreAccountHistoryAjaxlUrl)
                //{
                //    Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                //    {
                //        { "cursor[wallet_txnid]", historyCursor.WalletTxnid },
                //        { "cursor[timestamp_newest]", historyCursor.TimestampNewest.ToString() },
                //        { "cursor[balance]", historyCursor.Balance },
                //        { "cursor[currency]", historyCursor.Currency.ToString() },
                //        { "sessionid", loginState.SessionId },
                //    }),
                //};

                //request1.Headers.UserAgent.Clear();
                //request1.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

                //var respone1 = await client.SendAsync(request1);

                // 解析表格元素
                IBrowsingContext context = BrowsingContext.New();
                IDocument document = await context.OpenAsync(request => request.Content(html));

                IElement? tbodyElement = document.QuerySelector("table>tbody");
                if (tbodyElement != null)
                {
                    var historyData = ParseHistory(tbodyElement);
                    result.History = historyData;
                    result.IsSuccess = true;
                    return result;
                }
            }
            return result;
        });
        return r;
    }

    /// <summary>
    /// 解析历史记录条目
    /// </summary>
    /// <param name="tableElement"></param>
    /// <returns></returns>
    static HistoryParseResponse ParseHistory(IElement tableElement)
    {
        Regex pattern = ParseHistoryRegex();

        // 识别货币符号
        string ParseSymbol(string symbol1, string symbol2)
        {
            const char USD = '$';
            const char RMB = '¥';

            string currency = string.Empty;

            if (!string.IsNullOrEmpty(symbol1))
            {
                if (CurrencyHelper.SymbolCurrency.ContainsKey(symbol1))
                {
                    currency = CurrencyHelper.SymbolCurrency[symbol1];
                }
            }

            if (!string.IsNullOrEmpty(symbol2))
            {
                if (CurrencyHelper.SymbolCurrency.ContainsKey(symbol2))
                {
                    currency = CurrencyHelper.SymbolCurrency[symbol2];
                }
            }

            if (string.IsNullOrEmpty(currency))
            {
                if (symbol1.Contains(USD) || symbol2.Contains(USD))
                {
                    return "USD";
                }
                else if (symbol1.Contains(RMB) || symbol2.Contains(RMB))
                {
                    // 人民币和日元符号相同, 使用钱包默认货币单位
                    return RMB.ToString();
                }
                else
                {
                    //string.Format("检测货币符号失败, 使用默认货币单位 {0}", defaultCurrency);
                    return "unknown";
                }
            }
            else
            {
                return currency;
            }
        }

        // 识别货币数值
        int ParseMoneyString(string strMoney)
        {
            Match match = pattern.Match(strMoney);

            if (!match.Success)
            {
                return 0;
            }
            else
            {
                bool negative = match.Groups[1].Value == "-";
                string symbol1 = match.Groups[2].Value.Trim();
                string strPrice = match.Groups[3].Value;
                string symbol2 = match.Groups[4].Value.Trim();

                string currency = ParseSymbol(symbol1, symbol2);

                bool useDot = CurrencyHelper.DotCurrency.Contains(currency);

                if (useDot)
                {
                    strPrice = strPrice.Replace(".", "").Replace(',', '.');
                }
                else
                {
                    strPrice = strPrice.Replace(",", "");
                }

                int price;

                if (double.TryParse(strPrice, out double fPrice))
                {
                    price = (int)fPrice * 100;
                }
                else
                {
                    strPrice = strPrice.Replace(".", "");
                    if (!int.TryParse(strPrice, out price))
                    {
                        //string.Format("解析价格 {0} 失败", match.Groups[3].Value);
                        return 0;
                    }
                }

                //if (currencyRates.ContainsKey(currency))
                //{
                //    double rate = currencyRates[currency];
                //    return (negative ? -1 : 1) * (int)(price / rate);
                //}
                //else
                //{
                //string.Format("无 {0} 货币的汇率", currency);
                return (negative ? -1 : 1) * price;
                //}
            }
        }

        HistoryParseResponse result = new();

        IHtmlCollection<IElement> rows = tableElement.QuerySelectorAll("tr");

        foreach (var row in rows)
        {
            if (!row.HasChildNodes)
            {
                continue;
            }

            IElement? whtItem = row.QuerySelector("td.wht_items");
            IElement? whtType = row.QuerySelector("td.wht_type");
            IElement? whtTotal = row.QuerySelector("td.wht_total");
            IElement? whtChange = row.QuerySelector("td.wht_wallet_change.wallet_column");

            bool isRefund = whtType?.ClassName?.Contains("wht_refunded") == true;

            string strItem = whtItem?.Text().Trim().Replace("\t", "") ?? "";
            string strType = whtType?.Text().Trim().Replace("\t", "") ?? "";
            string strTotal = whtTotal?.Text().Trim().Replace("\t", "") ?? "";
            string strChange = whtChange?.Text().Trim().Replace("\t", "") ?? "";

            if (!string.IsNullOrEmpty(strType))
            {
                // 排除退款和转换货币
                if (!string.IsNullOrEmpty(strType) && !strType.StartsWith("转换") && !strType.StartsWith("退款"))
                {
                    int total = ParseMoneyString(strTotal);
                    int walletChange;

                    if (string.IsNullOrEmpty(strChange))
                    {
                        walletChange = 0;
                    }
                    else
                    {
                        walletChange = Math.Abs(ParseMoneyString(strChange));
                    }

                    if (total == 0)
                    {
                        continue;
                    }

                    if (strType.StartsWith("购买"))
                    {
                        if (!strItem.Contains("钱包资金"))
                        {
                            if (!isRefund)
                            {
                                result.StorePurchase += total;
                                result.StorePurchaseWallet += walletChange;
                            }
                            else
                            {
                                result.RefundPurchase += total;
                                result.RefundPurchaseWallet += walletChange;
                            }
                        }
                        else
                        {
                            result.WalletPurchase += total;
                        }
                    }
                    else if (strType.StartsWith("礼物购买"))
                    {
                        if (!isRefund)
                        {
                            result.GiftPurchase += total;
                            result.GiftPurchaseWallet += walletChange;
                        }
                        else
                        {
                            result.RefundPurchase += total;
                            result.RefundPurchaseWallet += walletChange;
                        }
                    }
                    else if (strType.StartsWith("游戏内购买"))
                    {
                        if (!isRefund)
                        {
                            result.InGamePurchase += walletChange;
                        }
                        else
                        {
                            result.RefundPurchase += walletChange;
                            result.RefundPurchaseWallet += walletChange;
                        }
                    }
                    else if (strType.StartsWith("市场交易") || strType.Contains("市场交易"))
                    {
                        if (!isRefund)
                        {
                            if (walletChange >= 0)
                            {
                                result.MarketSelling += total;
                            }
                            else
                            {
                                result.MarketPurchase += total;
                            }
                        }
                        else
                        {
                            result.RefundPurchase += walletChange;
                        }
                    }
                    else
                    {
                        if (!isRefund)
                        {
                            result.Other += total;
                        }
                    }
                }
            }
        }

        return result;
    }

    [GeneratedRegex("^\\s*([-+])?([^\\d,.]*)([\\d,.]+)([^\\d,.]*)$")]
    private static partial Regex ParseHistoryRegex();

    [GeneratedRegex("<a class=\"global_action_link\" id=\"header_wallet_balance\" href=\"https://store.steampowered.com/account/store_transactions/\">(.*?)</a>")]
    private static partial Regex HeaderWalletBalancegRegex();

    [GeneratedRegex("<div class=\"accountData price\">(.*?)</div>")]
    private static partial Regex AccountDataRegex();

    [GeneratedRegex("国家/地区：[\\s]+<span class=\"account_data_field\">(?<grp0>[\\w]+?)</span>")]
    private static partial Regex CountryRegionRegex();

    [GeneratedRegex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])")]
    private static partial Regex EmailRegex();

    public async Task<bool> GetWalletBalance(SteamLoginState loginState)
    {
        if (loginState.Cookies == null)
        {
            return false;
        }
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SteamStoreAccountlUrl);

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            cookieContainer.Add(loginState.Cookies);

            var respone = await client.SendAsync(request);

            if (respone.IsSuccessStatusCode)
            {
                var html = await respone.Content.ReadAsStringAsync();
                var walletBalanceString = HeaderWalletBalancegRegex().Match(html).Groups.Values.LastOrDefault()?.Value;

                // 不包含历史消费记录账号大概率疑似未定区账号
                if (string.IsNullOrEmpty(walletBalanceString))
                {
                    loginState.IsUndeterminedArea = true;
                    walletBalanceString = AccountDataRegex().Match(html).Groups.Values.LastOrDefault()?.Value;
                }

                var steamCountry = CountryRegionRegex().Match(html).Groups.Values.LastOrDefault()?.Value;

                var email = EmailRegex().Match(html).Groups.Values.LastOrDefault()?.Value;

                loginState.WalletBalanceString = walletBalanceString;
                loginState.SteamCountry = string.IsNullOrWhiteSpace(steamCountry) ? null : steamCountry;
                loginState.Email = email;

                // 获取货币符号
                loginState.CurrencySymbol = walletBalanceString?.Split(' ').FirstOrDefault();

                return true;
            }
            return false;
        });
        return r;
    }

    async Task<(SteamResult Result, PurchaseResultDetail? Detail)?> RedeemWalletCodeCore(SteamLoginState loginState, string walletCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, SteamStoreRedeemWalletCodelUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
            {
                { "wallet_code", walletCode },
                { "sessionid", cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid") },
            }),
        };

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
        cookieContainer.Add(loginState.Cookies!);

        var respone = await client.SendAsync(request);

        if (respone.IsSuccessStatusCode)
        {
            var detail = await respone.Content.ReadFromJsonAsync(RedeemWalletResponse_.Default.RedeemWalletResponse);

            if (detail == null)
                return null;

            if ((detail.Result != SteamResult.OK) || (detail.Detail != PurchaseResultDetail.NoDetail))
            {
                return (detail.Result == SteamResult.OK ? SteamResult.Fail : detail.Result, detail.Detail);
            }
        }
        return (SteamResult.Fail, PurchaseResultDetail.NoDetail);
    }

    public async Task<(SteamResult Result, PurchaseResultDetail? Detail)?> RedeemWalletCode(SteamLoginState loginState, string walletCode, bool isRetry = false)
    {
        if (loginState.Cookies == null)
        {
            return null;
        }

        if (isRetry)
        {
            var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
            {
                return await RedeemWalletCodeCore(loginState, walletCode);
            });
            return r;
        }
        else
        {
            return await RedeemWalletCodeCore(loginState, walletCode);
        }
    }

    public async Task<bool> SetSteamAccountCountry(SteamLoginState loginState, string? currencyCode)
    {
        if (loginState.Cookies == null)
        {
            return false;
        }

        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, SteamStoreAccountSetCountryUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "cc", currencyCode },
                    { "sessionid", cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid") },
                }),
            };

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            cookieContainer.Add(loginState.Cookies);

            var respone = await client.SendAsync(request);

            if (respone.IsSuccessStatusCode)
            {
                var html = await respone.Content.ReadAsStringAsync();

                if (bool.TryParse(html, out bool isOk) && isOk)
                {
                    return true;
                }
            }
            return false;

        });
        return r;
    }

    [GeneratedRegex("<div class=\"currency_change_option btnv6_grey_black\" data-country=\"(?<grp0>[^\"]+)\" >[\\s]+<span>[\\s]+<div class=\"country\">(?<grp1>[\\w]+?)</div>")]
    private static partial Regex CountryItemRegex();

    public async Task<List<CurrencyData>?> GetSteamAccountCountryCodes(SteamLoginState loginState)
    {
        if (loginState.Cookies == null)
        {
            return null;
        }

        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SteamStoreAddFundsUrl);

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            cookieContainer.Add(loginState.Cookies);

            var respone = await client.SendAsync(request);

            if (respone.IsSuccessStatusCode)
            {
                var html = await respone.Content.ReadAsStringAsync();

                var matchCollection = CountryItemRegex().Matches(html);

                if (matchCollection.Any())
                {
                    var data = new List<CurrencyData>();
                    foreach (var m in matchCollection.Cast<Match>())
                    {
                        data.Add(new CurrencyData()
                        {
                            CurrencyCode = m.Groups[1].Value,
                            Display = m.Groups[2].Value,
                        });
                    }
                    return data;
                }
            }
            return null;
        });
        return r;
    }

    public async Task<string?> GetCaptchaImageBase64(string captchaId)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var url = CaptchaImageUrl + captchaId;
            var response = await client.GetByteArrayAsync(url);
            return Convert.ToBase64String(response);
        });
        return r;
    }

    public async Task<InventoryTradeHistoryRenderPageResponse> GetInventoryTradeHistory(SteamLoginState loginState, int[]? appFilter = null, InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null)
    {
        StringBuilder urlBuilder = new StringBuilder($"{STEAM_COMMUNITY_URL}/profiles/{loginState.SteamId}/inventoryhistory/?ajax=1&sessionid={loginState.SeesionId}");

        if (cursor != null)
        {
            urlBuilder.Append($"&cursor%5Btime%5D={cursor.Value.Time}&cursor%5Btime_frac%5D={cursor.Value.TimeFrac}&cursor%5Bs%5D={cursor.Value.S}");
        }

        if (appFilter != null && appFilter.Any())
        {
            foreach (var app in appFilter)
            {
                urlBuilder.Append($"&app%5B%5D={app}");
            }
        }
        cookieContainer.Add(loginState.Cookies!);
        cookieContainer.Add(new Cookie()
        {
            Name = "Steam_Language",
            Value = loginState.Language ?? "schinese",
            Domain = new Uri(STEAM_COMMUNITY_URL).Host,
            Secure = true,
            Path = "/",
        });

        cookieContainer.Add(new Cookie()
        {
            Name = "timezoneOffset",
            Value = Uri.EscapeDataString($"{TimeSpan.FromHours(8).TotalSeconds},0"),
            Domain = new Uri(STEAM_COMMUNITY_URL).Host
        });

        var resp = await client.GetAsync(urlBuilder.ToString());

        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<InventoryTradeHistoryRenderPageResponse>();

        return result;
    }

    public async IAsyncEnumerable<InventoryTradeHistoryRow> ParseInventoryTradeHistory(string html, CultureInfo? cultureInfo = null)
    {
        IBrowsingContext context = BrowsingContext.New();

        var htmlParser = context.GetService<IHtmlParser>();

        if (htmlParser == null)
            throw new ArgumentNullException("获取Html解析器失败");

        var document = await htmlParser.ParseDocumentAsync(html);

        var rowElements = document.QuerySelectorAll("div.tradehistoryrow");

        cultureInfo ??= new CultureInfo("zh");

        foreach (var rowElement in rowElements)
        {
            var (date, timeOfDate) = await ParseDateTimePart(rowElement);

            var (desc, groups) = await ParseContentPart(rowElement);

            yield return new InventoryTradeHistoryRow()
            {
                Date = date,
                TimeOfDate = timeOfDate,
                Desc = desc,
                Groups = groups
            };
        }

        Task<(DateTime date, TimeSpan timeOfDate)> ParseDateTimePart(IElement rowElement)
        {
            DateTime date = default;
            TimeSpan time = default;

            var timeElement = rowElement.QuerySelector("div.tradehistory_timestamp");

            if (!DateTime.TryParse(timeElement?.TextContent?.Trim(), cultureInfo, out var timeDate))
            {
                throw new ArgumentException("日期转换失败!");
            }

            time = timeDate.TimeOfDay;

            var dateElement = timeElement.ParentElement;
            if (dateElement != null)
            {
                dateElement.RemoveElement(timeElement);

                string? dateText = dateElement.TextContent?.Trim();

                if (!DateTime.TryParse(dateText?.Trim(), CultureInfo.GetCultureInfo("zh-CN"), out date))
                {
                    throw new ArgumentException("日期转换失败!");
                }
            }

            return Task.FromResult((date, time));
        }

        Task<(string desc, IEnumerable<InventoryTradeHistoryGroup> groups)> ParseContentPart(IElement rowElement)
        {
            var emptyResult = Task.FromResult((string.Empty, Enumerable.Empty<InventoryTradeHistoryGroup>()));

            var contentElement = rowElement.QuerySelector("div.tradehistory_content");

            if (contentElement == null || !contentElement.HasChildNodes)
                return emptyResult;

            string desc = contentElement.QuerySelector("div.tradehistory_event_description")?.TextContent ?? string.Empty;

            var items = contentElement.QuerySelectorAll("div.tradehistory_items");

            if (items == null || !items.Any())
            {
                return emptyResult;
            }

            List<InventoryTradeHistoryGroup> groups = new(items.Length);

            foreach (var item in items)
            {
                string plusminus = item.QuerySelector("div.tradehistory_items_plusminus")?.TextContent ?? string.Empty;

                var itemGroupElement = item.QuerySelector("div.tradehistory_items_group");

                if (itemGroupElement == null || !itemGroupElement.HasChildNodes)
                    continue;

                List<InventoryTradeHistoryItem> groupItems = new(itemGroupElement.ChildElementCount);

                foreach (var groupItem in itemGroupElement.Children)
                {
                    if (groupItem == null)
                        continue;

                    bool previewInInventoryPage = string.Equals(groupItem.TagName, "a", StringComparison.OrdinalIgnoreCase);

                    InventoryTradeHistoryItem rowItem = new()
                    {
                        Amount = groupItem.GetAttribute("data-amount") ?? string.Empty,
                        AppId = groupItem.GetAttribute("data-appid") ?? string.Empty,
                        ContextId = groupItem.GetAttribute("data-contextid") ?? string.Empty,
                        InstanceId = groupItem.GetAttribute("data-instanceid") ?? string.Empty,
                        ClassId = groupItem.GetAttribute("data-classid") ?? string.Empty,
                        ProfilePreviewPageUrl = previewInInventoryPage ? groupItem.GetAttribute("href") ?? string.Empty : string.Empty,
                    };

                    if (groupItem.HasChildNodes)
                    {
                        rowItem.ItemImgUrl = groupItem.FirstElementChild?.GetAttribute("src") ?? string.Empty;
                        rowItem.ItemName = groupItem.LastElementChild?.TextContent?.Trim() ?? string.Empty;
                    }

                    groupItems.Add(rowItem);
                }

                groups.Add(new()
                {
                    PlusMinus = plusminus,
                    Items = groupItems
                });
            }

            return Task.FromResult<(string, IEnumerable<InventoryTradeHistoryGroup>)>((desc, groups));
        }
    }

    public async Task<InventoryPageResponse> GetInventories(ulong steamId, string appId, string contextId, int count = 100, string? startAssetId = null, string language = "schinese")
    {
        string url = string.Format("{0}/inventory/{1}/{2}/{3}?l={4}&count={5}{6}",
            STEAM_COMMUNITY_URL,
            steamId,
            appId,
            contextId,
            language,
            count,
            !string.IsNullOrEmpty(startAssetId) ? $"&start_assetid={startAssetId}" : string.Empty);

        InventoryPageResponse inventories = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var resp = await client.GetAsync(url);

            return await resp.Content.ReadFromJsonAsync<InventoryPageResponse>();
        });

        return inventories;
    }

    public async Task<string?> GetApiKey(SteamLoginState loginState)
    {
        const string url = "https://steamcommunity.com/dev/apikey";

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
        cookieContainer.Add(loginState.Cookies!);

        var resp = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var resp = await client.SendAsync(request);

            return resp.Content;
        });

        return await ParseApiKeyFromHttpContent(resp);
    }

    public async Task<string?> RegisterApiKey(SteamLoginState loginState, string? domain = null)
    {
        if (string.IsNullOrEmpty(loginState.SeesionId))
            throw new ArgumentException(nameof(loginState.SeesionId));

        const string url = "https://steamcommunity.com/dev/registerkey";

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            { "domain", domain ?? Guid.NewGuid().ToString("N") },
            { "agreeToTerms", "agreed" },
            { "sessionid", loginState.SeesionId },
            { "Submit", "注册" },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(data)
        };

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());

        request.Headers.Add("origin", "https://steamcommunity.com");
        request.Headers.Add("authority", "steamcommunity.com");
        request.Headers.Add("referer", "https://steamcommunity.com/dev/revokekey");
        request.Headers.Add("accept-language", "zh-CN");

        cookieContainer.Add(loginState.Cookies!);
        cookieContainer.Add(new Cookie()
        {
            Name = "sessionid",
            Value = loginState.SeesionId,
            Domain = new Uri(STEAM_COMMUNITY_URL).Host,
            Secure = true,
            Path = "/"
        });

        var resp = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var resp = await client.SendAsync(request);

            return resp.Content;
        });

        string xx = await resp.ReadAsStringAsync();

        return await ParseApiKeyFromHttpContent(resp);
    }

    public async Task<IEnumerable<SendGiftHisotryItem>> GetSendGiftHisotries(SteamLoginState loginState)
    {
        cookieContainer.Add(loginState.Cookies!);

        var resp = await client.GetAsync("https://steamcommunity.com/gifts/0/history/");

        resp.EnsureSuccessStatusCode();

        using Stream respStream = await resp.Content.ReadAsStreamAsync();

        IBrowsingContext context = BrowsingContext.New();

        var htmlParser = context.GetService<IHtmlParser>();

        if (htmlParser == null)
            throw new ArgumentNullException("获取Html解析器失败");

        var document = await htmlParser.ParseDocumentAsync(respStream);

        List<SendGiftHisotryItem> result = new();

        var rowElements = document.QuerySelectorAll("div.sent_gift");

        if (rowElements != null && rowElements.Any())
        {
            foreach (var rowElement in rowElements)
            {
                var giftItemElement = rowElement.QuerySelector("div.gift_item");
                var giftStatusElement = rowElement.QuerySelector("div.gift_status_area");

                SendGiftHisotryItem giftHisotryItem = new SendGiftHisotryItem()
                {
                    Name = giftItemElement?.QuerySelector("div.gift_item_details > b")?.TextContent?.Trim() ?? string.Empty,
                    ImgUrl = giftItemElement?.QuerySelector("div.item_icon > img")?.GetAttribute("src") ?? string.Empty,
                    RedeemedGiftStatusText = giftItemElement?.QuerySelector("div.sent_gift_actions > span")?.TextContent?.Trim() ?? string.Empty,
                    GiftStatusText = giftStatusElement?.TextContent?.Trim() ?? string.Empty
                };

                result.Add(giftHisotryItem);
            }
        }

        return result;
    }

    public async IAsyncEnumerable<LoginHistoryItem>? GetLoginHistory(SteamLoginState loginState)
    {
        const string url = "https://help.steampowered.com/zh-cn/accountdata/SteamLoginHistory";

        cookieContainer.Add(loginState.Cookies!);

        cookieContainer.Add(new Cookie("sessionid", loginState.SeesionId, "/", $".{new Uri(url).Host}"));
        cookieContainer.Add(new Cookie("steamLoginSecure", cookieContainer.GetCookieValue(new Uri(STEAM_COMMUNITY_URL), "steamLoginSecure"), "/", $".{new Uri(url).Host}"));

        var resp = await client.GetAsync(url);

        resp.EnsureSuccessStatusCode();

        using var respStream = await resp.Content.ReadAsStreamAsync();

        var result = await ParseHtmlResponseStream(respStream, (doc) => Task.FromResult(doc));

        var tableElement = result.QuerySelector("table");

        if (tableElement == null || !tableElement.HasChildNodes)
        {
            yield break;
        }

        await foreach (var item in HtmlParseHelper.ParseSimpleTable(tableElement, false, ParseLoginHistoryRow))
        {
            yield return item;
        }

        ValueTask<LoginHistoryItem> ParseLoginHistoryRow(IElement trElement)
        {
            return ValueTask.FromResult(new LoginHistoryItem()
            {
                LogInDateTime = trElement.Children[0].TextContent?.Trim() ?? string.Empty,
                LogOutDateTime = trElement.Children[1].TextContent?.Trim() ?? string.Empty,
                OsType = int.TryParse(trElement.Children[2].TextContent, out int parsedOsType) ? parsedOsType : -1,
                CountryOrRegion = trElement.Children[3].TextContent?.Trim() ?? string.Empty,
                City = trElement.Children[4].TextContent?.Trim() ?? string.Empty,
                State = trElement.Children[5].TextContent?.Trim() ?? string.Empty,
            });
        }
    }

    /// <summary>
    /// 解析开发密钥信息
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private async Task<string?> ParseApiKeyFromHttpContent(HttpContent content)
    {
        const string selector = "#bodyContents_ex > p";
        const string revokeKeyUrl = "https://steamcommunity.com/dev/revokekey";

        using Stream respStream = await content.ReadAsStreamAsync();

        IBrowsingContext context = BrowsingContext.New();

        var htmlParser = context.GetService<IHtmlParser>();

        if (htmlParser == null)
            throw new ArgumentNullException("获取Html解析器失败");

        var document = htmlParser.ParseDocument(respStream);

        var form = document.QuerySelector("#editForm");
        if (form == null)
        {
            return string.Empty;
        }

        if (form.GetAttribute("action") != revokeKeyUrl)
        {
            return string.Empty;
        }

        var paramList = document.QuerySelectorAll(selector);

        if (paramList == null || !paramList.Any())
            return string.Empty;

        string keyParam = paramList.First().TextContent.Trim().Replace(" ", string.Empty);

        return keyParam[(keyParam.LastIndexOf(":") + 1)..];
    }

    private async Task<T> ParseHtmlResponseStream<T>(Stream stream, Func<IDocument, Task<T>> parseFunc)
    {
        using (stream)
        {
            IBrowsingContext context = BrowsingContext.New();

            var htmlParser = context.GetService<IHtmlParser>();

            if (htmlParser == null)
                throw new ArgumentNullException("获取Html解析器失败");

            var document = await htmlParser.ParseDocumentAsync(stream);

            return await parseFunc(document);
        }
    }

    public async Task<bool> CheckAccessTokenValidation(string accesstoken)
    {
        var rsp = await client.GetAsync(string.Format(AccountGetSteamNotificationsUrl, accesstoken));

        if (rsp.IsSuccessStatusCode)
        {
            return true;
        }

        return false;
    }

    public async Task<bool?> CheckAccountPhoneStatus(string accessToken)
    {
        var resp = await client
            .PostAsync(string.Format(STEAM_AUTHENTICATOR_ACCOUNTPHONESTATUS, accessToken), new StringContent(""));

        if (resp.IsSuccessStatusCode)
        {
            var json = await resp.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);

            /*
             * {
                    "response": {
                        verified_phone": false
                    }
               }
             */

            return doc.RootElement.TryGetProperty("response", out var jsonResp) &&
                jsonResp.TryGetProperty("verified_phone", out var prop) &&
                prop.GetBoolean();
        }

        return null;
    }
}