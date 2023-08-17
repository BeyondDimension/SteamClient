namespace BD.SteamClient;

public record struct LoginHistoryItem
{
    public string LogInDateTime { get; set; }

    public string? LogOutDateTime { get; set; }

    public int OsType { get; set; }

    public string CountryOrRegion { get; set; }

    public string City { get; set; }

    public string State { get; set; }
}
