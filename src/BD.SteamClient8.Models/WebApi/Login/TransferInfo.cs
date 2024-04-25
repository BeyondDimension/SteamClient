namespace BD.SteamClient8.Models;

/// <summary>
/// 跳转信息
/// </summary>
public sealed class TransferInfo : JsonModel<TransferInfo>
{
    /// <summary>
    /// 跳转域名地址
    /// </summary>
    [SystemTextJsonProperty("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 跳转参数
    /// </summary>
    [SystemTextJsonProperty("params")]
    public TransferInfoParams? Params { get; set; }
}
