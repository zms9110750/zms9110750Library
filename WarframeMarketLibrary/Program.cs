// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using WarframeMarketLibrary;
using WarframeMarketLibrary.Help;
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
		case [var s] when s.StartsWith("-u"):
			Console.WriteLine($"查询用户{s[2..]}");
			await preset.GetUserOrders(s[2..]);
			break;
		case { Length: > 0 }:
			foreach (var item in args)
			{
				Console.WriteLine($"===={item}====");
				await preset.GetItemPrice(item);
				Console.WriteLine();
			}
			break;
		default:
			Console.WriteLine("参数没有被识别");
			break;
	}
	Console.WriteLine();
	Console.WriteLine("继续输入命令来重新执行。或者输入-c退出程序。");
	input = Console.ReadLine() ?? "-c";
	Console.Clear();
	args = Preset.ParseCommandLine(input);
} while (!input.Equals("-c", StringComparison.OrdinalIgnoreCase));
