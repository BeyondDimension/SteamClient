namespace BD.SteamClient8.Models.WebApi.Authenticator;

/// <summary>
/// 获取确认消息返回结果
/// </summary>
public class SteamMobileConfGetListJsonStruct
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 是否需要认证
    /// </summary>
    [SystemTextJsonProperty("needauth")]
    public bool NeedAuth { get; set; }

    /// <summary>
    /// 确认消息列表
    /// </summary>
    [SystemTextJsonProperty("conf")]
    public SteamMobileTradeConf[]? Conf { get; set; }
}

/// <summary>
/// 确认消息详情
/// </summary>
public class SteamMobileTradeConf
{
    /// <summary>
    /// 确认类型
    /// </summary>
    [SystemTextJsonProperty("type")]
    public int Type { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    [SystemTextJsonProperty("type_name")]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 确定消息 Id
    /// </summary>
    [SystemTextJsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 创建 Id
    /// </summary>
    [SystemTextJsonProperty("creator_id")]
    public string CreatorId { get; set; } = string.Empty;

    /// <summary>
    /// 随机数
    /// </summary>
    [SystemTextJsonProperty("nonce")]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [SystemTextJsonProperty("creation_time")]
    public long CreationTime { get; set; }

    /// <summary>
    /// 取消
    /// </summary>
    [SystemTextJsonProperty("cancel")]
    public string Cancel { get; set; } = string.Empty;

    /// <summary>
    /// 允许
    /// </summary>
    [SystemTextJsonProperty("accept")]
    public string Accept { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    [SystemTextJsonProperty("icon")]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否多个
    /// </summary>
    [SystemTextJsonProperty("multi")]
    public bool Multi { get; set; }

    /// <summary>
    /// 令牌的标题或摘要
    /// </summary>
    [SystemTextJsonProperty("headline")]
    public string Headline { get; set; } = string.Empty;

    /// <summary>
    /// 汇总
    /// </summary>
    [SystemTextJsonProperty("summary")]
    public string[]? Summary { get; set; }

    /// <summary>
    /// 警告信息
    /// </summary>
    [SystemTextJsonProperty("warn")]
    public string[]? Warn { get; set; }
}