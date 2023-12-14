namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public class SteamMobileConfGetListJsonStruct
{
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    [SystemTextJsonProperty("needauth")]
    public bool NeedAuth { get; set; }

    [SystemTextJsonProperty("conf")]
    public SteamMobileTradeConf[]? Conf { get; set; }
}

public class SteamMobileTradeConf
{
    [SystemTextJsonProperty("type")]
    public int Type { get; set; }

    [SystemTextJsonProperty("type_name")]
    public string TypeName { get; set; } = string.Empty;

    [SystemTextJsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [SystemTextJsonProperty("creator_id")]
    public string CreatorId { get; set; } = string.Empty;

    [SystemTextJsonProperty("nonce")]
    public string Nonce { get; set; } = string.Empty;

    [SystemTextJsonProperty("creation_time")]
    public long CreationTime { get; set; }

    [SystemTextJsonProperty("cancel")]
    public string Cancel { get; set; } = string.Empty;

    [SystemTextJsonProperty("accept")]
    public string Accept { get; set; } = string.Empty;

    [SystemTextJsonProperty("icon")]
    public string Icon { get; set; } = string.Empty;

    [SystemTextJsonProperty("multi")]
    public bool Multi { get; set; }

    [SystemTextJsonProperty("headline")]
    public string Headline { get; set; } = string.Empty;

    [SystemTextJsonProperty("summary")]
    public string[]? Summary { get; set; }

    [SystemTextJsonProperty("warn")]
    public string[]? Warn { get; set; }
}