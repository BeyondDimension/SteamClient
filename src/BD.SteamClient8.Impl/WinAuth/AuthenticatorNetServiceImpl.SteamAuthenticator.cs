using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.Converters;
using BD.SteamClient8.WinAuth.Models.Abstractions;
using System.Extensions;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Services.WinAuth;

partial class AuthenticatorNetServiceImpl
{
    /// <inheritdoc/>
    public async Task<long> GetSteamServerTimeAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using HttpRequestMessage req = new(HttpMethod.Post, SteamApiUrls.STEAM_AUTHENTICATOR_TWOFAQUERYTIME);
        using var rsp = await client.UseDefaultSendAsync(req, cancellationToken);
        var obj = await rsp.Content.ReadFromJsonAsync(SteamSyncStructLiteJSC.Default.SteamSyncStructLite, cancellationToken);
        var response = obj?.Response;
        return response.ThrowIsNull().ServerTime;
    }

    /// <inheritdoc/>
    public ISteamConvertSteamDataJsonStruct? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var obj = JsonSerializer.Deserialize(json, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);
        return obj;
    }
}

/// <summary>
/// TwoFAQueryTime 接口返回模型类，仅获取 Steam 服务器时间
/// </summary>
sealed class SteamSyncStructLite : JsonModel<SteamSyncStructLite>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => SteamSyncStructLiteJSC.Default;

    /// <summary>
    /// <see cref="SteamSyncResponseStructLite"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public SteamSyncResponseStructLite? Response { get; set; }
}

/// <summary>
/// <see cref="SteamSyncStructLite.Response"/> 详细信息
/// </summary>
sealed class SteamSyncResponseStructLite : JsonModel<SteamSyncResponseStructLite>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => SteamSyncStructLiteJSC.Default;

    /// <summary>
    /// 服务器时间 （秒）
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("server_time")]
    [global::System.Text.Json.Serialization.JsonConverter(typeof(SteamDataInt64Converter))]
    public long ServerTime { get; set; }
}

/// <summary>
/// TwoFAQueryTime 接口返回模型类，仅获取 Steam 服务器时间的 JSON 序列化上下文
/// </summary>
[JsonSerializable(typeof(SteamSyncStructLite))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SteamSyncStructLiteJSC : JsonSerializerContext
{
}