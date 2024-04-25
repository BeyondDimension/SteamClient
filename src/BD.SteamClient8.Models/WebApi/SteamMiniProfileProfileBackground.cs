namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 小型用户个人资料视频
/// </summary>
[MP2Obj]
public partial record class SteamMiniProfileProfileBackground
{
    /// <summary>
    /// Webm 格式
    /// </summary>
    [NewtonsoftJsonProperty("video/webm")]
    [SystemTextJsonProperty("video/webm")]
    public string? VideoWebm { get; set; }

    /// <summary>
    /// Mp4 格式
    /// </summary>
    [NewtonsoftJsonProperty("video/mp4")]
    [SystemTextJsonProperty("video/mp4")]
    public string? VideoMp4 { get; set; }
}
