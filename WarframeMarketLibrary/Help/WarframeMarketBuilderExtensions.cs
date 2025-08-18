using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using WarframeMarketLibrary.Model;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization;

namespace WarframeMarketLibrary.Help;

/// <summary>
/// 依赖注入扩展
/// </summary>
public static class WarframeMarketBuilderExtensions
{
	/// <summary>
	/// 注册WarframeMarketClient
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <param name="language"><see cref="WarframeMarketClient"/>的语言设置</param>
	/// <param name="queueSec">允许排队多久</param>
	/// <param name="permitLimit">每秒的许可数</param>
	/// <param name="cachePath">缓存路径</param>
	/// <returns></returns>
	public static IServiceCollection AddWarframeMarketClient(this IServiceCollection services, Language language = Language.ZhHans, int queueSec = 30, int permitLimit = 4, string cachePath = "cache.sqlite.db")
	{
		services.AddHttpClient<WarframeMarketClient>(client =>
		{
			client.BaseAddress = new Uri("https://api.warframe.market");
			client.DefaultRequestHeaders.Add("Language", JsonSerializer.Serialize(language, SourceGenerationContext.V2).Trim('"'));
		})
		.AddAsKeyed()
		.AddResilienceHandler("wm", builder =>
		{
			builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
			{
				ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
				   .HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests),
				UseJitter = true,
				BackoffType = DelayBackoffType.Exponential
			})
			.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
			{
				ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
					.Handle<RateLimiterRejectedException>(),
				Delay = TimeSpan.FromSeconds(queueSec),
				MaxRetryAttempts = 1
			})
			.AddRateLimiter(
			new SlidingWindowRateLimiter(
				new SlidingWindowRateLimiterOptions
				{
					PermitLimit = permitLimit,
					SegmentsPerWindow = 3,
					Window = TimeSpan.FromSeconds(1),
					QueueLimit = queueSec * permitLimit
				})
			);
		});
		services.AddSqliteCache(cachePath);
		services.AddFusionCacheSystemTextJsonSerializer(SourceGenerationContext.V2);
		services.AddSingleton(SourceGenerationContext.V2);
		services.AddFusionCache()
			.WithSerializer(sp => sp.GetRequiredService<IFusionCacheSerializer>())
			.WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>());
		services.AddSingleton<WarframeMarketClient>();
		services.AddSingleton(ArcanePackage.Create());
		return services;
	}
}
