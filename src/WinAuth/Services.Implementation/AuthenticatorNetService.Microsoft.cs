// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

#pragma warning disable SA1600 // Elements should be documented

public sealed partial class MicrosoftNetService : WebApiClientFactoryService, IMicrosoftNetService
{
    private const string Tag = "MicrosoftNetService";

    protected override string ClientName => Tag;

    public MicrosoftNetService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(Tag), serviceProvider)
    {
    }
}
