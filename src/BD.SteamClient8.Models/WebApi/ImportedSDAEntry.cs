using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 导入 SDA 实体
/// </summary>
public sealed record class ImportedSDAEntry : JsonRecordModel<ImportedSDAEntry>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

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
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public string? json;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Username + " (" + SteamId + ")";
    }
}