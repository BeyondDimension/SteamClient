#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

namespace BD.WTTS.Models;

/// <summary>
/// 令牌数据轻量化导出模型
/// </summary>
[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class AuthenticatorExportDTO
{
    /// <summary>
    /// 平台
    /// </summary>
    [MPKey(0), MP2Key(0)]
    public AuthenticatorPlatform Platform { get; set; }

    /// <summary>
    /// 颁发者
    /// </summary>
    [MPKey(1), MP2Key(1)]
    public string? Issuer { get; set; }

    /// <summary>
    /// HMAC 加密类型
    /// </summary>
    [MPKey(2), MP2Key(2)]
    public HMACTypes HMACType { get; set; }

    /// <summary>
    /// BattleNet(战网) 序列号
    /// </summary>
    [MPKey(3), MP2Key(3)]
    public string? Serial { get; set; }

    /// <summary>
    /// Steam 设备唯一标识
    /// </summary>
    [MPKey(4), MP2Key(4)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// Steam 相关数据
    /// </summary>
    [MPKey(5), MP2Key(5)]
    public string? SteamData { get; set; }

    /// <summary>
    /// HOTP 计数器
    /// </summary>
    [MPKey(6), MP2Key(6)]
    public long Counter { get; set; }

    /// <summary>
    /// 下一个代码的周期（秒）
    /// </summary>
    [MPKey(7), MP2Key(7)]
    public int Period { get; set; }

    /// <summary>
    /// 密钥信息
    /// </summary>
    [MPKey(8), MP2Key(8)]
    public byte[]? SecretKey { get; set; }

    /// <summary>
    /// 代码中返回的位数（默认为 6）
    /// </summary>
    [MPKey(9), MP2Key(9)]
    public int CodeDigits { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [MPKey(10), MP2Key(10)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 内部 ToString 重写
    /// </summary>
    /// <returns></returns>
    public override string ToString() => this.ToUrl();
}