namespace BD.SteamClient.Models;

public sealed class TransferParameters : JsonModel
{
    [JsonPropertyName("steamid")]
    public string? Steamid { get; set; }

    [JsonPropertyName("token_secure")]
    public string? TokenSecure { get; set; }

    [JsonPropertyName("auth")]
    public string? Auth { get; set; }

    [JsonPropertyName("remember_login")]
    public bool RememberLogin { get; set; }

    [JsonPropertyName("webcookie")]
    public string? Webcookie { get; set; }
}