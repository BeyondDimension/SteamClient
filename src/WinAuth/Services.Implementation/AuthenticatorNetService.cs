// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

/// <summary>
/// 提供身份验证器服务
/// </summary>
public static class AuthenticatorNetService
{
    private static readonly IServiceProvider _serviceProvider = Ioc.Get<IServiceProvider>();
    private static readonly ILoggerFactory _loggerFactory = Ioc.Get<ILoggerFactory>();

    /// <summary>
    /// <see cref="BattleNetService"/> 服务实例
    /// </summary>
    public static BattleNetService BattleNet = new BattleNetService(_serviceProvider, _loggerFactory);

    /// <summary>
    /// <see cref="GoogleNetService"/> 服务实例
    /// </summary>
    public static GoogleNetService Google = new GoogleNetService();

    /// <summary>
    /// <see cref="HOTPNetService"/> 服务实例
    /// </summary>
    public static HOTPNetService HOTP = new HOTPNetService(_serviceProvider, _loggerFactory);

    /// <summary>
    /// <see cref="MicrosoftNetService"/> 服务实例
    /// </summary>
    public static MicrosoftNetService Microsoft = new MicrosoftNetService(_serviceProvider, _loggerFactory);
}
