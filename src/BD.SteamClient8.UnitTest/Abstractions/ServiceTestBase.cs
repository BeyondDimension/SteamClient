namespace BD.SteamClient8.UnitTest.Abstractions;

/// <summary>
/// 服务的单元测试基类
/// </summary>
abstract class ServiceTestBase
{
    IServiceProvider? serviceProvider;

    /// <summary>
    /// 依赖注入服务提供程序
    /// </summary>
    protected IServiceProvider ServiceProvider
    {
        get => serviceProvider.ThrowIsNull();
        private set => serviceProvider = value;
    }

    /// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/>
    protected T GetRequiredService<T>() where T : notnull
        => serviceProvider.ThrowIsNull().GetRequiredService<T>();

    /// <inheritdoc cref="SetUpAttribute"/>
    public virtual ValueTask Setup()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
        return default;
    }

    /// <summary>
    /// 配置依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        ConfigurationBuilder builder = new();
        ConfigureConfiguration(builder);
        IConfigurationRoot configurationRoot = builder.Build();
        services.AddSingleton(configurationRoot);
        services.AddSingleton<IConfiguration>(configurationRoot);

        services.AddLogging(ConfigureLogging);
        services.TryAddHttpPlatformHelper();
        services.AddFusilladeHttpClientFactory();
    }

    /// <summary>
    /// 配置 <see cref="IConfiguration"/>
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        builder.AddUserSecrets(GetType().Assembly);
    }

    /// <summary>
    /// 配置日志
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void ConfigureLogging(ILoggingBuilder builder)
    {
        builder.AddConsole();
    }
}
