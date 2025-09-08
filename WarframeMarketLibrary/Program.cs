// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using WarframeMarketLibrary;
using WarframeMarketLibrary.Help;
using WarframeMarketLibrary.Model.Item;
Console.OutputEncoding = Encoding.Unicode;
Console.InputEncoding = Encoding.Unicode;
ServiceCollection? services = new ServiceCollection();
services.AddWarframeMarketClient();
services.AddSingleton<Preset>();
ServiceProvider? provider = services.BuildServiceProvider();
var preset = provider.GetRequiredService<Preset>();
string input;
do
{
	switch (args)
	{
		case { Length: > 0 } when args.All(s => int.TryParse(s, out _)):
			Console.WriteLine($"赋能包参数:{string.Join(",", args)}");
			var set = args.Select(int.Parse).ToHashSet();
			await preset.ArcanePrice(set);
			break;
		case ["/?"] or ["/help"]:
			Console.WriteLine("""
以下命令只能单独使用：
空格分开的纯数字：一组小小黑分解后的赋能包价值。数字是每天分解数量，按照市场流通量限制低需求赋能的参考价
-c：退出
/?：帮助

以下命令可以混合使用：
-t开头的词语，查询翻译。没有联网延迟。例如：-t成长
-u开头加wm上的用户名，查询用户公开订单。例如-ulonnstyle
其他词语，查询对应物品的价格。例如：wisp");

单词包括wm支持的中文翻译，英文原名，wm网址上表示的物品id。例如：盲怒，"Blind Rage"，blind_rage
用空格隔开多个命令，如果单词本身有空格，用引号括起来。例如"Blind Rage"

空格，下划线，赋能的·，这三个符号是分隔符。物品名字会被分隔符截断。
例如，"Wisp Prime 一套"会被分割为Wisp，Prime，一套
使用分隔符代表无视一节，从下一节开始匹配。例如" Prime 一套"允许任意的第一节。
"""); 

			break;
		case { Length: > 0 }:
			foreach (var item in args)
			{
				if (item.StartsWith("-t"))
				{
					Console.WriteLine($"====查询翻译：{item[2..]}====");
					await preset.GetItemTranslate(item[2..]);
				}
				else if (item.StartsWith("-u"))
				{
					Console.WriteLine($"====查询用户：{item[2..]}====");
					await preset.GetUserOrders(item[2..]);
				}
				else
				{
					Console.WriteLine($"====查询价格：{item}====");
					await preset.GetItemPrice(item);
				}
				Console.WriteLine();
			}
			break;
		default:
			Console.WriteLine("参数没有被识别");
			break;
	}
	Console.WriteLine();
	Console.WriteLine("继续输入命令来重新执行。或者输入-c退出程序。输入/?或/help列出命令"); 
	input = Console.ReadLine() ?? "-c";
	Console.Clear();
	args = Preset.ParseCommandLine(input);
} while (!input.Equals("-c", StringComparison.OrdinalIgnoreCase));
