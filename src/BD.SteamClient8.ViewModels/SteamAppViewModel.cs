namespace BD.SteamClient8.ViewModels;

/// <summary>
/// <see cref="SteamApp"/> 模型绑定类型
/// </summary>
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

    /// <summary>
    /// 编辑时用于展示游戏库网格封面的流
    /// </summary>
    public Stream? EditLibraryGridStream
    {
        get => _EditLibraryGridStream;
        set => this.RaiseAndSetIfChanged(ref _EditLibraryGridStream, value);
    }

    private Stream? _EditLibraryHeroStream;

    /// <summary>
    /// 编辑时用于展示游戏库英雄封面的流
    /// </summary>
    public Stream? EditLibraryHeroStream
    {
        get => _EditLibraryHeroStream;
        set => this.RaiseAndSetIfChanged(ref _EditLibraryHeroStream, value);
    }

    private Stream? _EditLibraryLogoStream;

    /// <summary>
    /// 编辑时用于展示游戏库 Logo 的流
    /// </summary>
    public Stream? EditLibraryLogoStream
    {
        get => _EditLibraryLogoStream;
        set => this.RaiseAndSetIfChanged(ref _EditLibraryLogoStream, value);
    }

    private Stream? _EditHeaderLogoStream;

    /// <summary>
    /// 编辑时用于展示头部 Logo 的流
    /// </summary>
    public Stream? EditHeaderLogoStream
    {
        get => _EditHeaderLogoStream;
        set => this.RaiseAndSetIfChanged(ref _EditHeaderLogoStream, value);
    }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// 获取游戏库网格封面的异步任务
    /// </summary>
    public Task<CommonImageSource?> LibraryGridStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Library_Grid);

    /// <summary>
    /// 获取游戏库英雄封面的异步任务
    /// </summary>
    public Task<CommonImageSource?> LibraryHeroStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Library_Hero);

    /// <summary>
    /// 获取游戏库英雄模糊封面的异步任务
    /// </summary>
    public Task<CommonImageSource?> LibraryHeroBlurStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Library_Hero_Blur);

    /// <summary>
    /// 获取游戏库 Logo 的异步任务
    /// </summary>
    public Task<CommonImageSource?> LibraryLogoStream => ISteamService.Instance.GetAppImageAsync(Model!, LibCacheType.Logo);

    /// <summary>
    /// 获取头部 Logo 的异步任务
    /// </summary>
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

    /// <summary>
    /// 刷新编辑时的图像流
    /// </summary>
    public async void RefreshEditImage()
    {
        EditLibraryGridStream = await GetStreamAsync(LibraryGridStream);
        EditLibraryHeroStream = await GetStreamAsync(LibraryHeroStream);
        EditLibraryLogoStream = await GetStreamAsync(LibraryLogoStream);
        EditHeaderLogoStream = await GetStreamAsync(HeaderLogoStream);
    }

#endif
}
