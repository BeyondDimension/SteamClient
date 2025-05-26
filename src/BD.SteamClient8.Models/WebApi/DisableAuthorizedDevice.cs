using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

[global::MessagePack.MessagePackObject, global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class DisableAuthorizedDevice : JsonRecordModel<DisableAuthorizedDevice>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// Steam ID3 格式 Id
    /// </summary>
    [global::MessagePack.Key(0), global::MemoryPack.MemoryPackOrder(0)]
    public long SteamId3_Int { get; set; }

    /// <summary>
    /// 使用时间
    /// </summary>
    [global::MessagePack.Key(1), global::MemoryPack.MemoryPackOrder(1)]
    public long Timeused { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [global::MessagePack.Key(2), global::MemoryPack.MemoryPackOrder(2)]
    public string? Description { get; set; }

    /// <summary>
    /// Token 唯一标识符
    /// </summary>
    [global::MessagePack.Key(3), global::MemoryPack.MemoryPackOrder(3)]
    public string? Tokenid { get; set; }
}
