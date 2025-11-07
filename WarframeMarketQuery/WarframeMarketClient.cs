using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Statistics;
using WarframeMarketQuery.Model.Users;
using zms9110750.TreeCollection.Trie;
using Version = WarframeMarketQuery.Model.Versions.Version;
namespace WarframeMarketQuery;


/// <summary>
/// Warframe市场的连接器封装
/// </summary>
/// <param name="http">网络客户端</param>
/// <param name="hybridCache">混合缓存</param>

public class WarframeMarketClient([FromKeyedServices(nameof(WarframeMarketClient))] HttpClient http, HybridCache hybridCache) : IAsyncDisposable
{
	static JsonSerializerOptions JsonOptions => Response.V2options;
	static DateTime SoftExpiration { get; } = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour / 2 * 2).ToLocalTime();
	static HybridCacheEntryOptions StatistiExpired
	{
		get
		{
			DateTime utcNow = DateTime.UtcNow;
			return new HybridCacheEntryOptions
			{
				Expiration = utcNow.Date.AddDays(2) - utcNow,
			};
		}
	}
	static HybridCacheEntryOptions DisableDistributedCache { get; } = new HybridCacheEntryOptions
	{
		Expiration = TimeSpan.FromSeconds(5),
		Flags = HybridCacheEntryFlags.DisableDistributedCache | HybridCacheEntryFlags.DisableCompression
	};
	HttpClient Http { get; } = http;
	HybridCache HybridCache { get; } = hybridCache;

	HashSet<Task> Unfinished { get; } = [];
	Task<Response<Version>> VersionTask => field ??= GetVersionAsync().AsTask();

	ValueTask<T> GetAndDisableDistributedCache<T>(string url, CancellationToken cancellation = default)
	{
		return HybridCache.GetOrCreateAsync(url
				, async c => await Http.GetFromJsonAsync<T>(url, JsonOptions, cancellationToken: c)
				, DisableDistributedCache, cancellationToken: cancellation)!;
	}

	/// <summary>
	/// 从物品索引获取物品的slug
	/// </summary> 
	public ValueTask<string> GetSlugByIndexAsync(string index, CancellationToken cancellation = default)
	{
		return HybridCache.GetOrDefaultAsync<string>("index:" + index, cancellationToken: cancellation)!;
	}
	/// <summary>
	/// 根据物品索引获取物品短信息
	/// </summary>
	/// <exception cref="ArgumentException"></exception>
	public async ValueTask<ItemShort> GetItemByIndexAsync(string index, CancellationToken cancellation = default)
	{
		var slug = await GetSlugByIndexAsync(index, cancellation);
		return await HybridCache.GetOrDefaultAsync<ItemShort>("item:" + slug, cancellationToken: cancellation) ?? throw new ArgumentException("Item not find");
	}


	/// <summary>
	/// 获取API版本
	/// </summary>
	public ValueTask<Response<Version>> GetVersionAsync(CancellationToken cancellation = default)
	{
		return GetAndDisableDistributedCache<Response<Version>>("https://api.warframe.market/v2/versions", cancellation);
	}

	/// <summary>
	/// 获取物品短信息，储存并设置索引
	/// </summary>
	public async ValueTask GetAndSetIndexByItemAsync(CancellationToken cancellation = default)
	{
		var items = await GetAndDisableDistributedCache<Response<ItemShort[]>>("https://api.warframe.market/v2/items", cancellation);
		ConcurrentBag<string> trie = [];
		await Parallel.ForEachAsync(items.Data, cancellation, async (item, can) =>
			{
				await HybridCache.SetAsync("item:" + item.Slug, item, tags: [nameof(Item).ToLower()], cancellationToken: can);
				await HybridCache.SetAsync("index:" + item.Slug, item.Slug, tags: [nameof(Item).ToLower(), "index", nameof(item.Slug).ToLower()], cancellationToken: can);
				await HybridCache.SetAsync("index:" + item.Id, item.Slug, tags: [nameof(Item).ToLower(), "index", nameof(item.Id).ToLower()], cancellationToken: can);

				trie.Add(item.Slug);
				trie.Add(item.Id);
				foreach (var i18n in item.I18n)
				{
					await HybridCache.SetAsync("index:" + i18n.Value.Name, item.Slug, tags: [nameof(Item).ToLower(), "index", nameof(item.I18n).ToLower(), i18n.Key.ToString().ToLower()], cancellationToken: can);
					trie.Add(i18n.Value.Name);
				}
			});
		await HybridCache.SetAsync(nameof(Trie).ToLower(), trie, tags: [nameof(Item).ToLower(), "trie"], cancellationToken: cancellation);
		await HybridCache.SetAsync(nameof(Version).ToLower(), items.ApiVersion, tags: [nameof(Item).ToLower(), nameof(Version).ToLower()], cancellationToken: cancellation);
	}

	/// <summary>
	/// 获取物品信息
	/// </summary>
	/// <param name="slug">物品的slug</param>
	public ValueTask<Response<Item>> GetItemAsync(string slug, CancellationToken cancellation = default)
	{
		var url = $"https://api.warframe.market/v2/item/{slug}";
		return HybridCache.GetOrCreateAsync(url, async c => await Http.GetFromJsonAsync<Response<Item>>(url, JsonOptions, c), tags: [nameof(Item).ToLower()], cancellationToken: cancellation)!;
	}

	/// <summary>
	/// 获取物品集
	/// </summary>
	/// <param name="slug">物品的slug</param> 
	public async ValueTask<Response<ItemSet>> GetItemSetAsync(string slug, CancellationToken cancellation = default)
	{
		var url = $"https://api.warframe.market/v2/item/{slug}/set";
		//url 映射到 root id 。id 映射到 set:{id}
		var index = await HybridCache.GetOrCreateAsync(slug, async can =>
		{
			var set = await Http.GetFromJsonAsync<Response<ItemSet>>(url, cancellationToken: can) ?? throw new ArgumentException("No have this Item");

			await HybridCache.SetAsync("set:" + set.Data.Id, set, null, [nameof(ItemSet).ToLower(), "set"], cancellationToken: can);
			foreach (var item in set.Data.Items)
			{
				await HybridCache.SetAsync($"https://api.warframe.market/v2/item/{item.Slug}/set", "set:" + set.Data.Id, tags: [nameof(Item).ToLower(), "set", "index"], cancellationToken: can);
				await HybridCache.SetAsync($"https://api.warframe.market/v2/item/{item.Slug}", new Response<Item>(set.ApiVersion, item, null), tags: [nameof(Item).ToLower()], cancellationToken: can);
			}
			return "set:" + set.Data.Id;
		}, null, [nameof(ItemSet).ToLower(), "set", "index"], cancellation);

		return (await HybridCache.GetOrDefaultAsync<Response<ItemSet>>(index, null, cancellation))!;
	}


	/// <summary>
	/// 获取最近的订单
	/// </summary> 
	public ValueTask<Response<Order[]>> GetOrdersRecentAsync(CancellationToken cancellation = default)
	{
		return GetAndDisableDistributedCache<Response<Order[]>>("https://api.warframe.market/v2/orders/recent", cancellation);
	}

	/// <summary>
	/// 获取指定物品可见订单
	/// </summary>
	/// <param name="slug">物品的slug</param> 
	public ValueTask<Response<Order[]>> GetOrdersItemAsync(string slug, CancellationToken cancellation = default)
	{
		return GetAndDisableDistributedCache<Response<Order[]>>($"https://api.warframe.market/v2/orders/item/{slug}", cancellation);
	}

	/// <summary>
	/// 获取指定物品的在线用户中买单卖单各前5个
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public ValueTask<Response<OrderTop>> GetOrdersItemTopAsync(string slug, CancellationToken cancellation = default)
	{
		return GetAndDisableDistributedCache<Response<OrderTop>>($"https://api.warframe.market/v2/orders/item/{slug}/top", cancellation);
	}
	/// <summary>
	/// 获取指定物品的在线用户中买单卖单各前5个，并附带查询参数
	/// </summary>
	/// <param name="slug">物品的slug</param>
	/// <param name="query">查询参数</param>
	/// <param name="cancellation"></param>
	/// <returns></returns>
	public async ValueTask<Response<OrderTop>> GetOrdersItemTopAsync(string slug, OrderTopQueryParameter query, CancellationToken cancellation = default)
	{
		using var node = JsonSerializer.SerializeToDocument(query, JsonOptions);
		using var encodedContent = new FormUrlEncodedContent(node.RootElement.EnumerateObject().Select(s => new KeyValuePair<string, string>(s.Name, s.Value.ToString())));
		string queryString = await encodedContent.ReadAsStringAsync(cancellation);
		return await GetAndDisableDistributedCache<Response<OrderTop>>($"https://api.warframe.market/v2/orders/item/{slug}/top?{queryString}", cancellation);
	}

	/// <summary>
	/// 获取用户的公开订单
	/// </summary>
	/// <param name="slug">用户的wm用户名</param> 
	public ValueTask<Response<Order[]>> GetOrdersFromUserAsync(string slug, CancellationToken cancellation = default)
	{
		return GetAndDisableDistributedCache<Response<Order[]>>($"https://api.warframe.market/v2/orders/user/{slug}", cancellation);
	}

	/// <summary>
	/// 获取用户信息
	/// </summary>
	/// <param name="slug">用户的wm用户名</param>
	public ValueTask<Response<User>> GetUserAsync(string slug, CancellationToken cancellation = default)
	{
		return GetAndDisableDistributedCache<Response<User>>($"https://api.warframe.market/v2/user/{slug}", cancellation);
	}

	/// <summary>
	/// 获取物品订单统计数据
	/// </summary>
	/// <param name="slug">物品的slug</param> 
	public async ValueTask<Response<Statistic>> GetStatisticAsync(string slug, CancellationToken cancellation = default)
	{
		var url = $"https://api.warframe.market/v1/items/{slug}/statistics";
		var statistic = await GetStatisticAsync(); 

		if (statistic.Time < SoftExpiration)
		{
			await HybridCache.RemoveAsync(url, cancellation);
			Unfinished.Add(GetStatisticAsync().AsTask());
		}
		return statistic;


		ValueTask<Response<Statistic>> GetStatisticAsync()
		{
			return HybridCache.GetOrCreateAsync(url, async can =>
				{ 
					var version = await VersionTask;
					var statistic = await Http.GetFromJsonAsync<Statistic>(url, Response.V1options, can);
					return new Response<Statistic>(version.ApiVersion, statistic!, null);
				}, StatistiExpired, [nameof(Statistic).ToLower()], cancellation);
		}
	}

	/// <summary>
	/// 获取物品订单统计数据
	/// </summary>
	/// <param name="index">物品的索引</param> 
	public async ValueTask<Response<Statistic>> GetStatisticByIndexAsync(string index, CancellationToken cancellation = default)
	{
		var sulg = await GetSlugByIndexAsync(index, cancellation);
		return await GetStatisticAsync(sulg, cancellation);
	}

	public async ValueTask DisposeAsync()
	{
		await Task.WhenAll(Unfinished);
	}
}