namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamTradeServiceImpl"/> 单元测试
/// </summary>
sealed class SteamTradeServiceTest : ServiceTestBase
{
    SteamLoginState steamLoginState = null!;
    ISteamTradeService steamTradeService = null!;
    ISteamAccountService steamAccountService = null!;
    IConfiguration configuration = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamAccountService();
        services.AddSteamIdleCardService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamTradeService = GetRequiredService<ISteamTradeService>();
        steamAccountService = GetRequiredService<ISteamAccountService>();
        configuration = GetRequiredService<IConfiguration>();

        steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService, GetRequiredService<ISteamSessionService>());
    }
}
