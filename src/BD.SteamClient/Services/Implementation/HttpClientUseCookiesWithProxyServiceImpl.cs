namespace BD.SteamClient.Services;

public abstract class HttpClientUseCookiesWithProxyServiceImpl : HttpClientUseCookiesServiceImpl
{
    public sealed class DynamicWebProxy : IWebProxy
    {
        public IWebProxy InnerProxy { get; set; } = HttpNoProxy.Instance;

        public ICredentials? Credentials
        {
            get => InnerProxy.Credentials;
            set => InnerProxy.Credentials = value;
        }

        public Uri? GetProxy(Uri destination) => InnerProxy.GetProxy(destination);

        public bool IsBypassed(Uri host) => InnerProxy.IsBypassed(host);
    }

    public interface IAppSettings
    {
        string? WebProxyAddress { get; set; }

        int? WebProxyPort { get; set; }

        string? WebProxyUserName { get; set; }

        string? WebProxyPassword { get; set; }
    }
}
