using AngleSharp;
using AngleSharp.Dom;
using BD.SteamClient.Enums;
using BD.SteamClient.Heleprs;
using BD.SteamClient.Models;
using Google.Protobuf;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using HttpMethod = System.Net.Http.HttpMethod;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.SteamClient.Services;

public sealed class SteamAccountServiceImpl : HttpClientUseCookiesWithProxyServiceImpl, ISteamAccountService
{
    public SteamAccountServiceImpl(IServiceProvider s)
    {
    }

    /// <summary>
    /// 获取Steam登录所需的RSAkey来加密password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<(string encryptedPassword64, string timestamp)> GetRSAkeyAsync(string username, string password)
    {
        var data = new Dictionary<string, string>()
        {
            { "donotache", ISteamAccountService.default_donotache },
            { "username", username },
        };
        var request = new HttpRequestMessage(HttpMethod.Post, ISteamAccountService.Urls.GetRSAkeyUrl)
        {
            Content = new FormUrlEncodedContent(data),
        };
        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
        var result = await client.SendAsync(request);

        if (!result.IsSuccessStatusCode)
            throw new Exception($"获取RSAKey出现错误: {result.StatusCode}");

        var jsonObj = await result.Content.ReadFromJsonAsync<JsonObject>();

        if (jsonObj == null)
            throw new Exception($"获取RSAKey出现错误: 无效的{nameof(jsonObj)}");

        var success = jsonObj["success"]?.GetValue<bool>();
        var publickey_exp = jsonObj["publickey_exp"]?.GetValue<string>();
        var publickey_mod = jsonObj["publickey_mod"]?.GetValue<string>();
        var timestamp = jsonObj["timestamp"]?.GetValue<string>();
        if (!(success.HasValue && success.Value) ||
            string.IsNullOrEmpty(publickey_exp) ||
            string.IsNullOrEmpty(publickey_mod) ||
            string.IsNullOrEmpty(timestamp))
            throw new Exception($"获取RSAKey出现错误: " + jsonObj.ToJsonString());

        // 使用 RSA 密钥加密密码
        using (var rsa = new RSACryptoServiceProvider())
        {
            var passwordBytes = Encoding.ASCII.GetBytes(password);
            var p = rsa.ExportParameters(false);
            p.Exponent = Convert.FromHexString(publickey_exp);
            p.Modulus = Convert.FromHexString(publickey_mod);
            rsa.ImportParameters(p);
            byte[] encryptedPassword = rsa.Encrypt(passwordBytes, false);
            var encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
            return (encryptedPassword64, timestamp);
        }
    }

    /// <summary>
    /// 执行请求Steam登录，写入登录状态
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <exception cref="Exception"></exception>r
    public async Task DoLoginAsync(SteamLoginResponse loginState, bool isTransfer = false, bool isDownloadCaptchaImage = false)
    {
        loginState.Success = false;

        if (string.IsNullOrEmpty(loginState.Username) ||
            string.IsNullOrEmpty(loginState.Password))
        {
            loginState.Message = "请填写正确的 Steam 用户名密码";
            return;
        }

        // Steam 会从用户名和密码中删除所有非 ASCII 字符
        loginState.Username = Regex.Replace(loginState.Username, @"[^\u0000-\u007F]", string.Empty);
        loginState.Password = Regex.Replace(loginState.Password, @"[^\u0000-\u007F]", string.Empty);

        if (string.IsNullOrEmpty(cookieContainer.GetCookieValue(new Uri(ISteamAccountService.Urls.SteamStore), "sessionid")))
        {
            //访问一次登录页获取SessionId
            await client.GetAsync(ISteamAccountService.Urls.SteamLoginUrl);
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
            { "donotache", ISteamAccountService.default_donotache },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ISteamAccountService.Urls.DologinUrl)
        {
            Content = new FormUrlEncodedContent(data),
        };

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);

        if (loginState.Cookies != null)
            cookieContainer.Add(loginState.Cookies);

        var respone = await client.SendAsync(request);

        if (respone.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = respone.Headers.RetryAfter?.ToString();
            loginState.Message = $"{HttpStatusCode.TooManyRequests} 请求过于频繁，请稍后再试，{retryAfter}";
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
            loginState.Message = $"登录错误: 无效的{nameof(jsonObj)}";
            loginState.Success = false;
            try
            {
                var errorStr = await respone.Content.ReadAsStringAsync();
                throw new Exception($"登录异常:{errorStr} StatusCode:{respone.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"登录异常 StatusCode:{respone.StatusCode}", ex);
            }
        }

        var message = jsonObj["message"]?.GetValue<string>();

        //var success = jsonObj["success"]?.GetValue<bool>();
        //loginState.Success = success.HasValue && success.Value;
        //if (loginState.Success)
        //{
        //    throw new Exception($"doLogin出现错误: 无效的用户名");
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
                loginState.CaptchaUrl = ISteamAccountService.Urls.CaptchaImageUrl + loginState.CaptchaId;
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
            //    throw new Exception($"doLogin出现错误: Invalid response from Steam (No OAuth token)");
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
                loginState.Cookies = cookieContainer.GetAllCookies();
                //loginState.Cookies.Add(new Cookie("sessionid", "", "/", "steamcommunity.com"));
                if (doLoginRespone.TransferParameters != null && doLoginRespone.TransferUrls != null)
                {
                    loginState.SteamIdString = doLoginRespone.TransferParameters.Steamid;

                    _ = ulong.TryParse(loginState.SteamIdString, out var steamid);
                    loginState.SteamId = steamid;

                    if (isTransfer)
                    {
                        foreach (var url in doLoginRespone.TransferUrls)
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
                            req.Headers.UserAgent.ParseAdd(UserAgent.Default);

                            await client.SendAsync(req);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 执行Steam第三方快速登录接口请求并返回登录后Cookie
    /// </summary>
    /// <param name="openidparams">请求参数</param>
    /// <param name="nonce">nonce</param>
    /// <param name="cookie">登录成功状态</param>
    /// <returns></returns>
    public async Task<CookieCollection?> OpenIdLoginAsync(string openidparams, string nonce, CookieCollection cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ISteamAccountService.Urls.OpenIdloginUrl)
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
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
        cookieContainer.Add(cookie);

        var respone = await client.SendAsync(request);

        if (respone.IsSuccessStatusCode)
        {
            var html = await respone.Content.ReadAsStringAsync();

            var regexPattern = @"<input type=""hidden"" name=""(.*?)"" value=""(.*?)"" />";
            var matches = Regex.Matches(html, regexPattern);

            openidparams = matches.FirstOrDefault(f => f.Groups[1].Value == "openidparams")?.Groups[2].Value ?? string.Empty;
            nonce = matches.FirstOrDefault(f => f.Groups[1].Value == "nonce")?.Groups[2].Value ?? string.Empty;

            if (!string.IsNullOrEmpty(openidparams) && !string.IsNullOrEmpty(nonce))
            {
                //登录出现其它错误
                return null;
            }

            return cookieContainer.GetAllCookies();
        }
        return null;
    }

    /// <summary>
    /// 获取Steam登录所需的RSAkey来加密password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<(string encryptedPassword64, ulong timestamp)> GetRSAkeyV2Async(string username, string password)
    {
        var data = UrlEncoder.Default.Encode(new CAuthentication_GetPasswordRSAPublicKey_Request()
        {
            AccountName = username,
        }.ToByteString().ToBase64());

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.steampowered.com/IAuthenticationService/GetPasswordRSAPublicKey/v1?input_protobuf_encoded=" + data);

        var respone = await client.SendAsync(request);

        if (!respone.IsSuccessStatusCode)
        {
            throw new Exception($"获取RSAKey出现错误: {respone.StatusCode}");
        }

        var result = CAuthentication_GetPasswordRSAPublicKey_Response.Parser.ParseFrom(
            await respone.Content.ReadAsStreamAsync());

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

    public async Task DoLoginV2Async(SteamLoginResponse loginState)
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
            loginState.Username = Regex.Replace(loginState.Username, @"[^\u0000-\u007F]", string.Empty);
            loginState.Password = Regex.Replace(loginState.Password, @"[^\u0000-\u007F]", string.Empty);

            loginState.SeesionId = cookieContainer.GetCookieValue(new Uri(ISteamAccountService.Urls.SteamStore), "sessionid");
            if (string.IsNullOrEmpty(loginState.SeesionId))
            {
                //访问一次登录页获取SessionId
                await client.GetAsync("https://store.steampowered.com/login/");
                loginState.SeesionId = cookieContainer.GetCookieValue(new Uri(ISteamAccountService.Urls.SteamStore), "sessionid");
            }

            var (encryptedPassword64, timestamp) = await GetRSAkeyV2Async(loginState.Username, loginState.Password);

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
            var data = new FormUrlEncodedContent(new Dictionary<string, string?>()
            {
                { "input_protobuf_encoded", input_protobuf_encoded },
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1")
            {
                Content = data,
            };

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(UserAgent.Default);

            if (loginState.Cookies != null)
                cookieContainer.Add(loginState.Cookies);

            var respone = await client.SendAsync(request);
            using var responeStream = await respone.Content.ReadAsStreamAsync();
            var result = CAuthentication_BeginAuthSessionViaCredentials_Response.Parser.ParseFrom(responeStream);

            loginState.ClientId = result.ClientId;
            loginState.SteamId = result.Steamid;
            loginState.RequestId = result.RequestId.ToByteArray();

            if (loginState.SteamId == 0)
            {
                var eResult = respone.Headers.GetValues("X-eresult").FirstOrDefault();

                loginState.Message = eResult switch
                {
                    "5" => "请核对您的密码和帐户名称并重试。",
                    "20" => "与 Steam 通信时出现问题。请稍后重试。",
                    "84" => "短期内来自您所在位置的失败登录过多。请15分钟后再试。",
                    _ => $"{eResult} 登录遇到未知错误，请稍后重试。",
                };

                //短期内来自您所在位置的失败登录过多。请稍后再试。
                //登录遇到问题，请检查账号名或密码是否正确。
                loginState.Requires2FA = false;
                loginState.RequiresCaptcha = false;
                loginState.RequiresEmailAuth = false;
                loginState.Success = false;
                loginState.ClientId = null;
                loginState.RequestId = null;
                loginState.SeesionId = null;
                return;
            }

            if (result.AllowedConfirmations.Any())
            {
                if (result.AllowedConfirmations[0].ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode)
                {
                    loginState.Message = "需要手机验证码";
                    loginState.Requires2FA = true;
                    loginState.Success = false;
                    return;
                }
                else if (result.AllowedConfirmations[0].ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode)
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
                    req.Headers.UserAgent.ParseAdd(UserAgent.Default);

                    await client.SendAsync(req);

                    loginState.Message = "需要邮箱验证码";
                    loginState.RequiresEmailAuth = true;
                    loginState.Success = false;
                    return;
                }
            }
        }

        var refresh_token = await PollAuthSessionStatusAsync(loginState.ClientId.Value, loginState.RequestId);

        if (string.IsNullOrEmpty(refresh_token))
        {
            loginState.Message = "登录失败，请确认令牌是否正确。";
            loginState.Requires2FA = false;
            loginState.RequiresCaptcha = false;
            loginState.RequiresEmailAuth = false;
            loginState.Success = false;
            loginState.ClientId = null;
            loginState.RequestId = null;
            loginState.SeesionId = null;
            return;
        }

        var tokens = await FinalizeLoginAsync(refresh_token, loginState.SeesionId);

        if (string.IsNullOrEmpty(tokens?.SteamId))
        {
            loginState.Message = "FinalizeLoginAsync 登录失败";
            loginState.Requires2FA = false;
            loginState.RequiresCaptcha = false;
            loginState.RequiresEmailAuth = false;
            loginState.Success = false;
            loginState.ClientId = null;
            loginState.RequestId = null;
            loginState.SeesionId = null;
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
                req.Headers.UserAgent.ParseAdd(UserAgent.Default);

                await client.SendAsync(req);
                //var res = data.Content.ReadAsStringAsync();
            }

            //await client.GetAsync("https://store.steampowered.com/");
            //await client.GetAsync("https://steamcommunity.com/");
            loginState.Cookies = cookieContainer.GetAllCookies();
        }
    }

    private async Task<string> PollAuthSessionStatusAsync(ulong client_id, byte[] request_id)
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
            throw new Exception($"PollAuthSessionStatus出现错误: {respone.StatusCode}");
        }

        var result = CAuthentication_PollAuthSessionStatus_Response.Parser.ParseFrom(await respone.Content.ReadAsStreamAsync());

        return result.RefreshToken;
    }

    private async Task<bool> UpdateAuthSessionWithSteamGuardAsync(SteamLoginResponse loginState)
    {
        ArgumentNullException.ThrowIfNull(loginState.ClientId);

        var input_protobuf_encoded = new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request()
        {
            ClientId = loginState.ClientId.Value,
            Steamid = loginState.SteamId,
            Code = loginState.Requires2FA ? loginState.TwofactorCode : loginState.EmailCode,
            CodeType = loginState.Requires2FA ? EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode : EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode,
        }.ToByteString().ToBase64();

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
    }

    private async Task<FinalizeLoginStatus> FinalizeLoginAsync(string nonce, string? sessionid)
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
            throw new Exception($"FinalizeLoginAsync出现错误: {respone.StatusCode}");
        }

        var result = await respone.Content.ReadFromJsonAsync(FinalizeLoginStatus_.Default.FinalizeLoginStatus);

        if (result == null)
        {
            throw new Exception($"FinalizeLoginAsync出现错误: {result}");
        }

        return result;
    }

    /// <summary>
    /// 获取Steam账号消费历史记录
    /// </summary>
    public async Task<(bool IsSuccess, string? Message, HistoryParseResponse? History)> GetAccountHistoryDetail(SteamLoginResponse loginState)
    {
        (bool IsSuccess, string? Message, HistoryParseResponse? History) result = (false, "获取Steam账号消费历史记录出现错误", null);

        if (loginState.Cookies == null)
        {
            return result;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, ISteamAccountService.Urls.SteamStoreAccountHistoryDetailUrl);

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
        cookieContainer.Add(loginState.Cookies);

        var respone = await client.SendAsync(request);

        if (respone.IsSuccessStatusCode)
        {
            var html = await respone.Content.ReadAsStringAsync();

            //var historyCursorString = Regex.Match(html, @"var g_historyCursor = (?<grp0>[^;]+);").Groups.Values.LastOrDefault()?.Value;

            //var historyCursor = JsonSerializer.Deserialize<CursorData>(historyCursorString);

            //var request1 = new HttpRequestMessage(HttpMethod.Post, Urls.SteamStoreAccountHistoryAjaxlUrl)
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
            //request1.Headers.UserAgent.ParseAdd(UserAgent.Default);

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

        /// <summary>
        /// 解析历史记录条目
        /// </summary>
        /// <param name="tableElement"></param>
        /// <param name="currencyRates"></param>
        /// <param name="defaultCurrency"></param>
        /// <returns></returns>
        static HistoryParseResponse ParseHistory(IElement tableElement)
        {
            Regex pattern = new(@"^\s*([-+])?([^\d,.]*)([\d,.]+)([^\d,.]*)$");

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
                        //人民币和日元符号相同, 使用钱包默认货币单位
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
    }

    /// <summary>
    /// 获取Steam账号余额货币类型及账号区域信息,并检测账号是否定区
    /// </summary>
    public async Task<bool> GetWalletBalance(SteamLoginResponse loginState)
    {
        if (loginState.Cookies == null)
        {
            return false;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, ISteamAccountService.Urls.SteamStoreAccountlUrl);

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
        cookieContainer.Add(loginState.Cookies);

        var respone = await client.SendAsync(request);

        if (respone.IsSuccessStatusCode)
        {
            var html = await respone.Content.ReadAsStringAsync();

            var walletBalanceString = Regex.Match(html, @"<a class=""global_action_link"" id=""header_wallet_balance"" href=""https://store.steampowered.com/account/store_transactions/"">(.*?)</a>").Groups.Values.LastOrDefault()?.Value;

            //不包含历史消费记录账号大概率疑似未定区账号
            if (string.IsNullOrEmpty(walletBalanceString))
            {
                loginState.IsUndeterminedArea = true;
                walletBalanceString = Regex.Match(html, @"<div class=""accountData price"">(.*?)</div>").Groups.Values.LastOrDefault()?.Value;
            }

            var steamCountry = Regex.Match(html, @"国家/地区：[\s]+<span class=""account_data_field"">(?<grp0>[\w]+?)</span>").Groups.Values.LastOrDefault()?.Value;

            var email = Regex.Match(html, Regexps.Email).Groups.Values.LastOrDefault()?.Value;

            loginState.WalletBalanceString = walletBalanceString;
            loginState.SteamCountry = string.IsNullOrWhiteSpace(steamCountry) ? null : steamCountry;
            loginState.Email = email;

            // 获取货币符号
            loginState.CurrencySymbol = walletBalanceString?.Split(' ').FirstOrDefault();

            return true;
        }
        return false;
    }

    /// <summary>
    /// 调用Steam充值卡充值接口
    /// </summary>
    public async Task<(SteamResult Result, PurchaseResultDetail? Detail)?> RedeemWalletCode(SteamLoginResponse loginState, string walletCode)
    {
        if (loginState.Cookies == null)
        {
            return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, ISteamAccountService.Urls.SteamStoreRedeemWalletCodelUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
            {
                { "wallet_code", walletCode },
                { "sessionid", cookieContainer.GetCookieValue(new Uri(ISteamAccountService.Urls.SteamStore), "sessionid") },
            }),
        };

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
        cookieContainer.Add(loginState.Cookies);

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

    /// <summary>
    /// 设置Steam账号区域到指定位置
    /// </summary>
    public async Task<bool> SetSteamAccountCountry(SteamLoginResponse loginState, string? currencyCode)
    {
        if (loginState.Cookies == null)
        {
            return false;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, ISteamAccountService.Urls.SteamStoreAccountSetCountryUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
            {
                { "cc", currencyCode },
                { "sessionid", cookieContainer.GetCookieValue(new Uri(ISteamAccountService.Urls.SteamStore), "sessionid") },
            }),
        };

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
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
    }

    /// <summary>
    /// 获取当前可设置得区域列表
    /// </summary>
    public async Task<List<CurrencyData>?> GetSteamAccountCountryCodes(SteamLoginResponse loginState)
    {
        if (loginState.Cookies == null)
        {
            return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, ISteamAccountService.Urls.SteamStoreAddFundsUrl);

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd(UserAgent.Default);
        cookieContainer.Add(loginState.Cookies);

        var respone = await client.SendAsync(request);

        if (respone.IsSuccessStatusCode)
        {
            var html = await respone.Content.ReadAsStringAsync();

            var matchCollection = Regex.Matches(html, @"<div class=""currency_change_option btnv6_grey_black"" data-country=""(?<grp0>[^""]+)"" >[\s]+<span>[\s]+<div class=""country"">(?<grp1>[\w]+?)</div>");

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
    }

    public async Task<string?> GetCaptchaImageBase64(string captchaId)
    {
        var url = ISteamAccountService.Urls.CaptchaImageUrl + captchaId;
        var response = await client.GetByteArrayAsync(url);
        return Convert.ToBase64String(response);
    }
}
