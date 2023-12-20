// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

#pragma warning disable SA1600 // Elements should be documented

public static class AuthenticatorNetService
{
    private static readonly IServiceProvider _serviceProvider = Ioc.Get<IServiceProvider>();
    private static readonly ILoggerFactory _loggerFactory = Ioc.Get<ILoggerFactory>();

    public static BattleNetService BattleNet = new BattleNetService(_serviceProvider, _loggerFactory);

    public static GoogleNetService Google = new GoogleNetService();

    public static HOTPNetService HOTP = new HOTPNetService(_serviceProvider, _loggerFactory);

    public static MicrosoftNetService Microsoft = new MicrosoftNetService(_serviceProvider, _loggerFactory);
}
