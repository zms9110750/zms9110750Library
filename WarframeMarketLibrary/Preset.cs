// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using WarframeMarketLibrary.Help;
using WarframeMarketLibrary.Model;
using WarframeMarketLibrary.Model.Orders;

namespace WarframeMarketLibrary;
/// <summary>
/// 预设功能
/// </summary>
/// <param name="wm"></param>
/// <param name="pack"></param>
internal class Preset(WarframeMarketClient wm, ArcanePackage[] pack)
{
	/// <summary>
	/// 查询赋能包的参考价
	/// </summary>
	/// <param name="sets"></param>
	/// <returns></returns>
	public async Task ArcanePrice(IReadOnlySet<int> sets)
	{
		ItemCache? cache = await wm.GetItemCacheAsync();
		Dictionary<ArcanePackage, double> dic = [];
		await foreach (var task in Task.WhenEach(pack.Select(async a =>
		{
			var p = await a.GetReferencePriceAsync(cache, wm);
			lock (dic)
			{
				dic.Add(a, p);
			}
			return dic;
		})))
		{
			Console.Clear();
			Console.WriteLine(DateOnly.FromDateTime(DateTime.Now).ToString("O"));
			Console.Write("分解小小黑/(组*天)  ");
			foreach (var i in sets)
			{
				Console.Write($"|{i,-6}");
			}
			Console.WriteLine();
			foreach (var item in dic.OrderByDescending(s => s.Value))
			{
				Console.Write($"{Fill(item.Key.Name, 20)}");
				foreach (var i in sets)
				{
					Console.Write($"|{await item.Key.GetReferencePriceAsync(cache, wm, i),-6:f1}");
				}
				Console.WriteLine();
			}
		}
	}
	public async Task GetUserOrders(string user)
	{
		ItemCache? cache = await wm.GetItemCacheAsync();
		var orders = await wm.GetOrdersFromUserAsync(user);
		var list = orders.Data.Select(async s => (await s.GetReferencePriceAsync(cache, wm), s)).ToList();
		Console.WriteLine(OrderType.Sell);
		foreach (var task in list)
		{
			var (refPrice, order) = await task;
			if (order.Type != OrderType.Sell)
			{
				continue;
			}
			var itemName = cache[order.ItemId].I18n[Language.ZhHans].Name;
			Console.WriteLine($"|{refPrice,-6:f1}({(order.Platinum >= refPrice ? "+" : "-")}{Math.Abs(order.Platinum - refPrice),4:f1})\t | {itemName}");
		}
		Console.WriteLine();
		Console.WriteLine(OrderType.Buy);
		foreach (var task in list)
		{
			var (refPrice, order) = await task;
			if (order.Type != OrderType.Buy)
			{
				continue;
			}
			var itemName = cache[order.ItemId].I18n[Language.ZhHans].Name;
			Console.WriteLine($"|{refPrice,-6:f1}({(order.Platinum >= refPrice ? "+" : "-")}{Math.Abs(order.Platinum - refPrice),4:f1})\t | {itemName}");
		}
		Console.WriteLine();
		Console.WriteLine();

		var dic = orders.Data.ToDictionary(s => s.GetItemShort(cache).I18n[Language.En].Name, s => s.GetItemShort(cache).I18n[Language.ZhHans].Name);
		foreach (var item in dic.OrderBy(s => s.Key))
		{
			Console.WriteLine($"|{item.Key,-30}|{item.Value}");
		}
	}
	public async Task GetItemPrice(string itemPart)
	{
		ItemCache? cache = await wm.GetItemCacheAsync();
		await foreach (var task in Task.WhenEach(cache.SearchItems(itemPart).Select(async s => (s, (await s.GetStatisticAsync(wm)).GetReferencePrice(), (await s.GetStatisticAsync(wm)).GetMaxReferencePrice(s)))))
		{
			var (item, price0, priceMax) = await task;
			if (price0 != priceMax)
			{
				Console.WriteLine($"{Fill(item.I18n[Language.ZhHans].Name, 30)}|{price0,-6:f1}|{priceMax,-6:f1}");
			}
			else
			{
				Console.WriteLine($"{Fill(item.I18n[Language.ZhHans].Name, 30)}|{price0,-6:f1}");
			}
		}
	}


	static string Fill(string str, int length)
	{
		return str + new string(' ', length - str.Length * 2 + str.Count(char.IsAscii));
	}
	public static string[] ParseCommandLine(string input)
	{
		var matches = Regex.Matches(input, @"(?<match>(?<!\\)""[^""]*""|\S+)");
		return matches
			.Cast<Match>()
			.Select(m =>
			{
				var val = m.Groups["match"].Value;
				// 移除首尾引号（如果存在）并处理转义引号
				if (val.StartsWith("\"") && val.EndsWith("\""))
				{
					val = val.Substring(1, val.Length - 2)
							 .Replace("\"\"", "\""); // 处理双引号转义
				}
				return val;
			})
			.ToArray();
	}
}