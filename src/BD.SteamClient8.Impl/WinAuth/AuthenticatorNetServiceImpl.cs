using BD.Common8.Http.ClientFactory.Services;
using BD.SteamClient8.WinAuth.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace BD.SteamClient8.Services.WinAuth;

/// <summary>
/// Initializes a new instance of the <see cref="AuthenticatorNetServiceImpl"/> class.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
sealed partial class AuthenticatorNetServiceImpl(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG), serviceProvider), IAuthenticatorNetService
{
    const string TAG = "AuthenticatorNetService";

    /// <inheritdoc/>
    protected override string ClientName => TAG;
}
