using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Constants;

namespace BD.SteamClient8.Models.WebApi;

public sealed record class AuthorizedDevice : JsonRecordModel<AuthorizedDevice>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disable { get; set; }

    /// <summary>
    /// 用户个人资料 Url
    /// </summary>
    public string ProfileUrl => string.Format(SteamApiUrls.STEAM_PROFILES_URL, SteamId64_Int);

    /// <summary>
    /// 第一项
    /// </summary>
    public bool First { get; set; }

    /// <summary>
    /// 最后一项
    /// </summary>
    public bool End { get; set; }

    /// <summary>
    /// 索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// SteamId3 格式
    /// </summary>
    public long SteamId3_Int { get; set; }

    /// <summary>
    /// SteamId64 格式
    /// </summary>
    public long SteamId64_Int => SteamIdConvert.UndefinedId + SteamId3_Int;

    /// <summary>
    /// 在线状态
    /// </summary>
    public string? OnlineState { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// SteamId
    /// </summary>
    public string? SteamID { get; set; }

    /// <summary>
    /// 展示名称
    /// </summary>
    public string? ShowName { get; set; }

    /// <summary>
    /// 用户小型简介
    /// </summary>
    public SteamMiniProfile? MiniProfile { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? SteamNickName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? AccountName { get; set; }

    public long Timeused { get; set; }

    public DateTime TimeusedTime => Timeused.ToDateTimeS();

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// TokenId
    /// </summary>
    public string? Tokenid { get; set; }

    /// <summary>
    /// 头像图标
    /// </summary>
    public string? AvatarIcon { get; set; }

    /// <summary>
    /// 头像（中）
    /// </summary>
    public string? AvatarMedium { get; set; }
}
