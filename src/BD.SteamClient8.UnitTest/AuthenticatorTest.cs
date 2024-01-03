using BD.Common8.Extensions;
using static BD.SteamClient8.Models.SteamAuthenticator;

namespace BD.SteamClient8.UnitTest;

/// <summary>
/// 令牌功能测试
/// </summary>
sealed class AuthenticatorTest : ServiceTestBase
{
    ISteamAccountService steamAccountService = null!;
    ISteamSessionService steamSessionService = null!;
    IConfiguration configuration = null!;

    SteamAuthenticator.EnrollState enrollState;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamAccountService = GetRequiredService<ISteamAccountService>();
        steamSessionService = GetRequiredService<ISteamSessionService>();
        configuration = GetRequiredService<IConfiguration>();

        enrollState ??= new();
    }

    /// <summary>
    /// 添加令牌
    /// </summary>
    /// <returns></returns>
    [Test]
    [TestCase("18112341234")]
    public async Task AddAuthenticator(string phone_number)
    {
        if (IsCI())
            return;

        var steamAuthenticator = await GetSteamAuthenticatorAsync() ?? new();
        var steamLoginState = await GetSteamLoginStateAsync();

        Assert.Multiple(() =>
        {
            Assert.That(steamAuthenticator, Is.Not.Null);
            Assert.That(steamLoginState, Is.Not.Null);
        });
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
            var result = await AddPhoneNumberAsync(enrollState, phone_number);

            // 需要邮箱手动确认手机号添加
            if (enrollState.RequiresEmailConfirmPhone)
            {
                await Task.Delay(TimeSpan.FromSeconds(15)); // waiting for confirm email
                result = await AddPhoneNumberAsync(enrollState, phone_number);
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
            Assert.Multiple(async () =>
            {
                Assert.That(finalize.IsSuccess && finalize.Content);
                Assert.That(await Update());
            });
        }

        TestContext.WriteLine("OK");
    }

    /// <summary>
    /// 已有令牌 替换新令牌
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task RemoveAuthenticator()
    {
        if (IsCI())
            return;

        var steamAuthenticator = await GetSteamAuthenticatorAsync() ?? new();
        var steamLoginState = await GetSteamLoginStateAsync();

        Assert.Multiple(() =>
        {
            Assert.That(steamAuthenticator, Is.Not.Null);
            Assert.That(steamLoginState, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(steamLoginState.AccessToken, Is.Not.Null);
            Assert.That(steamLoginState.RefreshToken, Is.Not.Null);
        });
        enrollState.AccessToken ??= steamLoginState.AccessToken;
        enrollState.RefreshToken ??= steamLoginState.RefreshToken;
        enrollState.SteamId = (long)steamLoginState.SteamId;

        // 开始移除，注意手机验证码
        var removeStart_result = await RemoveAuthenticatorViaChallengeStartSync(enrollState.SteamId.ToString());

        Assert.That(removeStart_result.IsSuccess && removeStart_result.Content);

        // 手机验证码
        var phoneVerifyCode = "7C86B";
        var removeContinue_result = await steamAuthenticator.RemoveAuthenticatorViaChallengeContinueSync(enrollState.SteamId.ToString(), phoneVerifyCode);
        SteamAuthenticatorHelper.SteamAuthenticator ??= steamAuthenticator;
        Assert.Multiple(async () =>
        {
            Assert.That(removeContinue_result.IsSuccess && removeContinue_result.Content, removeContinue_result.GetMessage());
            Assert.That(await Update());
        });

        TestContext.WriteLine("OK");
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

        var steamAuthenticator = await GetSteamAuthenticatorAsync();
        var steamLoginState = await GetSteamLoginStateAsync();

        Assert.Multiple(() =>
        {
            Assert.That(steamAuthenticator, Is.Not.Null);
            Assert.That(steamLoginState, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(steamLoginState.AccessToken, Is.Not.Null);
            Assert.That(steamLoginState.RefreshToken, Is.Not.Null);

            Assert.That(!string.IsNullOrEmpty(steamAuthenticator.RecoveryCode));
        });

        var remove_result = await steamAuthenticator.RemoveAuthenticatorAsync(steamLoginState.SteamId.ToString(), scheme);

        Assert.Multiple(async () =>
        {
            Assert.That(remove_result.IsSuccess && remove_result.Content);
            Assert.That(await DeleteSteamAuthenticatorAsync());
        });
    }

    /// <summary>
    /// 获取用户国家或地区
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task UserCountryOrRegion()
    {
        if (IsCI())
            return;

        var steamAuthenticator = await GetSteamAuthenticatorAsync();
        var steamLoginState = await GetSteamLoginStateAsync();

        Assert.Multiple(() =>
        {
            Assert.That(steamAuthenticator, Is.Not.Null);
            Assert.That(steamLoginState, Is.Not.Null);
        });

        var userCountryOrRegionRsp = await GetUserCountryOrRegion(steamLoginState.SteamId.ToString());
        string? userCountryOrRegion = userCountryOrRegionRsp.Content;
        Assert.That(userCountryOrRegion, Is.Not.Empty);

        TestContext.WriteLine(userCountryOrRegion);
    }

    async Task<bool> Update()
    {
        var steamAuthenticator = await GetSteamAuthenticatorAsync();
        var steamLoginState = await GetSteamLoginStateAsync();

        Assert.Multiple(() =>
        {
            Assert.That(steamAuthenticator, Is.Not.Null);
            Assert.That(steamLoginState, Is.Not.Null);
        });

        var steamData = SystemTextJsonSerializer.Deserialize<SteamConvertSteamDataJsonStruct>(steamAuthenticator.SteamData!);

        var sessionRsp = await steamSessionService.RentSession(steamData.ThrowIsNull().SteamId.ToString());
        var session = sessionRsp.Content;
        session ??= new();
        session.AccessToken = steamLoginState.AccessToken.ThrowIsNull();
        session.RefreshToken = steamLoginState.RefreshToken.ThrowIsNull();
        session.IdentitySecret = steamData.IdentitySecret;
        session.ServerTimeDiff = steamAuthenticator.ServerTimeDiff;
        await steamSessionService.AddOrSetSession(session);

        return await SaveSteamAuthenticatorAsync(steamAuthenticator);
    }
}
