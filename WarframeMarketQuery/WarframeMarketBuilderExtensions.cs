using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using WarframeMarketQuery;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
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
			client.DefaultRequestHeaders.Add("Language", JsonSerializer.Serialize(language, Response.V2options).Trim('"'));
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
				ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<RateLimiterRejectedException>(),
				MaxRetryAttempts = int.MaxValue,
				Delay = TimeSpan.FromSeconds(5)
			})
			.AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
			{
				TokenLimit = 3,
				ReplenishmentPeriod = TimeSpan.FromSeconds(1.0 / 4),
				TokensPerPeriod = 1,
				QueueLimit = 20
			}))  ; 
		});
		;
		services.AddSingleton<WarframeMarketClient>();
		return services;
	}
}
