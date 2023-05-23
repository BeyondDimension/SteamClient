#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.IO;
using ValveKeyValue;

namespace BD.SteamClient.Helpers;

/// <summary>
/// Valve Data File 格式助手类
/// </summary>
public static class VdfHelper
{
    const string TAG = nameof(VdfHelper);

    /// <summary>
    /// 根据路径读取 Valve Data File 内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KVObject Read(string filePath, bool isBinary = false)
    {
        var kv = KVSerializer.Create(isBinary ? KVSerializationFormat.KeyValues1Binary : KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize(IOPath.OpenRead(filePath));
        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(string filePath, KVObject content)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            kv.Serialize(stream, content);
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "Write vdf file error, filePath: {filePath}", filePath);
        }
    }
}
#endif