#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Enums;

/// <summary>
/// HMAC 哈希算法类型
/// </summary>
public enum HMACTypes
{
    /// <inheritdoc cref="HMACSHA1"/>
    SHA1 = 0,

    /// <inheritdoc cref="HMACSHA256"/>
    SHA256 = 1,

    /// <inheritdoc cref="HMACSHA512"/>
    SHA512 = 2,
}