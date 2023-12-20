// ReSharper disable once CheckNamespace
namespace WinAuth.Services.Implementation;

#pragma warning disable SA1600 // Elements should be documented

public sealed partial class HOTPNetService : WebApiClientFactoryService, IHOTPNetService
{
    private const string Tag = "HOTPNetService";

    protected override string ClientName => Tag;

    public HOTPNetService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(Tag), serviceProvider)
    {
    }
}
