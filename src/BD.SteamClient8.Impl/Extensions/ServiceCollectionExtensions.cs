#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// 尝试添加 Steamworks LocalApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddSteamworksLocalApiService(this IServiceCollection services)
    {
        services.TryAddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
        return services;
    }

#endif

    /// <summary>
    /// 添加 SteamDb WebApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamDbWebApiService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.AddSingleton<ISteamDbWebApiService, SteamDbWebApiServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加 SteamGridDB WebApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamGridDBWebApiService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.AddSingleton<ISteamGridDBWebApiServiceImpl, SteamGridDBWebApiServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加 Steamworks WebApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamworksWebApiService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.AddSingleton<ISteamworksWebApiService, SteamworksWebApiServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加 Steam 账号服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="getHandler"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamAccountService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.TryAddSingleton<ISteamSessionService, SteamSessionServiceImpl>();
        services.TryAddSingleton<IRandomGetUserAgentService, ConsoleRandomGetUserAgentServiceImpl>();
        services.TryAddSingleton<ISteamAccountService, SteamAccountService>();
        return services;
    }

    /// <summary>
    /// 添加 Steam 交易报价服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamTradeService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.TryAddSingleton<ISteamSessionService, SteamSessionServiceImpl>();
        services.TryAddSingleton<ISteamTradeService, SteamTradeServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加 Steam 市场服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamMarketService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.TryAddSingleton<ISteamMarketService, SteamMarketService>();
        return services;
    }

    /// <summary>
    /// 添加 Steam 令牌服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IServiceCollection AddSteamAuthenticatorService(this IServiceCollection services, Func<CookieContainer, HttpHandlerType>? getHandler = null)
    //{
    //    if (getHandler == null)
    //        services.TryAddSingleton<ISteamAuthenticatorService, SteamAuthenticatorServiceImpl>();
    //    else
    //        services.TryAddSingleton<ISteamAuthenticatorService>(s => new SteamAuthenticatorServiceImpl(s, getHandler));
    //    return services;
    //}

    /// <summary>
    /// 添加 Steam Idle挂卡服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamIdleCardService(this IServiceCollection services)
    {
        services.TryAddHttpPlatformHelper();
        services.TryAddSingleton<ISteamSessionService, SteamSessionServiceImpl>();
        services.TryAddSingleton<ISteamIdleCardService, SteamIdleCardServiceImpl>();
        return services;
    }
}
