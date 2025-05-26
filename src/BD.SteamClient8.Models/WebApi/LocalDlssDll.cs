using BD.Common8.Models.Abstractions;
using System.Diagnostics;
using System.Extensions;
using System.Security.Cryptography;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 本机 DLSS 库
/// </summary>
public sealed record class LocalDlssDll : JsonRecordModel<LocalDlssDll>, IJsonSerializerContext, IComparable<LocalDlssDll>
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 文件名
    /// </summary>
    public string Filename { get; private set; } = "";

    /// <summary>
    /// 版本信息
    /// </summary>
    public string Version { get; private set; } = "";

    /// <summary>
    /// 版本号
    /// </summary>
    public ulong VersionNumber { get; private set; }

    /// <summary>
    /// <see cref="SHA1"/> Hash 字符串
    /// </summary>
    public string SHA1Hash { get; private set; } = "";

    /// <summary>
    /// <see cref="MD5"/> Hash 字符串
    /// </summary>
    public string MD5Hash { get; private set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalDlssDll"/> class.
    /// </summary>
    [global::MessagePack.SerializationConstructor, global::MemoryPack.MemoryPackConstructor, global::System.Text.Json.Serialization.JsonConstructor]
    public LocalDlssDll()
    {
    }

    public void SetFilename(string filename)
    {
        Filename = filename;

        var versionInfo = FileVersionInfo.GetVersionInfo(filename);

        Version = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";
        VersionNumber = ((ulong)versionInfo.FileMajorPart << 48) +
                     ((ulong)versionInfo.FileMinorPart << 32) +
                     ((ulong)versionInfo.FileBuildPart << 16) +
                     ((ulong)versionInfo.FilePrivatePart);
        using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var hash_md5 = MD5.HashData(stream);
        MD5Hash = hash_md5.ToHexString(isLower: false);

        stream.Position = 0;

        var hash_sha1 = SHA1.HashData(stream);
        SHA1Hash = hash_sha1.ToHexString(isLower: false);
    }

    /// <summary>
    /// 通过文件构造实例
    /// </summary>
    /// <param name="filename"></param>
    public LocalDlssDll(string filename)
    {
        SetFilename(filename);
    }

    /// <summary>
    /// 返回 dll 版本信息
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Version;
    }

    /// <summary>
    /// 版本比较
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(LocalDlssDll? other)
    {
        if (other == null)
        {
            return -1;
        }

        return other.VersionNumber.CompareTo(VersionNumber);
    }
}
