// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

public sealed partial class MicrosoftNetService : WebApiClientFactoryService, IMicrosoftNetService
{
    private const string Tag = "MicrosoftNetService";

    /// <inheritdoc/>
    protected override string ClientName => Tag;

    /// <summary>
    /// 初始化 <see cref="MicrosoftNetService"/> 类的新实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public MicrosoftNetService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(Tag), serviceProvider)
    {
    }
}
