namespace BD.SteamClient8.UnitTest.Abstractions;

/// <summary>
/// 服务的单元测试基类
/// </summary>
abstract class ServiceTestBase
{
    static IServiceProvider? serviceProvider;

    /// <summary>
    /// Steam 登录状态
    /// </summary>
    protected static SteamLoginState SteamLoginState => SteamLoginStateHelper.SteamLoginState;

    /// <summary>
    /// 依赖注入服务提供程序
    /// </summary>
    protected static IServiceProvider ServiceProvider
    {
        get => serviceProvider.ThrowIsNull();
        private set => serviceProvider = value;
    }

    /// <summary>
    /// <see cref="GetInventories(ISteamAccountService, ulong, string, string)"/> 获取的用户库存，访问有频率限制
    /// </summary>
    protected static InventoryPageResponse? InventoryPageResponse { get; set; }

    static readonly AsyncExclusiveLock lock_Setup = new();
    static readonly AsyncExclusiveLock lock_GetInventoryPageResponseAsync = new();

    /// <summary>
    /// 获取用户库存信息
    /// </summary>
    /// <param name="steamAccountService"></param>
    /// <param name="steam_id"></param>
    /// <param name="app_id"></param>
    /// <param name="context_id"></param>
    /// <returns></returns>
    public virtual async ValueTask GetInventories(ISteamAccountService steamAccountService, ulong steam_id, string app_id, string context_id)
    {
        using (await lock_GetInventoryPageResponseAsync.AcquireLockAsync(CancellationToken.None))
        {
            if (InventoryPageResponse is null)
            {
                var response = await steamAccountService.GetInventories(steam_id, app_id, context_id, 50);
                InventoryPageResponse = response.Content;
            }
        }
    }

    /// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/>
    protected static T GetRequiredService<T>() where T : notnull
        => serviceProvider.ThrowIsNull().GetRequiredService<T>();

    static bool IsInit;

    /// <inheritdoc cref="SetUpAttribute"/>
    public virtual async ValueTask Setup()
    {
        using (await lock_Setup.AcquireLockAsync(CancellationToken.None))
        {
            if (IsInit)
                return;

            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            Ioc.ConfigureServices(serviceProvider);
            IsInit = true;
        }
    }

    /// <summary>
    /// 配置依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="needLoginState"></param>
    static void ConfigureServices(IServiceCollection services)
    {
        ConfigurationBuilder builder = new();
        ConfigureConfiguration(builder);
        IConfigurationRoot configurationRoot = builder.Build();
        services.AddSingleton(configurationRoot);
        services.AddSingleton<IConfiguration>(configurationRoot);

        services.AddLogging(ConfigureLogging);
        services.TryAddHttpPlatformHelper();
        services.AddFusilladeHttpClientFactory();

        services.AddSteamAccountService();
        services.AddSteamAuthenticatorService();
        services.AddSteamIdleCardService();
        services.AddSteamMarketService();
        services.AddSteamTradeService();
#if !(IOS || ANDROID)
        services.AddSingleton<ISteamService, TestSteamServiceImpl>();
        services.TryAddSteamworksLocalApiService();
#endif
        services.AddSteamDbWebApiService();
        services.AddSteamworksWebApiService();
        services.AddSteamGridDBWebApiService();

        services.AddAuthenticatorNetService();
    }

    /// <summary>
    /// 配置 <see cref="IConfiguration"/>
    /// </summary>
    /// <param name="builder"></param>
    static void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        builder.AddUserSecrets(typeof(ServiceTestBase).Assembly);
    }

    /// <summary>
    /// 配置日志
    /// </summary>
    /// <param name="builder"></param>
    static void ConfigureLogging(ILoggingBuilder builder)
    {
        builder.AddConsole();
    }

#if !(IOS || ANDROID)
    sealed class TestSteamServiceImpl(ILoggerFactory loggerFactory) : SteamServiceImpl(loggerFactory)
    {
        public override string? SteamLanguageString => default;

        protected override string? StratSteamDefaultParameter => default;

        protected override bool IsRunSteamAdministrator => default;

        protected override Dictionary<uint, string?>? HideGameList => default;
    }
#endif
}
