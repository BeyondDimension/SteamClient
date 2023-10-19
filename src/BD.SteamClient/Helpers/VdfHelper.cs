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

    private static KVSerializerOptions options = new KVSerializerOptions
    {
        HasEscapeSequences = true,
    };

    /// <summary>
    /// 根据路径读取 Valve Data File 内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KVObject Read(string filePath, bool isBinary = false)
    {
        var kv = KVSerializer.Create(isBinary ? KVSerializationFormat.KeyValues1Binary : KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize(IOPath.OpenRead(filePath), options);
        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(string filePath, KVObject content)
    {
        try
        {
            //不要用 FileMode.OpenOrCreate 文件内容长度不一致会导致结尾内容错误
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            kv.Serialize(stream, content, options);
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "Write vdf file error, filePath: {filePath}", filePath);
        }
    }
}
#endif