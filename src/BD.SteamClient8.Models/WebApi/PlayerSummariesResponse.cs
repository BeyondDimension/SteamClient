namespace BD.SteamClient8.Models.WebApi;

public sealed record PlayerSummariesResponse
{
    [NewtonsoftJsonProperty("response")]
    [SystemTextJsonProperty("response")]
    public required PlayerSummariesDetail Response { get; set; }

    public sealed record PlayerSummariesDetail
    {
        [NewtonsoftJsonProperty("players")]
        [SystemTextJsonProperty("players")]
        public required List<PlayerSummaries> Players { get; set; }
    }
}
