using BD.SteamClient8.WinAuth.Enums;
using BD.SteamClient8.WinAuth.Extensions;

namespace BD.SteamClient8.WinAuth.Models;

/// <summary>
/// 身份验证器(游戏平台令牌)数据轻量化导出模型
/// </summary>
[global::MessagePack.MessagePackObject, global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class AuthenticatorExportModel
{
    /// <summary>
    /// 默认期限为 30s
    /// </summary>
    public const int DEFAULT_PERIOD = 30;

    public const HMACTypes DEFAULT_HMAC_TYPE = HMACTypes.SHA1;

    /// <summary>
    /// 平台
    /// </summary>
    [global::MessagePack.Key(0), global::MemoryPack.MemoryPackOrder(0)]
    public AuthenticatorPlatform Platform { get; set; }

    /// <summary>
    /// 颁发者
    /// </summary>
    [global::MessagePack.Key(1), global::MemoryPack.MemoryPackOrder(1)]
    public string? Issuer { get; set; }

    /// <summary>
    /// HMAC 加密类型
    /// </summary>
    [global::MessagePack.Key(2), global::MemoryPack.MemoryPackOrder(2)]
    public HMACTypes HMACType { get; set; }

    /// <summary>
    /// BattleNet(战网) 序列号
    /// </summary>
    [global::MessagePack.Key(3), global::MemoryPack.MemoryPackOrder(3)]
    public string? Serial { get; set; }

    /// <summary>
    /// Steam 设备唯一标识
    /// </summary>
    [global::MessagePack.Key(4), global::MemoryPack.MemoryPackOrder(4)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// Steam 相关数据
    /// </summary>
    [global::MessagePack.Key(5), global::MemoryPack.MemoryPackOrder(5)]
    public string? SteamData { get; set; }

    /// <summary>
    /// HOTP 计数器
    /// </summary>
    [global::MessagePack.Key(6), global::MemoryPack.MemoryPackOrder(6)]
    public long Counter { get; set; }

    /// <summary>
    /// 下一个代码的周期（秒）
    /// </summary>
    [global::MessagePack.Key(7), global::MemoryPack.MemoryPackOrder(7)]
    public int Period { get; set; }

    /// <summary>
    /// 密钥信息
    /// </summary>
    [global::MessagePack.Key(8), global::MemoryPack.MemoryPackOrder(8)]
    public byte[]? SecretKey { get; set; }

    /// <summary>
    /// 代码中返回的位数（默认为 6）
    /// </summary>
    [global::MessagePack.Key(9), global::MemoryPack.MemoryPackOrder(9)]
    public int CodeDigits { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [global::MessagePack.Key(10), global::MemoryPack.MemoryPackOrder(10)]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public sealed override string ToString() => this.ToUrl();
}
