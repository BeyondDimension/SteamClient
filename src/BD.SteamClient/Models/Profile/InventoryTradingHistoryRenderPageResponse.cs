namespace BD.SteamClient;

public class InventoryTradeHistoryRenderPageResponse
{
    public bool Success { get; set; }

    public string Html { get; set; } = string.Empty;

    public int Num { get; set; }

    public IEnumerable<InventoryTradeHistoryApp> Apps { get; set; } = Enumerable.Empty<InventoryTradeHistoryApp>();

    public JsonElement Descriptions { get; set; }

    public InventoryTradeHistoryCursor? Cursor { get; set; }

    public bool Next => Success && Cursor != null;

    public record struct InventoryTradeHistoryCursor
    {
        [S_JsonProperty("time")]
        public long Time { get; set; }

        [S_JsonProperty("time_frac")]
        public long TimeFrac { get; set; }

        [S_JsonProperty("s")]
        public string S { get; set; }
    }

    public record struct InventoryTradeHistoryApp
    {
        public int AppId { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string Link { get; set; }
    }
}

public record struct InventoryTradeHistoryRow
{
    public DateTime Date { get; set; }

    public TimeSpan TimeOfDate { get; set; }

    public readonly DateTime? DateTime => Date != default && TimeOfDate != default ? Date.Add(TimeOfDate) : default;

    public string Desc { get; set; }

    public IEnumerable<InventoryTradeHistoryGroup> Groups { get; set; }
}

public record struct InventoryTradeHistoryGroup
{
    public string PlusMinus { get; set; }

    public IEnumerable<InventoryTradeHistoryItem> Items { get; set; }
}

public record struct InventoryTradeHistoryItem
{
    public string AppId { get; set; }

    public string ClassId { get; set; }

    public string ContextId { get; set; }

    public string InstanceId { get; set; }

    public string Amount { get; set; }

    public string ItemName { get; set; }

    public string ProfilePreviewPageUrl { get; set; }

    public string ItemImgUrl { get; set; }
}
