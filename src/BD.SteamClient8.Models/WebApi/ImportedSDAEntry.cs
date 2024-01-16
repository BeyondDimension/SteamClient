#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 导入 SDA 实体
/// </summary>
public sealed record class ImportedSDAEntry
{
    /// <summary>
    /// PBKDF2_ITERATIONS
    /// </summary>
    public const int PBKDF2_ITERATIONS = 50000;

    /// <summary>
    /// SALT_LENGTH
    /// </summary>
    public const int SALT_LENGTH = 8;

    /// <summary>
    /// KEY_SIZE_BYTES
    /// </summary>
    public const int KEY_SIZE_BYTES = 32;

    /// <summary>
    /// IV_LENGTH
    /// </summary>
    public const int IV_LENGTH = 16;

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username;

    /// <summary>
    /// SteamId
    /// </summary>
    public string? SteamId;

    /// <summary>
    /// json text
    /// </summary>
    public string? json;

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Username + " (" + SteamId + ")";
    }
}