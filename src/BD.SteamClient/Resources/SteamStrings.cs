namespace BD.SteamClient.Resources;

internal static class SteamStrings
{
    static readonly System.Resources.ResourceManager ResourceManager = new("BD.SteamClient.Resources.Strings", typeof(SteamStrings).Assembly);

    internal static string Get(string name) => ResourceManager.GetString(name) ?? throw new InvalidOperationException($"Missing Steam resource: {name}");

    internal static string Format(string name, params object?[] args) => string.Format(Get(name), args);
}
