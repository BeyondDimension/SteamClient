namespace BD.SteamClient.Models;

/// <summary>
/// A Steam login challenge, never a destination for an authenticated HTTP request.
/// </summary>
public sealed record SteamLoginQrChallenge(int Version, ulong ClientId)
{
    /// <summary>
    /// Accepts only HTTPS s.team login challenges. Versions must fit the protocol's
    /// unsigned 16-bit signature field; query strings, fragments and trailing slashes are rejected.
    /// </summary>
    public static bool TryParse(string? value, out SteamLoginQrChallenge? challenge)
    {
        challenge = null;
        const string authority = "https://s.team";
        if (value is null || !value.StartsWith(authority, StringComparison.OrdinalIgnoreCase))
            return false;

        var path = value.AsSpan(authority.Length);
        if (path.StartsWith(":443/q/", StringComparison.Ordinal))
            path = path[7..];
        else if (path.StartsWith("/q/", StringComparison.Ordinal))
            path = path[3..];
        else
            return false;

        var separator = path.IndexOf('/');
        if (separator <= 0 || separator == path.Length - 1)
            return false;
        for (var i = 0; i < path.Length; i++)
        {
            if (i != separator && (path[i] < '0' || path[i] > '9'))
                return false;
        }

        if (!ushort.TryParse(path[..separator], NumberStyles.None, CultureInfo.InvariantCulture, out var version)
            || version == 0
            || !ulong.TryParse(path[(separator + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var clientId)
            || clientId == 0)
            return false;

        challenge = new(version, clientId);
        return true;
    }
}
