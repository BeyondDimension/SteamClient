namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 本机 Dlss 库
/// </summary>
public record class LocalDlssDll : IComparable<LocalDlssDll>
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// 版本信息
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// 版本号
    /// </summary>
    public ulong VersionNumber { get; }

    /// <summary>
    /// <see cref="SHA1"/> Hash 字符串
    /// </summary>
    public string SHA1Hash { get; }

    /// <summary>
    /// <see cref="MD5"/> Hash 字符串
    /// </summary>
    public string MD5Hash { get; }

    /// <summary>
    /// 通过文件构造实例
    /// </summary>
    /// <param name="filename"></param>
    public LocalDlssDll(string filename)
    {
        Filename = filename;

        var versionInfo = FileVersionInfo.GetVersionInfo(filename);

        Version = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";
        VersionNumber = ((ulong)versionInfo.FileMajorPart << 48) +
                     ((ulong)versionInfo.FileMinorPart << 32) +
                     ((ulong)versionInfo.FileBuildPart << 16) +
                     ((ulong)versionInfo.FilePrivatePart);

        using (var stream = File.OpenRead(filename))
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                MD5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }

            stream.Position = 0;

            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(stream);
                SHA1Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }
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
