#if !(IOS || ANDROID)
namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// 移动过的游戏 App
/// </summary>
[MPObj, MP2Obj(MP2SerializeLayout.Explicit)]
public sealed partial class ModifiedApp
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModifiedApp"/> class.
    /// </summary>
    [MPConstructor, MP2Constructor, SystemTextJsonConstructor]
    public ModifiedApp()
    {
        ReadChanges();
    }

    /// <summary>
    /// 通过 <see cref="SteamApp"/> 构造 <see cref="ModifiedApp"/> 实例
    /// </summary>
    /// <param name="app"></param>
    /// <exception cref="ArgumentException"></exception>
    public ModifiedApp(SteamApp app)
    {
        if (app.ChangesData == null)
        {
            throw new ArgumentException("New ModifiedApp Failed. SteamApp.ChangesData is null.");
        }
        AppId = app.AppId;
        if (app.OriginalData?.Clone() is byte[] originalData)
        {
            OriginalData = originalData;
        }

        using MemoryStream memoryStream = new();
        using BinaryWriter binaryWriter = new(memoryStream);
        binaryWriter.Write(app.ChangesData);

        ChangesData = binaryWriter.BaseStream.ToByteArray();
    }

    /// <summary>
    /// AppId
    /// </summary>
    [MPKey(0), MP2Key(0)]
    public uint AppId { get; set; }

    private byte[]? originalData;

    /// <summary>
    /// 原始数据
    /// </summary>
    [MPKey(1), MP2Key(1)]
    public byte[]? OriginalData { get => originalData; set => originalData = value; }

    private byte[]? changesData;

    /// <summary>
    /// 改动的数据
    /// </summary>
    [MPKey(2), MP2Key(2)]
    public byte[]? ChangesData { get => changesData; set => changesData = value; }

    /// <summary>
    /// 改动的属性
    /// </summary>
    [MPIgnore, MP2Ignore]
    public SteamAppPropertyTable? Changes { get; set; }

    /// <summary>
    /// 读取改动信息
    /// </summary>
    /// <returns></returns>
    public SteamAppPropertyTable? ReadChanges()
    {
        if (ChangesData != null)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(ChangesData));
            return Changes = reader.ReadPropertyTable();
        }
        return null;
    }

    /// <summary>
    /// 读取原始数据
    /// </summary>
    /// <returns></returns>
    public SteamAppPropertyTable? ReadOriginalData()
    {
        if (OriginalData != null)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(OriginalData));
            return reader.ReadPropertyTable();
        }
        return null;
    }
}
#endif