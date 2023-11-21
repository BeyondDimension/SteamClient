#if DEBUG
namespace BD.SteamClient8.Models;

#pragma warning disable SA1600 // Elements should be documented

/// <summary>
/// 模型类的示例
/// </summary>
[MP2Obj(MP2SerializeLayout.Sequential)]
public sealed partial record class SampleModel
{
    public string A { get; set; } = "";

    public bool B { get; set; }
}
#endif