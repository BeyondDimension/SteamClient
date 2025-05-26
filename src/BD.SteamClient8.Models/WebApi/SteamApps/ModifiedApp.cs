#if !(IOS || ANDROID)
using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Models.Extensions;
using System.Buffers;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

/// <summary>
/// 移动过的游戏 App
/// </summary>
[global::MessagePack.MessagePackObject, global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class ModifiedApp : JsonModel<ModifiedApp>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifiedApp"/> class.
    /// </summary>
    [global::MessagePack.SerializationConstructor, global::MemoryPack.MemoryPackConstructor, global::System.Text.Json.Serialization.JsonConstructor]
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

        using var memoryStream = RecyclableMemoryStreamHelper.Manager.GetStream();
        using BinaryWriter binaryWriter = new(memoryStream);
        binaryWriter.Write(app.ChangesData);
        binaryWriter.Flush();

        ChangesData = memoryStream.GetReadOnlySequence().ToArray();
    }

    /// <summary>
    /// AppId
    /// </summary>
    [global::MessagePack.Key(0), global::MemoryPack.MemoryPackOrder(0)]
    public uint AppId { get; set; }

    private byte[]? originalData;

    /// <summary>
    /// 原始数据
    /// </summary>
    [global::MessagePack.Key(1), global::MemoryPack.MemoryPackOrder(1)]
    public byte[]? OriginalData { get => originalData; set => originalData = value; }

    private byte[]? changesData;

    /// <summary>
    /// 改动的数据
    /// </summary>
    [global::MessagePack.Key(2), global::MemoryPack.MemoryPackOrder(2)]
    public byte[]? ChangesData { get => changesData; set => changesData = value; }

    /// <summary>
    /// 改动的属性
    /// </summary>
    [global::MessagePack.IgnoreMemberAttribute, global::MemoryPack.MemoryPackIgnore]
    public SteamAppPropertyTable? Changes { get; set; }

    /// <summary>
    /// 读取改动信息
    /// </summary>
    /// <returns></returns>
    public SteamAppPropertyTable? ReadChanges()
    {
        if (ChangesData != null)
        {
            using var memoryStream = RecyclableMemoryStreamHelper.Manager.GetStream(ChangesData);
            using BinaryReader reader = new BinaryReader(memoryStream);
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
            using var memoryStream = RecyclableMemoryStreamHelper.Manager.GetStream(OriginalData);
            using BinaryReader reader = new BinaryReader(memoryStream);
            return reader.ReadPropertyTable();
        }
        return null;
    }
}
#endif