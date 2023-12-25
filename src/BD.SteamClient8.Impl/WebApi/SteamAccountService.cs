namespace BD.SteamClient8.Impl.WebApi;

public sealed partial class SteamAccountService : WebApiClientFactoryService, ISteamAccountService
{
    /// <inheritdoc/>
    protected sealed override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override SystemTextJsonSerializerOptions JsonSerializerOptions =>
        DefaultJsonSerializerContext_.Default.Options;

    /// <summary>
    /// 用于标识和记录日志信息
    /// </summary>
    public const string TAG = "SteamAccountWebApiS";

    /// <summary>
    /// 此标志表示不应缓存从此查询检索到的名称
    /// </summary>
    public const string default_donotcache = "-62135596800000"; // default(DateTime).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();

    readonly IRandomGetUserAgentService uas;

    readonly ISteamSessionService sessions;

    /// <summary>
    /// 初始化 <see cref="SteamAccountService"/> 类的新实例
    /// </summary>
    /// <param name="uas"></param>
    /// <param name="sessions"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceProvider"></param>
    [ActivatorUtilitiesConstructor]
    public SteamAccountService(
        IRandomGetUserAgentService uas,
        ISteamSessionService sessions,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider) : base(
            loggerFactory.CreateLogger(TAG),
            serviceProvider)
    {
        this.uas = uas;
        this.sessions = sessions;
    }

    /// <summary>
    /// 是否使用重试机制
    /// </summary>
    [Obsolete("根据业务而定")]
    public bool UseRetry { get; set; } = true;

    /// <summary>
    /// 等待一个时间并且重试
    /// </summary>
    /// <returns></returns>
    AsyncRetryPolicy WaitAndRetryAsync(
        IEnumerable<TimeSpan> retryTimes,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0) => Policy.Handle<Exception>().WaitAndRetryAsync(retryTimes, (ex, _, i, _) =>
        {
            logger.LogError(ex, "第 {i} 次重试，MemberName：{memberName}，FilePath：{sourceFilePath}，LineNumber：{sourceLineNumber}", i, memberName, sourceFilePath, sourceLineNumber);
        });

    /// <summary>
    /// 重试间隔
    /// </summary>
    static readonly IEnumerable<TimeSpan> sleepDurations = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), };

    #region Public Methods

    /// <inheritdoc/>
    public async Task<ApiRspImpl<(string encryptedPassword64, string timestamp)>> GetRSAkeyAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var stm_login_getrsakey_req_form = new Dictionary<string, string>()
        {
            { "donotache", default_donotcache },
            { "username", username },
        };
        async Task<ApiRspImpl<(string encryptedPassword64, string timestamp)>> GetRSAkeyAsync(CancellationToken cancellationToken = default)
        {
            var result = await GetRSAkeyCoreAsync(username, password, stm_login_getrsakey_req_form, cancellationToken);
            return result;
        }
        bool HasValue(ApiRspImpl<(string encryptedPassword64, string timestamp)>? result)
        {
            if (result is null)
                return false;
            if (!result.IsSuccess)
                return false;
            if (string.IsNullOrWhiteSpace(result.Content.encryptedPassword64))
                return false;
            if (string.IsNullOrWhiteSpace(result.Content.timestamp))
                return false;
            return true;
        }
        var result = await Policy.HandleResult<ApiRspImpl<(string encryptedPassword64, string timestamp)>>(HasValue).WaitAndRetryAsync(sleepDurations).ExecuteAsync(GetRSAkeyAsync, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>> GetRSAkeyV2Async(string username, string password, CancellationToken cancellationToken = default)
    {
        var input_protobuf_encoded = UrlEncoder.Default.Encode(new CAuthentication_GetPasswordRSAPublicKey_Request()
        {
            AccountName = username,
        }.ToByteString().ToBase64());
        var requestUriString = SteamApiUrls.GetRSAkeyV2Url.Format(input_protobuf_encoded);
        var requestUri = new Uri(requestUriString);

        async Task<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>> GetRSAkeyV2Async(CancellationToken cancellationToken = default)
        {
            var result = await GetRSAkeyV2CoreAsync(username, password, requestUri, requestUriString, cancellationToken);
            return result;
        }
        bool HasValue(ApiRspImpl<(string encryptedPassword64, ulong timestamp)>? result)
        {
            if (result is null)
                return false;
            if (!result.IsSuccess)
                return false;
            if (string.IsNullOrWhiteSpace(result.Content.encryptedPassword64))
                return false;
            return true;
        }
        var result = await Policy.HandleResult<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>>(HasValue).WaitAndRetryAsync(sleepDurations).ExecuteAsync(GetRSAkeyV2Async, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl> DoLoginAsync(SteamLoginState loginState, bool isTransfer = false, bool isDownloadCaptchaImage = false, CancellationToken cancellationToken = default)
    {
        loginState.Success = false;

        if (string.IsNullOrEmpty(loginState.Username) ||
            string.IsNullOrEmpty(loginState.Password))
        {
            return loginState.Message = "请填写正确的 Steam 用户名密码";
        }

        loginState.Username = SteamUNPWDRegex().Replace(loginState.Username, string.Empty);
        loginState.Password = SteamUNPWDRegex().Replace(loginState.Password, string.Empty);

        if (string.IsNullOrEmpty(loginState.Cookies?["sessionid"]?.Value))
        {
            // 访问一次登录页获取 SessionId
            using WebApiClientSendArgs args = new(SteamApiUrls.SteamLoginUrl)
            {
                Method = HttpMethod.Get,
            };
            var client = CreateClient(loginState.Username);
            args.SetHttpClient(client);
            await SendAsync<string>(args, cancellationToken);
        }

        var rsaKey = await GetRSAkeyAsync(loginState.Username, loginState.Password, cancellationToken);
        if (!rsaKey.IsSuccess)
            return rsaKey.GetMessage();

        var (encryptedPassword64, timestamp) = rsaKey.Content;

        async Task<ApiRspImpl<SystemTextJsonObject?>> DoLoginCoreAsync(CancellationToken cancellationToken = default)
        {
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
                { "donotache", default_donotcache },
            };

            using WebApiClientSendArgs args = new(SteamApiUrls.DologinUrl)
            {
                Method = HttpMethod.Post,
                ContentType = MediaTypeNames.FormUrlEncoded
            };
            try
            {
                var client = CreateClient(loginState.Username);
                args.SetHttpClient(client);
                using var dologinRspMsg = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(args, data, cancellationToken);
                dologinRspMsg.ThrowIsNull();
                if (dologinRspMsg.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    loginState.Requires2FA = false;
                    loginState.RequiresCaptcha = false;
                    loginState.RequiresEmailAuth = false;
                    loginState.Success = false;
                    return loginState.Message = $"{HttpStatusCode.TooManyRequests} 请求过于频繁，请稍后再试";
                }

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                // SystemTextJsonObject 应配置为跳过裁剪所以忽略警告
                var dologinRspJObj = await ReadFromSJsonAsync<SystemTextJsonObject>(dologinRspMsg.Content, cancellationToken);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                if (dologinRspJObj is null)
                {
                    loginState.Success = false;
                    return loginState.Message = $"登录错误: 无效的 {nameof(dologinRspJObj)}";
                }

                return dologinRspJObj;
            }
            catch (Exception ex)
            {
                var errorResult = OnErrorReApiRspBase<ApiRspImpl<SystemTextJsonObject?>>(ex, args);
                loginState.Message = $"登录错误: " + errorResult.GetMessage();
                return errorResult;
            }
        }

        bool HasValue(ApiRspImpl<SystemTextJsonObject?>? result)
        {
            if (result is null)
                return false;
            if (!result.IsSuccess)
                return false;
            if (result.Content == null)
                return false;
            return true;
        }

        var result = await Policy.HandleResult<ApiRspImpl<SystemTextJsonObject?>>(HasValue).WaitAndRetryAsync(sleepDurations).ExecuteAsync(DoLoginCoreAsync, cancellationToken);
        var dologinRspJObj = result.Content;
        if (dologinRspJObj == null)
        {
            return result.GetMessage();
        }

        var message = dologinRspJObj["message"]?.GetValue<string>();

        var emailsteamid = dologinRspJObj["emailsteamid"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(emailsteamid))
        {
            loginState.SteamIdString = emailsteamid;
        }

        var captcha_needed = dologinRspJObj["captcha_needed"]?.GetValue<bool>();
        loginState.RequiresCaptcha = captcha_needed.HasValue && captcha_needed.Value;
        if (loginState.RequiresCaptcha)
        {
            if (message?.Contains("验证码中的字符", StringComparison.OrdinalIgnoreCase) == true)
            {
                loginState.CaptchaId = dologinRspJObj["captcha_gid"]?.GetValue<string>();
                loginState.CaptchaUrl = SteamApiUrls.CaptchaImageUrl + loginState.CaptchaId;
                if (isDownloadCaptchaImage && !string.IsNullOrEmpty(loginState.CaptchaId))
                {
                    loginState.CaptchaImageBase64 = await GetCaptchaImageBase64(loginState.CaptchaId);
                }
                return loginState.Message = $"登录错误: " + message;
            }
            else
            {
                loginState.RequiresCaptcha = false;
                loginState.CaptchaId = null;
                loginState.CaptchaUrl = null;
                loginState.CaptchaText = null;
                return loginState.Message = $"登录错误: " + message;
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
        var emailauth_needed = dologinRspJObj["emailauth_needed"]?.GetValue<bool>();
        loginState.RequiresEmailAuth = emailauth_needed.HasValue && emailauth_needed.Value;
        if (loginState.RequiresEmailAuth)
        {
            var emaildomain = dologinRspJObj["emaildomain"]?.GetValue<string>();
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
        var requires_twofactor = dologinRspJObj["requires_twofactor"]?.GetValue<bool>();
        loginState.Requires2FA = requires_twofactor.HasValue && requires_twofactor.Value;
        if (loginState.Requires2FA)
        {
            return loginState.Message = $"登录错误: 需要输入令牌";
        }

        // 登录因为其它原因失败
        var login_complete = dologinRspJObj["login_complete"]?.GetValue<bool>();
        loginState.Success = login_complete.HasValue && login_complete.Value;
        if (!loginState.Success)
        {
            if (!string.IsNullOrEmpty(message))
            {
                return loginState.Message = $"登录错误: " + message;
            }
        }
        else
        {
            // 登录成功
            DoLoginResponse? dologinRspObj = null;
            try
            {
                dologinRspObj = SystemTextJsonSerializer.Deserialize(dologinRspJObj, DefaultJsonSerializerContext_.Default.DoLoginResponse);
            }
            catch (Exception ex)
            {
                var errorResult = OnSerializerErrorReApiRspBase<ApiRspImpl>(ex, isSerializeOrDeserialize: false, typeof(DoLoginResponse));
                loginState.Message = $"登录错误: " + errorResult.GetMessage();
                return errorResult;
            }
            if (dologinRspObj != null)
            {
                var cookieContainer = GetCookieContainer(loginState.Username);
                var session = new SteamSession
                {
                    Cookies = cookieContainer.GetAllCookies(),
                    SteamId = loginState.SteamId.ToString(),
                };
                loginState.Cookies = cookieContainer.GetAllCookies();
                if (dologinRspObj.TransferParameters != null && dologinRspObj.TransferUrls != null)
                {
                    session.AccessToken = dologinRspObj.TransferParameters.Auth.ThrowIsNull();
                    loginState.SteamIdString = dologinRspObj.TransferParameters.Steamid;

                    _ = ulong.TryParse(loginState.SteamIdString, out var steamid);
                    loginState.SteamId = steamid;

                    if (isTransfer)
                    {
                        foreach (var transferUrl in dologinRspObj.TransferUrls)
                        {
                            await Policy.Handle<Exception>().WaitAndRetryAsync(sleepDurations).ExecuteAsync(async (cancellationToken) =>
                            {
                                using WebApiClientSendArgs args = new(transferUrl)
                                {
                                    Method = HttpMethod.Post,
                                };
                                var client = CreateClient(loginState.Username);
                                args.SetHttpClient(client);
                                using var transferUrlRsp = await SendAsync<HttpResponseMessage>(args, cancellationToken);
                                transferUrlRsp.ThrowIsNull();
                                transferUrlRsp.EnsureSuccessStatusCode();
                            }, cancellationToken);
                        }
                    }
                }
                session.GenerateSetCookie();
                sessions.AddOrSetSession(session);
                return ApiRspHelper.Ok();
            }
        }
        return loginState.Message = "登录错误: 出现未知错误";
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<CookieCollection?>> OpenIdLoginAsync(string openidparams, string nonce, CookieCollection cookie) => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<ApiRspImpl> DoLoginV2Async(SteamLoginState loginState)
    {
        loginState.Success = false;

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var cookieContainer = GetCookieContainer(loginState.Username.ThrowIsNull());

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
                return loginState.Message = "请填写正确的 Steam 用户名密码";
            }

            // Steam 会从用户名和密码中删除所有非 ASCII 字符
            loginState.Username = SteamUNPWDRegex().Replace(loginState.Username, string.Empty);
            loginState.Password = SteamUNPWDRegex().Replace(loginState.Password, string.Empty);

            if (string.IsNullOrEmpty(loginState.SeesionId))
            {
                // 访问一次登录页获取 SessionId
                await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
                {
                    await client.GetAsync("https://store.steampowered.com/login/");
                });
                loginState.SeesionId = cookieContainer.GetCookieValue(new Uri(SteamApiUrls.STEAM_STORE_URL), "sessionid");
            }

            var (encryptedPassword64, timestamp) = (await GetRSAkeyV2Async(loginState.Username, loginState.Password)).Content;

            if (string.IsNullOrEmpty(encryptedPassword64))
            {
                loginState.ResetStatus();
                return loginState.Message = "登录失败，获取 RSAkey 出现错误，请重试";
            }

            var (result, response) = await BeginAuthSessionViaCredentials(encryptedPassword64, timestamp);

            loginState.ClientId = result.ClientId;
            loginState.SteamId = result.Steamid;
            loginState.RequestId = result.RequestId.ToByteArray();

            if (loginState.SteamId == 0)
            {
                var eResult = response.Headers.GetValues("X-eresult").FirstOrDefault();

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
                return loginState.Message;
            }

            if (result.AllowedConfirmations.Count > 0)
            {
                if (result.AllowedConfirmations[0].ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeDeviceCode)
                {
                    loginState.Requires2FA = true;
                    loginState.Success = false;
                    return loginState.Message = "需要2FA验证码";
                }
                else if (result.AllowedConfirmations[0].ConfirmationType == EAuthSessionGuardType.KEauthSessionGuardTypeEmailCode)
                {
                    await JwtCheckDevice();
                    loginState.RequiresEmailAuth = true;
                    loginState.Success = false;
                    return loginState.Message = "需要邮箱验证码";
                }
            }
        }

        var pollAuthSessionStatusResponse = await PollAuthSessionStatusAsync(loginState.ClientId!.Value, loginState.RequestId!);
        loginState.AccessToken = pollAuthSessionStatusResponse.AccessToken;
        loginState.RefreshToken = pollAuthSessionStatusResponse.RefreshToken;

        if (string.IsNullOrEmpty(pollAuthSessionStatusResponse.RefreshToken))
        {
            loginState.ResetStatus();
            return loginState.Message = "登录失败，请确认输入验证码是否正确。";
        }

        var tokens = await FinalizeLoginAsync(pollAuthSessionStatusResponse.RefreshToken, loginState.SeesionId);

        if (string.IsNullOrEmpty(tokens?.SteamId))
        {
            loginState.ResetStatus();
            return loginState.Message = "FinalizeLoginAsync 登录失败";
        }

        loginState.Success = true;
        loginState.RequiresCaptcha = false;
        loginState.Requires2FA = false;
        loginState.RequiresEmailAuth = false;
        loginState.Message = null;

        if (tokens.TransferInfo?.Count > 0)
        {
            foreach (var transfer in tokens.TransferInfo)
            {
                if (string.IsNullOrEmpty(transfer.Url) || transfer.Url?.Contains("help.steampowered.com") == true ||
                    transfer.Url?.Contains("steam.tv") == true)
                {
                    //跳过暂时用不到的域名 节约带宽
                    continue;
                }

                HttpResponseMessage? rsp = null;
                await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
                {
                    using var sendArgs = new WebApiClientSendArgs(transfer.Url!)
                    {
                        Method = HttpMethod.Post,
                        ContentType = MediaTypeNames.FormUrlEncoded,
                        ConfigureRequestMessage = (req, args, token) =>
                        {
                            req.Headers.UserAgent.Clear();
                            req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                        }
                    };
                    sendArgs.SetHttpClient(client);
                    var param = new Dictionary<string, string?>()
                    {
                        { "nonce", transfer.Params?.Nonce },
                        { "auth", transfer.Params?.Auth },
                        { "steamID", loginState.SteamId.ToString() },
                    };

                    rsp = (await SendAsync<HttpResponseMessage, Dictionary<string, string?>>(sendArgs, param))?.EnsureSuccessStatusCode();
                    //var res = data.Content.ReadAsStringAsync();
                });
            }
            loginState.Cookies = cookieContainer.GetAllCookies();
        }
        var session = new SteamSession();
        session.SteamId = loginState.SteamId.ToString();
        session.AccessToken = loginState.AccessToken;
        session.RefreshToken = loginState.RefreshToken;
        session.Cookies = cookieContainer.GetAllCookies();
        session.GenerateSetCookie();
        sessions.AddOrSetSession(session);
        return ApiRspHelper.Ok();

        async Task JwtCheckDevice()
        {
            await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
            {
                using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_CHECKDEVICE.Format(loginState.SteamId))
                {
                    Method = HttpMethod.Post,
                    ContentType = MediaTypeNames.FormUrlEncoded,
                    ConfigureRequestMessage = (req, args, token) =>
                    {
                        req.Headers.UserAgent.Clear();
                        req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                    }
                };
                sendArgs.SetHttpClient(client);
                var param = new Dictionary<string, string?>()
                {
                    { "clientid", loginState.ClientId.ToString() },
                    { "steamid", loginState.SteamId.ToString() },
                };

                (await SendAsync<HttpResponseMessage, Dictionary<string, string?>>(sendArgs, param))?.EnsureSuccessStatusCode();
            });
        }

        async Task<(CAuthentication_BeginAuthSessionViaCredentials_Response, HttpResponseMessage)> BeginAuthSessionViaCredentials(string encryptedPassword64, ulong timestamp)
        {
            var input_protobuf_encoded = new CAuthentication_BeginAuthSessionViaCredentials_Request()
            {
                AccountName = loginState.Username,
                DeviceFriendlyName = uas.GetUserAgent(),
                EncryptedPassword = encryptedPassword64,
                EncryptionTimestamp = timestamp,
                WebsiteId = "Community",
                PlatformType = EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser,
                RememberLogin = false,
                Persistence = ESessionPersistence.KEsessionPersistencePersistent,
            }.ToByteString().ToBase64();

            return await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
            {
                var data = new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", input_protobuf_encoded },
                };

                using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_BEGINAUTHSESSIONVIACREDENTIALS)
                {
                    Method = HttpMethod.Post,
                    ContentType = MediaTypeNames.FormUrlEncoded,
                    ConfigureRequestMessage = (req, args, token) =>
                    {
                        req.Headers.UserAgent.Clear();
                        req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                    }
                };
                sendArgs.SetHttpClient(client);

                if (loginState.Cookies != null)
                    cookieContainer.Add(loginState.Cookies);

                var response = await SendAsync<HttpResponseMessage, Dictionary<string, string?>>(sendArgs, data);
                using var responseStream = await response.ThrowIsNull().Content.ReadAsStreamAsync();
                var result = CAuthentication_BeginAuthSessionViaCredentials_Response.Parser.ParseFrom(responseStream);
                return (result, response);
            });
        }

        async Task<CAuthentication_PollAuthSessionStatus_Response> PollAuthSessionStatusAsync(ulong client_id, byte[] request_id)
        {
            var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
            {
                using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_POLLAUTHSESSIONSTATUS)
                {
                    Method = HttpMethod.Post,
                    ContentType = MediaTypeNames.FormUrlEncoded,
                };
                sendArgs.SetHttpClient(client);

                var param = new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", new CAuthentication_PollAuthSessionStatus_Request()
                        {
                            ClientId = client_id,
                            RequestId = ByteString.CopyFrom(request_id),
                        }.ToByteString().ToBase64()
                    },
                };

                var response = await SendAsync<HttpResponseMessage, Dictionary<string, string?>>(sendArgs, param);

                if (!response.ThrowIsNull().IsSuccessStatusCode)
                {
                    throw new Exception($"PollAuthSessionStatus 出现错误: {response.StatusCode}");
                }

                var result = CAuthentication_PollAuthSessionStatus_Response.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());
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

            var result = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
            {
                using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_UPDATEAUTHSESSIONWITHSTEAMGUARDCODE)
                {
                    Method = HttpMethod.Post,
                    ContentType = MediaTypeNames.FormUrlEncoded
                };
                sendArgs.SetHttpClient(client);
                var param = new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", input_protobuf_encoded },
                };

                var response = await SendAsync<HttpResponseMessage, Dictionary<string, string?>>(sendArgs, param);

                if (response != null && !response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            });

            return result;
        }

        async Task<FinalizeLoginStatus> FinalizeLoginAsync(string nonce, string? sessionid)
        {
            var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
            {
                using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_FINALIZELOGIN)
                {
                    Method = HttpMethod.Post,
                    ContentType = MediaTypeNames.FormUrlEncoded
                };
                sendArgs.SetHttpClient(client);
                var param = new Dictionary<string, string?>()
                {
                    { "nonce", nonce },
                    { "sessionid", sessionid },
                    { "redir", "https://steamcommunity.com/login/home/?goto=" },
                };

                var response = await SendAsync<HttpResponseMessage, Dictionary<string, string?>>(sendArgs, param);

                if (!response.ThrowIsNull().IsSuccessStatusCode)
                {
                    throw new Exception($"FinalizeLoginAsync 出现错误: {response.StatusCode}");
                }

                var result = await ReadFromSJsonAsync<FinalizeLoginStatus>(response.Content);

                if (result == null)
                {
                    throw new Exception($"FinalizeLoginAsync 出现错误: {result}");
                }

                return result;
            });
            return r;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<(bool IsSuccess, string? Message, HistoryParseResponse? History)>> GetAccountHistoryDetail(SteamLoginState loginState)
    {
        (bool IsSuccess, string? Message, HistoryParseResponse? History) result = (false, "获取 Steam 账号消费历史记录出现错误", null);

        if (loginState.Cookies == null)
        {
            return result;
        }

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_HISTORY_DETAIL)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            }
        };
        sendArgs.SetHttpClient(client);

        var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            var html = await SendAsync<string>(sendArgs);

            if (!string.IsNullOrEmpty(html))
            {
                //var historyCursorString = Regex.Match(html, @"var g_historyCursor = (?<grp0>[^;]+);").Groups.Values.LastOrDefault()?.Value;

                //var historyCursor = JsonSerializer.Deserialize<CursorData>(historyCursorString);

                //var request1 = new HttpRequestMessage(HttpMethod.Post, SteamApiUrls.STEAM_ACCOUNT_HISTORY_AJAX)
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

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> GetWalletBalance(SteamLoginState loginState)
    {
        if (loginState.Cookies == null)
        {
            return false;
        }

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            }
        };
        sendArgs.SetHttpClient(client);

        var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            var html = await SendAsync<string>(sendArgs);

            if (!string.IsNullOrEmpty(html))
            {
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

    /// <inheritdoc/>
    public async Task<ApiRspImpl<(SteamResult Result, PurchaseResultDetail? Detail)?>> RedeemWalletCode(SteamLoginState loginState, string walletCode, bool isRetry = false)
    {
        if (loginState.Cookies == null || string.IsNullOrEmpty(loginState.Cookies?["sessionid"]?.Value))
            return ApiRspHelper.Fail<(SteamResult Result, PurchaseResultDetail? Detail)?>("Parameter Cookies not be null in SteamLoginState");

        if (isRetry)
            return await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () => await RedeemWalletCodeCore(loginState, walletCode));
        else
            return await RedeemWalletCodeCore(loginState, walletCode);
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> SetSteamAccountCountry(SteamLoginState loginState, string? currencyCode)
    {
        if (loginState.Cookies == null)
        {
            return false;
        }
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_SETCOUNTRY)
        {
            Method = HttpMethod.Post,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                req.Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                    {
                        { "cc", currencyCode },
                        { "sessionid", loginState.Cookies["sessionid"]?.Value },
                    });
            }
        };
        sendArgs.SetHttpClient(client);

        var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            var html = await SendAsync<string>(sendArgs);

            if (!string.IsNullOrEmpty(html))
            {
                if (bool.TryParse(html, out bool isOk) && isOk)
                {
                    return true;
                }
            }
            return false;
        });
        return r;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<List<CurrencyData>?>> GetSteamAccountCountryCodes(SteamLoginState loginState)
    {
        if (loginState.Cookies == null)
        {
            return ApiRspHelper.Fail<List<CurrencyData>?>("Parameter Cookies not be null in SteamLoginState");
        }

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_ADD_FUNDS)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            }
        };
        sendArgs.SetHttpClient(client);

        var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            var html = await SendAsync<string>(sendArgs);

            if (!string.IsNullOrEmpty(html))
            {
                var matchCollection = CountryItemRegex().Matches(html);

                if (matchCollection.Count > 0)
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

    /// <inheritdoc/>
    public async Task<ApiRspImpl<InventoryTradeHistoryRenderPageResponse>> GetInventoryTradeHistory(SteamLoginState loginState, int[]? appFilter = null, InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null)
    {
        StringBuilder urlBuilder = new StringBuilder($"{SteamApiUrls.STEAM_COMMUNITY_URL}/profiles/{loginState.SteamId}/inventoryhistory/?ajax=1&sessionid={loginState.SeesionId}");

        if (cursor != null)
        {
            urlBuilder.Append($"&cursor%5Btime%5D={cursor.Time}&cursor%5Btime_frac%5D={cursor.TimeFrac}&cursor%5Bs%5D={cursor.S}");
        }

        if (appFilter != null && appFilter.Length > 0)
        {
            foreach (var app in appFilter)
            {
                urlBuilder.Append($"&app%5B%5D={app}");
            }
        }

        var cookieCollection = new CookieCollection
        {
            loginState.Cookies!,
            new Cookie()
            {
                Name = "Steam_Language",
                Value = loginState.Language ?? "schinese",
                Domain = new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host,
                Secure = true,
                Path = "/",
            },
            new Cookie()
            {
                Name = "timezoneOffset",
                Value = Uri.EscapeDataString($"{TimeSpan.FromHours(8).TotalSeconds},0"),
                Domain = new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host
            }
        };

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(cookieCollection);
        using var sendArgs = new WebApiClientSendArgs(urlBuilder.ToString())
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
        };
        sendArgs.SetHttpClient(client);
        var result = await SendAsync<InventoryTradeHistoryRenderPageResponse>(sendArgs);
        return result!;
    }

    /// <inheritdoc/>
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

            if (items == null || items.Length <= 0)
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

    /// <inheritdoc/>
    public async Task<ApiRspImpl<InventoryPageResponse?>> GetInventories(ulong steamId, string appId, string contextId, int count = 100, string? startAssetId = null, string language = "schinese")
    {
        string url = string.Format("{0}/inventory/{1}/{2}/{3}?l={4}&count={5}{6}",
            SteamApiUrls.STEAM_COMMUNITY_URL,
            steamId,
            appId,
            contextId,
            language,
            count,
            !string.IsNullOrEmpty(startAssetId) ? $"&start_assetid={startAssetId}" : string.Empty);

        using var sendArgs = new WebApiClientSendArgs(url);
        InventoryPageResponse? inventories = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            var result = await SendAsync<InventoryPageResponse>(sendArgs);
            if (result is null)
                throw new NullReferenceException("GetInventories 429 too many requests");
            return result;
        });

        return inventories;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string?>> GetApiKey(SteamLoginState loginState)
    {
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_APIKEY_GET)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
            }
        };
        sendArgs.SetHttpClient(client);
        var stream = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            return await SendAsync<Stream>(sendArgs);
        });
        return ApiRspHelper.Ok(ParseApiKeyFromHttpContent(stream));
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string?>> RegisterApiKey(SteamLoginState loginState, string? domain = null)
    {
        if (string.IsNullOrEmpty(loginState.SeesionId))
            throw new ArgumentException(nameof(loginState.SeesionId));

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            { "domain", domain ?? Guid.NewGuid().ToString("N") },
            { "agreeToTerms", "agreed" },
            { "sessionid", loginState.SeesionId },
            { "Submit", "注册" },
        };

        var cookieCollection = new CookieCollection
        {
            loginState.Cookies!,
            new Cookie()
            {
                Name = "sessionid",
                Value = loginState.SeesionId,
                Domain = new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host,
                Secure = true,
                Path = "/"
            }
        };
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(cookieCollection);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_APIKEY_REGISTER)
        {
            Method = HttpMethod.Post,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                req.Headers.Add("origin", "https://steamcommunity.com");
                req.Headers.Add("authority", "steamcommunity.com");
                req.Headers.Add("referer", "https://steamcommunity.com/dev/revokekey");
                req.Headers.Add("accept-language", "zh-CN");
                req.Content = new FormUrlEncodedContent(data);
            }
        };
        sendArgs.SetHttpClient(client);
        var stream = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            return await SendAsync<Stream>(sendArgs);
        });
        return ApiRspHelper.Ok(ParseApiKeyFromHttpContent(stream));
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<IEnumerable<SendGiftHistoryItem>>> GetSendGiftHistories(SteamLoginState loginState)
    {
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_SENDGIFTHISTORIES)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
        };
        sendArgs.SetHttpClient(client);
        using Stream? respStream = await SendAsync<Stream>(sendArgs);

        if (respStream == null)
            return ApiRspHelper.Fail<IEnumerable<SendGiftHistoryItem>>("访问网站页面信息异常")!;

        IBrowsingContext context = BrowsingContext.New();

        var htmlParser = context.GetService<IHtmlParser>();

        if (htmlParser == null)
            return ApiRspHelper.Fail<IEnumerable<SendGiftHistoryItem>>("获取Html解析器失败")!;

        var document = await htmlParser.ParseDocumentAsync(respStream);

        List<SendGiftHistoryItem> result = [];

        var rowElements = document.QuerySelectorAll("div.sent_gift");

        if (rowElements != null && rowElements.Length > 0)
        {
            foreach (var rowElement in rowElements)
            {
                var giftItemElement = rowElement.QuerySelector("div.gift_item");
                var giftStatusElement = rowElement.QuerySelector("div.gift_status_area");

                SendGiftHistoryItem giftHistoryItem = new SendGiftHistoryItem()
                {
                    Name = giftItemElement?.QuerySelector("div.gift_item_details > b")?.TextContent?.Trim() ?? string.Empty,
                    ImgUrl = giftItemElement?.QuerySelector("div.item_icon > img")?.GetAttribute("src") ?? string.Empty,
                    RedeemedGiftStatusText = giftItemElement?.QuerySelector("div.sent_gift_actions > span")?.TextContent?.Trim() ?? string.Empty,
                    GiftStatusText = giftStatusElement?.TextContent?.Trim() ?? string.Empty
                };

                result.Add(giftHistoryItem);
            }
        }

        return result!;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<LoginHistoryItem>? GetLoginHistory(SteamLoginState loginState)
    {
        const string url = SteamApiUrls.STEAM_ACCOUNT_HISTORY_LOGIN;

        var cookieCollection = new CookieCollection
        {
            loginState.Cookies!,
            new Cookie("sessionid", loginState.SeesionId, "/", $".{new Uri(url).Host}"),
            new Cookie("steamLoginSecure", loginState.Cookies!["steamLoginSecure"]?.Value, "/", $".{new Uri(url).Host}")
        };

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(cookieCollection);
        using var sendArgs = new WebApiClientSendArgs(url)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
        };
        sendArgs.SetHttpClient(client);

        using var respStream = await SendAsync<Stream>(sendArgs);

        if (respStream == null)
            throw new Exception("访问网站页面信息异常")!;

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

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> CheckAccessTokenValidation(string access_token)
    {
        var rsp = await CreateClient().GetAsync(string.Format(SteamApiUrls.STEAM_ACCOUNT_GET_STEAMNOTIFICATION, access_token));

        if (rsp.IsSuccessStatusCode)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Static Methods

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
    #endregion

    #region Private Methods

    private async Task<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>> GetRSAkeyV2CoreAsync(string username, string password, Uri requestUri, string requestUriString, CancellationToken cancellationToken = default)
    {
        using WebApiClientSendArgs args = new(requestUri, requestUriString)
        {
            Method = HttpMethod.Get,
        };
        try
        {
            var client = CreateClient(username);
            args.SetHttpClient(client);

            using var stm_login_getrsakey_rsp_stream = await SendAsync<Stream>(args, cancellationToken);
            var result = CAuthentication_GetPasswordRSAPublicKey_Response.Parser.ParseFrom(stm_login_getrsakey_rsp_stream);

            // 使用 RSA 密钥加密密码
            using var rsa = RSA.Create();
            var passwordBytes = Encoding.ASCII.GetBytes(password);
            var p = rsa.ExportParameters(false);
            p.Exponent = Convert.FromHexString(result.PublickeyExp);
            p.Modulus = Convert.FromHexString(result.PublickeyMod);
            rsa.ImportParameters(p);
            byte[] encryptedPassword = rsa.Encrypt(passwordBytes, RSAEncryptionPadding.Pkcs1);
            var encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
            return (encryptedPassword64, result.Timestamp);
        }
        catch (Exception ex)
        {
            var errorResult = OnErrorReApiRspBase<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>>(ex, args);
            return errorResult;
        }
    }

    private async Task<ApiRspImpl<(string encryptedPassword64, string timestamp)>> GetRSAkeyCoreAsync(string username, string password, Dictionary<string, string> stm_login_getrsakey_req_form, CancellationToken cancellationToken = default)
    {
        const string error_prefix = "获取 RSAKey 出现错误: ";

        using WebApiClientSendArgs args = new(SteamApiUrls.GetRSAkeyUrl)
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        try
        {
            var client = CreateClient(username);
            args.SetHttpClient(client);

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            // SystemTextJsonObject 与 Dictionary 应配置为跳过裁剪所以忽略警告
            var stm_login_getrsakey_rsp_jobj = await SendAsync<SystemTextJsonObject, Dictionary<string, string>>(args, stm_login_getrsakey_req_form, cancellationToken);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

            if (stm_login_getrsakey_rsp_jobj is null)
                return $"{error_prefix}stm_login_getrsakey_rsp_jobj is null.";

            var success = stm_login_getrsakey_rsp_jobj["success"]?.GetValue<bool>();
            var publickey_exp = stm_login_getrsakey_rsp_jobj["publickey_exp"]?.GetValue<string>();
            var publickey_mod = stm_login_getrsakey_rsp_jobj["publickey_mod"]?.GetValue<string>();
            var timestamp = stm_login_getrsakey_rsp_jobj["timestamp"]?.GetValue<string>();
            if (!(success.HasValue && success.Value) ||
                string.IsNullOrEmpty(publickey_exp) ||
                string.IsNullOrEmpty(publickey_mod) ||
                string.IsNullOrEmpty(timestamp))
                return $"{error_prefix} stm_login_getrsakey_rsp_jobj value incorrect.";

            // 使用 RSA 密钥加密密码
            using var rsa = RSA.Create();
            var passwordBytes = Encoding.ASCII.GetBytes(password);
            var p = rsa.ExportParameters(false);
            p.Exponent = Convert.FromHexString(publickey_exp);
            p.Modulus = Convert.FromHexString(publickey_mod);
            rsa.ImportParameters(p);
            byte[] encryptedPassword = rsa.Encrypt(passwordBytes, RSAEncryptionPadding.Pkcs1);
            var encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
            return (encryptedPassword64, timestamp);
        }
        catch (Exception ex)
        {
            var errorResult = OnErrorReApiRspBase<ApiRspImpl<(string encryptedPassword64, string timestamp)>>(ex, args);
            return errorResult;
        }
    }

    private async Task<(SteamResult Result, PurchaseResultDetail? Detail)?> RedeemWalletCodeCore(SteamLoginState loginState, string walletCode)
    {
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_ACCOUNT_REDEEMWALLETCODE)
        {
            Method = HttpMethod.Post,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                req.Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "wallet_code", walletCode },
                    { "sessionid", loginState.Cookies?["sessionid"]?.Value },
                });
            }
        };
        sendArgs.SetHttpClient(client);

        var detail = await SendAsync<RedeemWalletResponse>(sendArgs);

        if (detail == null)
            return null;

        if ((detail.Result != SteamResult.OK) || (detail.Detail != PurchaseResultDetail.NoDetail))
            return (detail.Result == SteamResult.OK ? SteamResult.Fail : detail.Result, detail.Detail);

        return (SteamResult.Fail, PurchaseResultDetail.NoDetail);
    }

    /// <summary>
    /// 获取验证码图片 Base64
    /// </summary>
    /// <param name="captchaId"></param>
    /// <returns></returns>
    private async Task<string?> GetCaptchaImageBase64(string captchaId)
    {
        var r = await WaitAndRetryAsync(sleepDurations).ExecuteAsync(async () =>
        {
            var url = SteamApiUrls.CaptchaImageUrl + captchaId;
            var response = await CreateClient().GetByteArrayAsync(url);
            return Convert.ToBase64String(response);
        });
        return r;
    }

    /// <summary>
    /// 解析开发密钥信息
    /// </summary>
    /// <param name="respStream"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private string? ParseApiKeyFromHttpContent(Stream? respStream)
    {
        if (respStream == null)
            return string.Empty;

        const string selector = "#bodyContents_ex > p";
        const string revokeKeyUrl = "https://steamcommunity.com/dev/revokekey";
        using (respStream)
        {
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

            if (paramList == null || paramList.Length <= 0)
                return string.Empty;

            string keyParam = paramList.First().TextContent.Trim().Replace(" ", string.Empty);

            return keyParam[(keyParam.LastIndexOf(':') + 1)..];
        }
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
    #endregion

    #region GeneratedRegex

    /// <summary>
    /// Steam 会从用户名和密码中删除所有非 ASCII 字符
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("[^\\u0000-\\u007F]")]
    private static partial Regex SteamUNPWDRegex();

    [GeneratedRegex("<input type=\"hidden\" name=\"(.*?)\" value=\"(.*?)\" />")]
    private static partial Regex OpenIdLoginRegex();

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

    [GeneratedRegex("<div class=\"currency_change_option btnv6_grey_black\" data-country=\"(?<grp0>[^\"]+)\" >[\\s]+<span>[\\s]+<div class=\"country\">(?<grp1>[\\w]+?)</div>")]
    private static partial Regex CountryItemRegex();
    #endregion
}
