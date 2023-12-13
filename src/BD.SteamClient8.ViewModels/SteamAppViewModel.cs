namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600 // Elements should be documented

[ViewModelWrapperGenerated(typeof(SteamApp),
    Properties = [
        nameof(SteamApp.SortAs),
        nameof(SteamApp.IsEdited),
        nameof(SteamApp.SizeOnDisk),
        nameof(SteamApp.LaunchItems),
        nameof(SteamApp.SaveFiles),
        nameof(SteamApp.Process),
        nameof(SteamApp.IsWatchDownloading)
    ])]
public partial class SteamAppViewModel
{
    /// <inheritdoc cref="SteamApp.Name"/>
    public string? Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                this.RaisePropertyChanging();
                Model.Name = value;
                SortAs = value;
                this.RaisePropertyChanged();
            }
        }
    }

    private Stream? _EditLibraryGridStream;

    public Stream? EditLibraryGridStream
    {
        get => _EditLibraryGridStream;
        set => this.RaiseAndSetIfChanged(ref _EditLibraryGridStream, value);
    }

    private Stream? _EditLibraryHeroStream;

    public Stream? EditLibraryHeroStream
    {
        get => _EditLibraryHeroStream;
        set => this.RaiseAndSetIfChanged(ref _EditLibraryHeroStream, value);
    }

    private Stream? _EditLibraryLogoStream;

    public Stream? EditLibraryLogoStream
    {
        get => _EditLibraryLogoStream;
        set => this.RaiseAndSetIfChanged(ref _EditLibraryLogoStream, value);
    }

    private Stream? _EditHeaderLogoStream;

    public Stream? EditHeaderLogoStream
    {
        get => _EditHeaderLogoStream;
        set => this.RaiseAndSetIfChanged(ref _EditHeaderLogoStream, value);
    }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public Task<CommonImageSource?> LibraryGridStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Library_Grid);

    public Task<CommonImageSource?> LibraryHeroStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Library_Hero);

    public Task<CommonImageSource?> LibraryHeroBlurStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Library_Hero_Blur);

    public Task<CommonImageSource?> LibraryLogoStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Logo);

    public Task<CommonImageSource?> HeaderLogoStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Header);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async Task<Stream?> GetStreamAsync(Task<CommonImageSource?> task)
    {
        if (task != null)
        {
            return (await task)?.Stream;
        }
        return null;
    }

    public async void RefreshEditImage()
    {
        EditLibraryGridStream = await GetStreamAsync(LibraryGridStream);
        EditLibraryHeroStream = await GetStreamAsync(LibraryHeroStream);
        EditLibraryLogoStream = await GetStreamAsync(LibraryLogoStream);
        EditHeaderLogoStream = await GetStreamAsync(HeaderLogoStream);
    }

#endif
}
