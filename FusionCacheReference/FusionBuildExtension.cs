using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using ZiggyCreatures.Caching.Fusion;

namespace FusionCacheReference;

public static class FusionBuildExtension
{
    /// <summary>
    /// 注册FusionCache为HybridCache并使用Sqlite为二级缓存
    /// </summary>
    /// <param name="services">服务容器</param> 
    /// <param name="cachePath">缓存路径</param>
    public static IFusionCacheBuilder AddFusionCacheAndSqliteCache(this IServiceCollection services, string cachePath = "cache.sqlite.db")
    {
        services.AddSqliteCache(cachePath);
        services.AddFusionCacheSystemTextJsonSerializer();
        return services.AddFusionCache()
            .WithDefaultEntryOptions(options =>
            {
                options.DistributedCacheDuration = TimeSpan.FromDays(365);
            })
            .TryWithAutoSetup()
            .AsHybridCache();
    }
}
