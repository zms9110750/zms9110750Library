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
	public static IServiceCollection AddFusionCacheAndSqliteCache(this IServiceCollection services, string cachePath = "cache.sqlite.db", Action<FusionCacheOptions>? options = null)
	{
		services.AddSqliteCache(cachePath);
		services.AddFusionCacheSystemTextJsonSerializer();
		services.AddFusionCache()
			.WithOptions(options ?? (options =>
			{
				options.DefaultEntryOptions = new FusionCacheEntryOptions
				{
					DistributedCacheDuration = TimeSpan.FromDays(365)
				};
			})) 
			.TryWithAutoSetup()
			.AsHybridCache();
		return services;
	}
}
