namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600 // Elements should be documented

[ViewModelWrapperGenerated(typeof(SteamRemoteFile))]
public partial class SteamRemoteFileViewModel
{
    public string Name { get => Model.Name; }

    public int Read(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileRead(Name, buffer);
    }

    public byte[] ReadAllBytes()
    {
        byte[] buffer = new byte[Size];
        int read = Read(buffer);
        if (read != buffer.Length)
            throw new IOException("Could not read entire file.");
        return buffer;
    }

    public bool WriteAllBytes(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileWrite(Name, buffer);
    }

    public bool Forget()
    {
        return ISteamworksLocalApiService.Instance.FileForget(Name);
    }

    public bool Delete()
    {
        return ISteamworksLocalApiService.Instance.FileDelete(Name);
    }
}
