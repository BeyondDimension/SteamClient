#if DEBUG
namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600 // Elements should be documented

/// <summary>
/// 视图模型的示例
/// </summary>
[ViewModelWrapperGenerated(typeof(SampleModel),
        Properties = [
            nameof(SampleModel.A),
        ])]
public partial class SampleViewModel
{
    [Reactive]
    public Guid Id { get; set; }
}
#endif