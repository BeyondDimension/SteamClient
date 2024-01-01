#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

[MPObj, MP2Obj(MP2SerializeLayout.Explicit)]
public partial record class DisableAuthorizedDevice
{
    /// <summary>
    /// Steam ID3 格式 Id
    /// </summary>
    [MPKey(0), MP2Key(0)]
    public long SteamId3_Int { get; set; }

    /// <summary>
    /// 使用时间
    /// </summary>
    [MPKey(1), MP2Key(1)]
    public long Timeused { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MPKey(2), MP2Key(2)]
    public string? Description { get; set; }

    /// <summary>
    /// Token唯一标识符
    /// </summary>
    [MPKey(3), MP2Key(3)]
    public string? Tokenid { get; set; }
}
