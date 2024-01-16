#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 获取用户国家或地区接口返回模型类
/// </summary>
public sealed class GetUserCountryOrRegionResponse
{
    /// <summary>
    /// <see cref="GetUserCountryOrRegionResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
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
public sealed class GetUserCountryOrRegionResponseResponse
{
    /// <summary>
    /// 国家或地区
    /// </summary>
    [SystemTextJsonProperty("country")]
    public string? CountryOrRegion { get; set; }

    /// <inheritdoc/>
    public override string ToString() => CountryOrRegion!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string?(GetUserCountryOrRegionResponseResponse? r)
        => r?.ToString();
}