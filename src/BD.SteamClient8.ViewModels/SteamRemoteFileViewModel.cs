namespace BD.SteamClient8.ViewModels;

/// <summary>
/// <see cref="SteamRemoteFile"/> 模型绑定类型
/// </summary>
[ViewModelWrapperGenerated(typeof(SteamRemoteFile))]
public partial class SteamRemoteFileViewModel
{
    /// <summary>
    /// 获取文件名
    /// </summary>
    public string Name { get => Model.Name; }

    /// <inheritdoc cref="ISteamworksLocalApiService.FileRead(string, byte[], CancellationToken)"/>
    public int Read(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileRead(Name, buffer).GetAwaiter().GetResult()?.Content ?? default;
    }

    /// <summary>
    /// 读取文件的所有字节
    /// </summary>
    public byte[] ReadAllBytes()
    {
        byte[] buffer = new byte[Size];
        int read = Read(buffer);
        if (read != buffer.Length)
            throw new IOException("Could not read entire file.");
        return buffer;
    }

    /// <inheritdoc cref="ISteamworksLocalApiService.FileWrite(string, byte[])"/>
    public bool WriteAllBytes(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileWrite(Name, buffer).GetAwaiter().GetResult()?.IsSuccess ?? default;
    }

    /// <inheritdoc cref="ISteamworksLocalApiService.FileForget(string)"/>
    public bool Forget()
    {
        return ISteamworksLocalApiService.Instance.FileForget(Name).GetAwaiter().GetResult()?.IsSuccess ?? default;
    }

    /// <inheritdoc cref="ISteamworksLocalApiService.FileDelete(string)"/>
    public bool Delete()
    {
        return ISteamworksLocalApiService.Instance.FileDelete(Name).GetAwaiter().GetResult()?.IsSuccess ?? default;
    }
}
