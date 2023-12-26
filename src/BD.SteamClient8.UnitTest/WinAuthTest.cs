namespace BD.SteamClient8.UnitTest;

/// <summary>
/// 令牌功能测试
/// </summary>
sealed class WinAuthTest : ServiceTestBase
{
    ISteamAccountService steamAccountService = null!;
    ISteamSessionService steamSessionService = null!;
    IConfiguration configuration = null!;

    SteamAuthenticator.EnrollState enrollState;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        ConfigureServices(services);
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamAccountService = GetRequiredService<ISteamAccountService>();
        steamSessionService = GetRequiredService<ISteamSessionService>();
        configuration = GetRequiredService<IConfiguration>();

        SteamAuthenticator ??= new();
        SteamLoginState ??= await GetSteamLoginStateAsync(configuration, steamAccountService, GetRequiredService<ISteamSessionService>());

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
        if (IsCI())
            return;

        Assert.That(SteamAuthenticator, Is.Not.Null);
        Assert.That(SteamLoginState, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(SteamLoginState.AccessToken, Is.Not.Null);
            Assert.That(SteamLoginState.RefreshToken, Is.Not.Null);
        });

        enrollState.AccessToken = SteamLoginState.AccessToken;
        enrollState.RefreshToken = SteamLoginState.RefreshToken;
        enrollState.SteamId = (long)SteamLoginState.SteamId;

        await SteamAuthenticator.AddAuthenticatorAsync(enrollState);

        // 没有绑定手机号
        if (enrollState.NoPhoneNumber)
        {
            var result = await SteamAuthenticator.AddPhoneNumberAsync(enrollState, phone_number);

            // 需要邮箱手动确认手机号添加
            if (enrollState.RequiresEmailConfirmPhone)
            {
                await Task.Delay(TimeSpan.FromSeconds(15)); // waiting for confirm email
                result = await SteamAuthenticator.AddPhoneNumberAsync(enrollState, phone_number);
                Assert.That(enrollState.RequiresEmailConfirmPhone == false && result == null);
            }

            await SteamAuthenticator.AddAuthenticatorAsync(enrollState);
        }

        // 是否激活令牌
        if (enrollState.RequiresActivation)
        {
            // 输入手机收到的验证码
            enrollState.ActivationCode = "7C86B";
            var finalize = await SteamAuthenticator.FinalizeAddAuthenticatorAsync(enrollState);
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
        if (IsCI())
            return;

        Assert.That(SteamAuthenticator, Is.Not.Null);
        Assert.That(SteamLoginState, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(SteamLoginState.AccessToken, Is.Not.Null);
            Assert.That(SteamLoginState.RefreshToken, Is.Not.Null);
        });
        enrollState.AccessToken ??= SteamLoginState.AccessToken;
        enrollState.RefreshToken ??= SteamLoginState.RefreshToken;
        enrollState.SteamId = (long)SteamLoginState.SteamId;

        // 开始移除，注意手机验证码
        var removeStart_result = await SteamAuthenticator.RemoveAuthenticatorViaChallengeStartSync(enrollState.SteamId.ToString());

        Assert.That(removeStart_result);

        // 手机验证码
        var phoneVerifyCode = "7C86B";
        var removeContinue_result = await SteamAuthenticator.RemoveAuthenticatorViaChallengeContinueSync(enrollState.SteamId.ToString(), phoneVerifyCode);

        Assert.That(removeContinue_result);

        Assert.That(await Update());
    }

    /// <summary>
    /// 解绑（移除） 令牌 ，危险操作请手动修改再测试
    /// </summary>
    /// <param name="scheme">1 = 移除令牌验证器但保留邮箱验证，2 = 移除所有防护</param>
    /// <returns></returns>
    public async Task UnBindingAuthenticator(int scheme)
    {
        if (IsCI())
            return;

        Assert.That(SteamAuthenticator, Is.Not.Null);
        Assert.That(SteamLoginState, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(SteamLoginState.AccessToken, Is.Not.Null);
            Assert.That(SteamLoginState.RefreshToken, Is.Not.Null);

            Assert.That(!string.IsNullOrEmpty(SteamAuthenticator.RecoveryCode));
        });

        var remove_result = await SteamAuthenticator.RemoveAuthenticatorAsync(SteamLoginState.SteamId.ToString(), scheme);

        Assert.That(remove_result);

        Assert.That(await DeleteSteamAuthenticatorAsync());
    }

    /// <summary>
    /// 获取用户国家
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetUserCountry()
    {
        if (IsCI())
            return;

        Assert.That(SteamAuthenticator, Is.Not.Null);
        Assert.That(SteamLoginState, Is.Not.Null);

        var country = await SteamAuthenticator.GetUserCountry(SteamLoginState.SteamId.ToString());
        Assert.That(string.IsNullOrEmpty(country));
    }

    private async Task<bool> Update()
    {
        Assert.That(SteamAuthenticator, Is.Not.Null);
        Assert.That(SteamLoginState, Is.Not.Null);

        var steamData = SystemTextJsonSerializer.Deserialize<SteamConvertSteamDataJsonStruct>(SteamAuthenticator.SteamData!);

        var session = steamSessionService.RentSession(steamData.ThrowIsNull().SteamId.ToString());
        session ??= new();
        session.AccessToken = SteamLoginState.AccessToken.ThrowIsNull();
        session.RefreshToken = SteamLoginState.RefreshToken.ThrowIsNull();
        session.IdentitySecret = steamData.IdentitySecret;
        session.ServerTimeDiff = SteamAuthenticator.ServerTimeDiff;
        steamSessionService.AddOrSetSession(session);

        return await SaveSteamAuthenticatorAsync(SteamAuthenticator);
    }
}
