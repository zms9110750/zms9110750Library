using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WarframeMarketQuery.Arcane;
using ZiggyCreatures.Caching.Fusion;
using zms9110750.TreeCollection.Trie;

namespace WarframeMarketQueryWPF;

 
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 将窗口宽和高设置为屏幕宽高的一半
        Width = SystemParameters.PrimaryScreenWidth / 3 * 2;
        Height = SystemParameters.PrimaryScreenHeight / 3 * 2;


        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        serviceCollection.AddMasaBlazor();
        serviceCollection.AddMemoryCache();

#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
        serviceCollection.AddFusionCacheAndSqliteCache()
    .AddWarframeMarketClient()
    .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddYamlFile("赋能包配置.yaml").Build())
    .AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetSection("赋能包配置").Get<ArcanePack[]>()!)
    .AddSingleton(sp =>
    {
        Trie? trie = new Trie(['_', ' ', '·']);
        var index = sp.GetService<IFusionCache>()?.GetOrDefaultAsync<string[]>(nameof(Trie).ToLower()).AsTask().Result;
        foreach (var item in index ?? [])
        {
            trie.Add(item);
        }
        return trie;
    });
        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }
}
