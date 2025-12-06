using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WarframeMarketQuery;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using zms9110750.TreeCollection.Trie;

Console.OutputEncoding = Encoding.Unicode;
Console.InputEncoding = Encoding.Unicode;
ServiceCollection? services = new ServiceCollection();
services
    .AddLogging(builder =>
    {
        //builder.AddConsole( ); // 输出到控制台
    })
    .AddFusionCacheAndSqliteCache()
    .AddWarframeMarketClient()
    .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddYamlFile("赋能包配置.yaml").Build())
    .AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetSection("赋能包配置").Get<ArcanePack[]>()!)
    .AddSingleton(sp =>
    {
        Trie? trie = new Trie(['_', ' ', '·']);
        var index = sp.GetService<HybridCache>()?.GetOrDefaultAsync<string[]>(nameof(Trie).ToLower()).AsTask().Result;
        foreach (var item in index ?? [])
        {
            trie.Add(item);
        }
        return trie;
    })
    .AddSingleton<Preset>();
ServiceProvider? buidler = services.BuildServiceProvider();
await using Preset? preset = buidler.GetRequiredService<Preset>();
//await preset.CheckUpdata();

//await preset.Run(args);


var wm = buidler.GetRequiredService<WarframeMarketClient>();
var mod = await wm.GetItemByIndexAsync("同伴武器裂罅 Mod (尚未揭开)");
Console.WriteLine(string.Join(',',mod.ItemType));




