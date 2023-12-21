// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

public sealed partial class HOTPNetService : WebApiClientFactoryService, IHOTPNetService
{
    private const string Tag = "HOTPNetService";

    /// <inheritdoc/>
    protected override string ClientName => Tag;

    /// <summary>
    /// 初始化 <see cref="HOTPNetService"/> 类的新实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public HOTPNetService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(Tag), serviceProvider)
    {
    }
}
