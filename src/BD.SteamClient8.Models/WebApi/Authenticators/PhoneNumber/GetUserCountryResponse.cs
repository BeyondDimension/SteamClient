using BD.Common8.Models.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators.PhoneNumber;

/// <summary>
/// 获取用户国家或地区接口返回模型类
/// </summary>
public sealed class GetUserCountryOrRegionResponse : JsonModel<GetUserCountryOrRegionResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="GetUserCountryOrRegionResponse"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public GetUserCountryOrRegionResponseResponse? Response { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Response?.ToString()!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string?(GetUserCountryOrRegionResponse? r)
        => r?.ToString();
}

/// <summary>
/// <see cref="GetUserCountryOrRegionResponse.Response"/> 详细信息
/// </summary>
public sealed class GetUserCountryOrRegionResponseResponse : JsonModel<GetUserCountryOrRegionResponseResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 国家或地区
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("country")]
    public string? CountryOrRegion { get; set; }

    /// <inheritdoc/>
    public override string ToString() => CountryOrRegion!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string?(GetUserCountryOrRegionResponseResponse? r)
        => r?.ToString();
}