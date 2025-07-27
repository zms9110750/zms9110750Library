// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using WarframeMarketLibrary.Help;
using WarframeMarketLibrary.Model;
using WarframeMarketLibrary.Model.Item;
using WarframeMarketLibrary.Model.Orders;


var services = new ServiceCollection();
services.AddWarframeMarketClient();


var provider = services.BuildServiceProvider();
var wm = provider.GetRequiredService<WarframeMarketClient>();
var cache = await wm.GetItemCache();


// 获取玩家订单的全部道具。和参考价值的差异
var orders = await wm.GetOrdersFromUser("lonnstyle");
foreach (var order in orders.Data.Where(s => s.Type == OrderType.Sell))
{
	var refPrice = (await order.GetItemShort(cache).GetStatistic(wm)).GetReferencePrice(e => e.ModRank == order.Rank);
	var itemName = cache[order.ItemId].I18n[Language.ZhHans].Name;
	Console.WriteLine($"|{refPrice,-6:f1}({(order.Platinum - refPrice > 0 ? "+" : "-")}{Math.Abs(order.Platinum - refPrice),4:f1})\t | {itemName}");
}


Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

//输出赋能包期望价值
var pack = ArcanePackage.Create();
int[] count = [0, 2, 6, 15, 35];


Console.WriteLine(DateOnly.FromDateTime(DateTime.Now).ToString("O"));
Console.Write("分解小小黑/(组*天)  ");
foreach (var i in count)
{
	Console.Write($"|{i,-6}");
}
Console.WriteLine();
foreach (var item in pack)
{
	var name = item.Name;
	name += new string(' ', 20 - (name.Length * (name == "Ostron" ? 1 : 2)));

	Console.Write($"{name}");
	foreach (var i in count)
	{
		Console.Write($"|{await item.GetReferencePrice(cache, wm, i),-6:f1}");
	}
	Console.WriteLine();
}


/*
|9.3   (+ 5.7)   | 最终先驱
|12.3  (+ 2.7)   | Mantis 一套
|20.0  (+10.0)   | 恶狼战锤 一套
|234.4 (+65.6)   | 膛室 Prime
|18.5  (+ 1.5)   | 龙辰 Prime 一套
|24.9  (+ 5.1)   | 创伤 Prime 一套
|35.2  (+ 4.8)   | 黑鸦 Prime 一套
|39.8  (+15.2)   | 烈焰
|18.9  (+ 1.1)   | 致命洪流
|63.8  (+ 1.2)   | 双簧管 Prime 一套
|65.5  (+ 4.5)   | 降灵追猎者 Prime 一套
|4.6   (+ 0.4)   | 阿耶檀识 Ayr 塑像
|5.3   (+ 1.7)   | 阿耶檀识 Orta 塑像
|4.1   (+ 0.9)   | 阿耶檀识 Piv 塑像
|3.9   (+ 1.1)   | 阿耶檀识 Sah 塑像
|29.9  (+ 0.1)   | 噬蛇弩 Prime 一套
|19.7  (+10.3)   | 葬铭 Prime 一套
|29.9  (+10.1)   | 手鼓 Prime 一套
|78.0  (+ 2.0)   | 提佩多 Prime 一套
|80.2  (+ 9.8)   | 搬运者 Prime 一套
|28.9  (+11.1)   | 格拉姆 Prime 一套
|48.7  (+ 1.3)   | 猎豹 Prime 一套
|49.0  (+ 1.0)   | 甲龙双拳 Prime 一套
|30.0  (+10.0)   | 碎裂者 Prime 一套
|78.9  (+ 1.1)   | 蛟龙 Prime 一套
|54.2  (+10.8)   | Gara Prime 一套
|44.6  (+ 5.4)   | Revenant Prime 一套
|22.7  (+ 2.3)   | 预言 神密
|45.3  (+ 4.7)   | 食人鱼 Prime 一套
|21.0  (+ 4.0)   | 雷霆 Prime 一套
|47.8  (+ 2.2)   | 关刀 Prime 一套
|36.3  (+ 3.7)   | 达克拉 Prime 一套
|8.1   (+ 1.9)   | 狼牙 Prime 一套
|10.0  (- 0.0)   | 主要·熟练
|10.0  (- 0.0)   | 次要·熟练
|10.0  (- 0.0)   | 次要·死首
*/