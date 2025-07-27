using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using WarframeMarketLibrary.Model;
using WarframeMarketLibrary.Model.Item;
using WarframeMarketLibrary.Model.Orders;
using WarframeMarketLibrary.Model.Statistics;
using WarframeMarketLibrary.Model.Users;
using ZiggyCreatures.Caching.Fusion;
using Version = WarframeMarketLibrary.Model.Versions.Version;

namespace WarframeMarketLibrary.Help;



/// <summary>
/// Warframe市场的连接器封装
/// </summary>
/// <param name="http">网络客户端</param>
/// <param name="memoryCache">内存缓存</param>
/// <param name="fusionCache">Fusions缓存</param>
/// <param name="hybridCache">混合缓存</param>
/// <param name="options">Json序列化设置</param>
public class WarframeMarketClient([FromKeyedServices(nameof(WarframeMarketClient))] HttpClient http, IMemoryCache? memoryCache = null, IFusionCache? fusionCache = null, HybridCache? hybridCache = null, JsonSerializerOptions? options = null)
{

	JsonSerializerOptions JsonOptions => options ?? SourceGenerationContext.V2;

#pragma warning disable CS1573 // 参数在 XML 注释中没有匹配的 param 标记(但其他参数有)
	/// <summary>
	/// 基于指定的语言，设置默认标头。同时设置http的<see cref="HttpClient.BaseAddress"/>
	/// </summary> 
	/// <param name="language">语言类型</param>
	/// <inheritdoc cref="WarframeMarketClient(HttpClient, IMemoryCache?, IFusionCache?, HybridCache?, JsonSerializerOptions?)" path="/param"/>
	public WarframeMarketClient(HttpClient http, Language language, IMemoryCache? memoryCache = null, IFusionCache? fusionCache = null, HybridCache? hybridCache = null, JsonSerializerOptions? options = null)
		: this(http, memoryCache, fusionCache, hybridCache, options)
	{
		http.BaseAddress = new Uri("https://api.warframe.market");
		http.DefaultRequestHeaders.Remove("Language");
		http.DefaultRequestHeaders.Add("Language", JsonSerializer.Serialize(language, JsonOptions).Trim('"'));
	}
#pragma warning restore CS1573 // 参数在 XML 注释中没有匹配的 param 标记(但其他参数有)

	/// <summary>
	/// 访问WarframeMarket的API，并序列化后返回结果
	/// </summary>
	/// <typeparam name="T">序列化类型</typeparam>
	/// <param name="url">相对url</param>
	/// <param name="policy">缓存策略</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public async Task<T> Get<T>(string url, CachePolicy policy = CachePolicy.Moment, JsonSerializerOptions? serializerOptions = null, CancellationToken cancellation = default)
	{
		serializerOptions ??= JsonOptions;
		return (
			fusionCache != null
			? await fusionCache.GetOrSetAsync(url, async ct => await HttpGet(ct), FusionOptionWithPolicy(policy), token: cancellation)
			: hybridCache != null
			? await hybridCache.GetOrCreateAsync(url, async ct => await HttpGet(ct), HybridOptionWithPolicy(policy), cancellationToken: cancellation)
			: memoryCache != null
			? await memoryCache.GetOrCreateAsync(url, async _ => await HttpGet(cancellation), MemoryOptionWithPolicy(policy))
			: await HttpGet(cancellation))!;

		async Task<T> HttpGet(CancellationToken ct)
		{
			try
			{
				return (await http.GetFromJsonAsync<T>(url, serializerOptions, ct))!;
			}
			catch (JsonException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(await http.GetFromJsonAsync<JsonObject>(url, serializerOptions, ct));
				Console.ResetColor();
				throw;
			}
		}
		static FusionCacheEntryOptions FusionOptionWithPolicy(CachePolicy policy)
		{
			var nowOfDay = DateTime.UtcNow.TimeOfDay;
			var toNextEvenHour = TimeSpan.FromHours((nowOfDay.Hours & ~1) + 2) - nowOfDay;
			var toNextUtcZero = TimeSpan.FromDays(1) - nowOfDay;
			TimeSpan moment = TimeSpan.FromSeconds(5);
			TimeSpan minute = TimeSpan.FromMinutes(1);
			TimeSpan day = TimeSpan.FromDays(1);


			var options = new FusionCacheEntryOptions();
			return policy switch
			{
				CachePolicy.Moment => options
											.SetDuration(moment)
											.SetSkipDistributedCache(true, true),
				CachePolicy.Permanent => options
											.SetDurationInfinite(),
				CachePolicy.Statistic => options
											.SetDuration(toNextEvenHour)
											.SetFailSafe(true, toNextUtcZero, moment),
				CachePolicy.Minute => options
											.SetDuration(minute)
											.SetFailSafe(true, minute * 10, minute)
											.SetSkipDistributedCache(true, true),
				CachePolicy.Day => options
											.SetDuration(day)
											.SetFailSafe(true, day * 7, day),
				_ => throw new ArgumentException("Invalid cache policy", nameof(policy)),
			};
		}
		static HybridCacheEntryOptions HybridOptionWithPolicy(CachePolicy policy)
		{

			return new HybridCacheEntryOptions()
			{
				Expiration = GetExpiration(policy)
			};
		}
		static MemoryCacheEntryOptions MemoryOptionWithPolicy(CachePolicy policy)
		{
			return new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = GetExpiration(policy)
			};
		}
		static TimeSpan GetExpiration(CachePolicy policy)
		{
			TimeSpan moment = TimeSpan.FromSeconds(5);
			TimeSpan minute = TimeSpan.FromMinutes(1);
			TimeSpan day = TimeSpan.FromDays(1);
			TimeSpan nowOfDay = DateTime.UtcNow.TimeOfDay;
			TimeSpan toNextEvenHour = TimeSpan.FromHours((nowOfDay.Hours & ~1) + 2) - nowOfDay;
			return policy switch
			{
				CachePolicy.Moment => moment,
				CachePolicy.Permanent => TimeSpan.MaxValue,
				CachePolicy.Statistic => toNextEvenHour,
				CachePolicy.Minute => minute,
				CachePolicy.Day => day,
				_ => throw new ArgumentException("Invalid cache policy", nameof(policy))
			};
		}
	}
	/// <summary>
	/// 移除缓存
	/// </summary>
	/// <param name="key">缓存键</param>
	/// <returns></returns>
	public async Task RemoveCache(string key)
	{
		memoryCache?.Remove(key);
		fusionCache?.Remove(key);
		await (hybridCache?.RemoveAsync(key) ?? ValueTask.CompletedTask);
	}

	/// <summary>
	/// 获取API版本
	/// </summary>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Version>> GetVersion(CancellationToken cancellation = default)
	{
		return Get<Response<Version>>("v2/versions", CachePolicy.Minute, cancellation: cancellation);
	}

	/// <summary>
	/// 获取物品缓存
	/// </summary>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public async Task<ItemCache> GetItemCache(CancellationToken cancellation = default)
	{
		var version = GetVersion(cancellation);
		var itemList = await Get<ItemList>("v2/items", CachePolicy.Day, cancellation: cancellation);
		if (itemList.ApiVersion != (await version).ApiVersion)
		{
			await RemoveCache("v2/items");
			itemList = await Get<ItemList>("v2/items", CachePolicy.Day, cancellation: cancellation);
		}
		return new ItemCache(itemList);
	}

	/// <summary>
	/// 获取物品信息
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Item>> GetItem(string slug, CancellationToken cancellation = default)
	{
		return Get<Response<Item>>($"v2/item/{slug}", CachePolicy.Day, cancellation: cancellation);
	}

	/// <summary>
	/// 获取物品信息
	/// </summary>
	/// <param name="itemShort">物品实体</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Item>> GetItem(ItemShort itemShort, CancellationToken cancellation = default)
	{
		return GetItem(itemShort.Slug, cancellation);
	}

	/// <summary>
	/// 获取物品集
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<ItemSet>> GetItemSet(string slug, CancellationToken cancellation = default)
	{
		return Get<Response<ItemSet>>($"v2/item/{slug}/set", CachePolicy.Moment, cancellation: cancellation);
	}

	/// <summary>
	/// 获取物品集
	/// </summary>
	/// <param name="itemShort">物品实体</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<ItemSet>> GetItemSet(ItemShort itemShort, CancellationToken cancellation = default)
	{
		return GetItemSet(itemShort.Slug, cancellation);
	}

	/// <summary>
	/// 获取最近的订单
	/// </summary>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Order[]>> GetOrdersRecent(CancellationToken cancellation = default)
	{
		return Get<Response<Order[]>>($"v2/orders/recent", CachePolicy.Moment, cancellation: cancellation);
	}

	/// <summary>
	/// 获取指定物品可见订单
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Order[]>> GetOrdersItem(string slug, CancellationToken cancellation = default)
	{
		return Get<Response<Order[]>>($"v2/orders/item/{slug}", CachePolicy.Moment, cancellation: cancellation);
	}

	/// <summary>
	/// 获取指定物品可见订单
	/// </summary>
	/// <param name="itemShort">物品实体</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Order[]>> GetOrdersItem(ItemShort itemShort, CancellationToken cancellation = default)
	{
		return GetOrdersItem(itemShort.Slug, cancellation);
	}

	/// <summary>
	/// 获取指定物品的在线用户中买单卖单各前5个
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public async Task<Response<OrderTop>> GetOrdersItemTop(string slug, CancellationToken cancellation = default)
	{
		return await Get<Response<OrderTop>>($"v2/orders/item/{slug}/top", CachePolicy.Minute, cancellation: cancellation);
	}
	/// <summary>
	/// 获取指定物品的在线用户中买单卖单各前5个，并附带查询参数
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="query">查询参数</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public async Task<Response<OrderTop>> GetOrdersItemTop(string slug, OrderTopQueryParameter query, CancellationToken cancellation = default)
	{
		using var node = JsonSerializer.SerializeToDocument(query, options ?? SourceGenerationContext.V2);
		using var encodedContent = new FormUrlEncodedContent(node.RootElement.EnumerateObject().Select(s => new KeyValuePair<string, string>(s.Name, s.Value.ToString())));
		string queryString = await encodedContent.ReadAsStringAsync(cancellation);

		return await Get<Response<OrderTop>>($"v2/orders/item/{slug}/top?{queryString}", CachePolicy.Minute, cancellation: cancellation);
	}
	/// <summary>
	/// 获取指定物品的在线用户中买单卖单各前5个，并附带查询参数
	/// </summary>
	/// <param name="itemShort">物品实体</param>
	/// <param name="query">查询参数</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<OrderTop>> GetOrdersItemTop(ItemShort itemShort, OrderTopQueryParameter? query = null, CancellationToken cancellation = default)
	{
		return query is OrderTopQueryParameter queryPara
			? GetOrdersItemTop(itemShort.Slug, queryPara, cancellation)
			: GetOrdersItemTop(itemShort.Slug, cancellation);
	}

	/// <summary>
	/// 获取用户的公开订单
	/// </summary>
	/// <param name="slug">用户的wm用户名</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Order[]>> GetOrdersFromUser(string slug, CancellationToken cancellation = default)
	{
		return Get<Response<Order[]>>($"v2/orders/user/{slug}", CachePolicy.Minute, cancellation: cancellation);
	}
	/// <summary>
	/// 获取用户的公开订单
	/// </summary>
	/// <param name="user">用户的wm用户名</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<Order[]>> GetOrdersFromUser(User user, CancellationToken cancellation = default)
	{
		return GetOrdersFromUser(user.Slug, cancellation);
	}

	/// <summary>
	/// 获取用户信息
	/// </summary>
	/// <param name="slug">用户的wm用户名</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Response<User>> GetUser(string slug, CancellationToken cancellation = default)
	{
		return Get<Response<User>>($"v2/user/{slug}", CachePolicy.Minute, cancellation: cancellation);
	}

	/// <summary>
	/// 获取物品订单统计数据
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Statistic> GetStatistic(string slug, CancellationToken cancellation = default)
	{
		return Get<Statistic>($"v1/items/{slug}/statistics", CachePolicy.Statistic, SourceGenerationContext.V1, cancellation: cancellation);
	}

	/// <summary>
	/// 获取物品订单统计数据
	/// </summary>
	/// <param name="itemShort">物品实体</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public Task<Statistic> GetStatistic(ItemShort itemShort, CancellationToken cancellation = default)
	{
		return GetStatistic(itemShort.Slug, cancellation);
	}
}
