using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.SteamClient.Models.Trade;
public sealed class Asset
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("contextid")]
    public string ContextId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("assetid")]
    public string AssetId { get; set; } = string.Empty;
}
