namespace BD.SteamClient.Enums;

public enum TradeTag : byte
{
    [Description("conf")]
    CONF = 1,

    [Description("details")]
    DETAILS = 2,

    [Description("allow")]
    ALLOW = 3,

    [Description("cancel")]
    CANCEL = 4
}
