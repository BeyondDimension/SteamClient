namespace BD.SteamClient8.UnitTest;

/// <summary>
/// 令牌功能测试
/// </summary>
sealed class WinAuthTest : ServiceTestBase
{
    SteamLoginState steamLoginState = null!;
    ISteamAccountService steamAccountService = null!;
    ISteamSessionService steamSessionService = null!;
    ISteamAuthenticatorService steamAuthenticatorService = null!;
    IConfiguration configuration = null!;

    SteamAuthenticator steamAuthenticator;
    SteamAuthenticator.EnrollState enrollState;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamAccountService();
        services.AddSteamAuthenticatorService();
        services.AddSteamTradeService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamAccountService = GetRequiredService<ISteamAccountService>();
        steamSessionService = GetRequiredService<ISteamSessionService>();
        configuration = GetRequiredService<IConfiguration>();
        steamAuthenticatorService = GetRequiredService<ISteamAuthenticatorService>();
        steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService, GetRequiredService<ISteamSessionService>());

        steamAuthenticator ??= await GetSteamAuthenticatorAsync() ?? new();
        enrollState ??= new();
    }

    /// <summary>
    /// 添加令牌
    /// </summary>
    /// <returns></returns>
    [Test]
    [TestCase("18112341234")]
    public async Task AddAuthenticator_Test(string phone_number)
    {
        Assert.Multiple(() =>
        {
            Assert.That(steamLoginState.AccessToken, Is.Not.Null);
            Assert.That(steamLoginState.RefreshToken, Is.Not.Null);
        });

        enrollState.AccessToken = steamLoginState.AccessToken;
        enrollState.RefreshToken = steamLoginState.RefreshToken;
        enrollState.SteamId = (long)steamLoginState.SteamId;

        await steamAuthenticator.AddAuthenticatorAsync(enrollState);

        // 没有绑定手机号
        if (enrollState.NoPhoneNumber)
        {
            var result = await steamAuthenticator.AddPhoneNumberAsync(enrollState, phone_number);

            // 需要邮箱手动确认手机号添加
            if (enrollState.RequiresEmailConfirmPhone)
            {
                await Task.Delay(TimeSpan.FromSeconds(15)); // waiting for confirm email
                result = await steamAuthenticator.AddPhoneNumberAsync(enrollState, phone_number);
                Assert.That(enrollState.RequiresEmailConfirmPhone == false && result == null);
            }

            await steamAuthenticator.AddAuthenticatorAsync(enrollState);
        }

        // 是否激活令牌
        if (enrollState.RequiresActivation)
        {
            // 输入手机收到的验证码
            enrollState.ActivationCode = "7C86B";
            var finalize = await steamAuthenticator.FinalizeAddAuthenticatorAsync(enrollState);
            Assert.That(finalize);

            Assert.That(await Update());
        }
    }

    /// <summary>
    /// 已有令牌 替换新令牌
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task RemoveAuthenticator_Test()
    {
        Assert.Multiple(() =>
        {
            Assert.That(steamLoginState.AccessToken, Is.Not.Null);
            Assert.That(steamLoginState.RefreshToken, Is.Not.Null);
        });
        enrollState.AccessToken ??= steamLoginState.AccessToken;
        enrollState.RefreshToken ??= steamLoginState.RefreshToken;
        enrollState.SteamId = (long)steamLoginState.SteamId;

        // 开始移除，注意手机验证码
        var removeStart_result = await steamAuthenticator.RemoveAuthenticatorViaChallengeStartSync(enrollState.SteamId.ToString());

        Assert.That(removeStart_result);

        // 手机验证码
        var phoneVerifyCode = "7C86B";
        var removeContinue_result = await steamAuthenticator.RemoveAuthenticatorViaChallengeContinueSync(enrollState.SteamId.ToString(), phoneVerifyCode);

        Assert.That(removeContinue_result);

        Assert.That(await Update());
    }

    /// <summary>
    /// 解绑（移除） 令牌
    /// </summary>
    /// <param name="scheme">1 = 移除令牌验证器但保留邮箱验证，2 = 移除所有防护</param>
    /// <returns></returns>
    [Test]
    [TestCase(2)]
    public async Task UnBindingAuthenticator(int scheme)
    {
        Assert.Multiple(() =>
        {
            Assert.That(steamLoginState.AccessToken, Is.Not.Null);
            Assert.That(steamLoginState.RefreshToken, Is.Not.Null);

            Assert.That(!string.IsNullOrEmpty(steamAuthenticator.RecoveryCode));
        });

        var remove_result = await steamAuthenticator.RemoveAuthenticatorAsync(steamLoginState.SteamId.ToString(), scheme);

        Assert.That(remove_result);

        Assert.That(await DeleteSteamAuthenticatorAsync());
    }

    /// <summary>
    /// 获取用户国家
    /// </summary>
    /// <returns></returns>
    public async Task GetUserCountry()
    {
        Assert.That(steamAuthenticator, Is.Not.Null);

        var country = await steamAuthenticator.GetUserCountry(steamLoginState.SteamId.ToString());
        Assert.That(string.IsNullOrEmpty(country));
    }

    private async Task<bool> Update()
    {
        Assert.That(steamAuthenticator, Is.Not.Null);

        var steamData = SystemTextJsonSerializer.Deserialize<SteamConvertSteamDataJsonStruct>(steamAuthenticator.SteamData!);

        var session = steamSessionService.RentSession(steamData.ThrowIsNull().SteamId.ToString());
        session ??= new();
        session.AccessToken = steamLoginState.AccessToken.ThrowIsNull();
        session.RefreshToken = steamLoginState.RefreshToken.ThrowIsNull();
        session.IdentitySecret = steamData.IdentitySecret;
        session.ServerTimeDiff = steamAuthenticator.ServerTimeDiff;
        steamSessionService.AddOrSetSession(session);

        return await SaveSteamAuthenticatorAsync(steamAuthenticator);
    }
}
