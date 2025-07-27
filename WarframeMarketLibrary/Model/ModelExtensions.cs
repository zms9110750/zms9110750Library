
using System.Numerics;
using WarframeMarketLibrary.Help;
using WarframeMarketLibrary.Model.Item;
using WarframeMarketLibrary.Model.Orders;
using WarframeMarketLibrary.Model.Statistics;
using WarframeMarketLibrary.Model.Users;

namespace WarframeMarketLibrary.Model;
/// <summary>
/// 模型扩展方法
/// </summary>
public static class ModelExtensions
{
	///<inheritdoc cref="WarframeMarketClient.GetItem(ItemShort, CancellationToken)"/>
	public static Task<Response<Item.Item>> GetItem(this ItemShort itemShort, WarframeMarketClient client, CancellationToken cancellation = default)
	{
		return client.GetItem(itemShort, cancellation);
	}

	///<inheritdoc cref="WarframeMarketClient.GetItemSet(ItemShort, CancellationToken)"/>
	public static Task<Response<ItemSet>> GetItemSet(this ItemShort itemShort, WarframeMarketClient client, CancellationToken cancellation = default)
	{
		return client.GetItemSet(itemShort, cancellation);
	}

	///<inheritdoc cref="WarframeMarketClient.GetOrdersItem(ItemShort, CancellationToken)"/>
	public static Task<Response<Order[]>> GetOrdersItem(this ItemShort itemShort, WarframeMarketClient client, CancellationToken cancellation = default)
	{
		return client.GetOrdersItem(itemShort, cancellation);
	}

	///<inheritdoc cref="WarframeMarketClient.GetOrdersItemTop(ItemShort, OrderTopQueryParameter?, CancellationToken)"/>
	public static Task<Response<OrderTop>> GetOrdersItemTop(this ItemShort itemShort, WarframeMarketClient client, OrderTopQueryParameter? query = null, CancellationToken cancellation = default)
	{
		return client.GetOrdersItemTop(itemShort, query, cancellation);
	}

	///<inheritdoc cref="WarframeMarketClient.GetOrdersFromUser(User, CancellationToken)"/>
	public static Task<Response<Order[]>> GetOrdersFromUser(this User user, WarframeMarketClient client, CancellationToken cancellation = default)
	{
		return client.GetOrdersFromUser(user, cancellation);
	}

	///<inheritdoc cref="WarframeMarketClient.GetStatistic(ItemShort, CancellationToken)"/>
	public static Task<Statistic> GetStatistic(this ItemShort itemShort, WarframeMarketClient client)
	{
		return client.GetStatistic(itemShort);
	}
	/// <summary>
	/// 从缓存里获取关联道具
	/// </summary>
	/// <param name="order">订单</param>
	/// <param name="cache">缓存</param>
	/// <returns></returns>
	public static ItemShort GetItemShort(this Order order, ItemCache cache)
	{
		return cache[order.ItemId];
	}
	/// <summary>
	/// 赋能合成消耗
	/// </summary>
	public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];
	/// <summary>
	/// 默认权重
	/// </summary>
	public static IReadOnlyList<double> DefaultWeight { get; } = [40, 25, 15, 5, 5, 5, 5];

	/// <summary>
	/// 获取参考价格
	/// </summary>
	/// <param name="statistic">统计数据</param> 
	/// <param name="filter">过滤器</param>
	/// <param name="weight">权重</param>
	/// <returns></returns>
	public static double GetReferencePrice(this Statistic statistic, Func<Entry, bool>? filter = null, IEnumerable<double>? weight = null)
	{
		// 获取过去 90 天的统计数据，并根据 filter 过滤，按时间降序排序
		var filteredEntries = statistic.Payload.StatisticsClosed.Day90
			.Where(filter ?? (static _ => true))
			.OrderByDescending(x => x.Datetime)
			.Zip(weight ?? DefaultWeight)
			.ToArray(); // 将数据与权重配对 

		// 初始化总权重和加权总和
		var totalWeight = 0.0;
		var weightedSum = 0.0;

		// 遍历每个数据项，计算加权总和和总权重
		foreach (var (entry, weightValue) in filteredEntries)
		{
			var weightedVolume = entry.Volume * weightValue;
			totalWeight += weightedVolume;
			weightedSum += weightedVolume * entry.Median;
		}

		// 返回加权平均价
		return weightedSum / totalWeight;
	}
	/// <summary>
	/// 获取参考价格
	/// </summary>
	/// <param name="statistic">统计数据</param>
	/// <param name="itemShot">道具缓存。从缓存中获取道具的类型，并根据类型选择合成消耗和权重。</param>
	/// <param name="weight">权重</param>
	/// <returns></returns>
	public static double GetReferencePrice(this Statistic statistic, ItemShort itemShot, IEnumerable<double>? weight = null)
	{
		Func<Entry, bool>? filter = itemShot.ItemType switch
		{
			ItemType.ArcaneEnhancement => static s => s.ModRank == 0,
			ItemType.AyatanSculpture => static s => s.AmberStars != 0,
			ItemType.CraftedComponent => static s => s.Subtype == ItemSubtypes.Blueprint,
			ItemType.MOD => static s => s.ModRank == 0,
			ItemType.Relic => static s => s.Subtype == ItemSubtypes.Intact,
			_ => static s => true,
		};
		return GetReferencePrice(statistic, filter, weight);

	}
	/// <summary>
	/// 获取参考价格
	/// </summary>
	/// <param name="statistic">统计数据</param>
	/// <param name="subtype">筛选为指定的子类型</param>
	/// <param name="weight">权重</param>
	/// <returns></returns>
	public static double GetReferencePrice(this Statistic statistic, ItemSubtypes subtype, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(statistic, e => e.Subtype == subtype, weight);
	}
	/// <summary>
	/// 获取满级参考价格
	/// </summary>
	/// <param name="statistic">统计数据</param>
	/// <param name="weight">权重</param>
	/// <returns></returns>
	public static double GetMaxRankReferencePrice(this Statistic statistic, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(statistic, e => e.ModRank != 0, weight);
	}
	/// <summary>
	/// 获取赋能用满级价格推算一级的价格
	/// </summary>
	/// <param name="statistic">统计数据</param>
	/// <param name="itemShort">这个道具的信息</param> 
	/// <param name="weight">权重</param>
	/// <returns></returns>
	public static double GetMaterialBasedReferencePrice(this Statistic statistic, ItemShort itemShort, IEnumerable<double>? weight = null)
	{
		return GetMaxRankReferencePrice(statistic, weight) / SyntheticConsumption[itemShort.MaxRank ?? 0];
	}
	/// <summary>
	/// 一组小小黑可以买的赋能包能开出来的赋能数量
	/// </summary>
	public const double PackGainRate = 420.0 * 6 / 200 * 3;

	/// <summary>
	/// 获取一组小小黑赋能全部分解为荧尘，然后用全部的荧尘买这个赋能包后的，开出的赋能的期望价格
	/// </summary>
	/// <param name="package">赋能包</param>
	/// <param name="cache">缓存</param>
	/// <param name="client">wm访问器</param>
	/// <param name="purchase">每天购入的小小黑组数。如果开出的赋能比市场流通的还多，按照市场流通数量算期望</param>
	/// <param name="weight">权重</param>
	/// <returns></returns>
	public static async Task<double> GetReferencePrice(this ArcanePackage package, ItemCache cache, WarframeMarketClient client, int purchase = 0, IEnumerable<double>? weight = null)
	{
		var result = package
			.SelectMany(s => s)
			.Select(async item =>
			{
				var itemShort = cache[item];
				var statistics = await itemShort.GetStatistic(client);
				//GetProbability 获取这个道具在这个包里开出来的概率。
				//PackGainRate 一组小小黑可以买的赋能包能开出来的赋能数量
				double effectiveVolume = package.GetProbability(item) * PackGainRate;
				if (purchase != 0)
				{
					//statistics.Payload.StatisticsClosed.Day90 90天内结算的订单数据。
					//entry => entry.Volume * SyntheticConsumption[s.ModRank ?? 0]) 这个等级对应的合成消耗数量（把满级的转为一级，一级的不变）
					effectiveVolume = Math.Min(effectiveVolume, statistics.Payload.StatisticsClosed.Day90.Sum(entry => entry.Volume * SyntheticConsumption[entry.ModRank ?? 0]) / 90 / purchase);
				}
				return statistics.GetMaterialBasedReferencePrice(cache[item], weight) * effectiveVolume;
			});
		return (await Task.WhenAll(result)).Sum();
	}
}