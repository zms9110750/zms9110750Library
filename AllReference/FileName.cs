using Godot;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using Refit;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using WarframeMarketQuery;
using WarframeMarketQuery.API;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Users;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ZiggyCreatures.Caching.Fusion;
var services = new ServiceCollection();

var http = services.AddHttpClient<WarframeMarketClient>(client =>
{
    client.BaseAddress = new Uri("https://api.warframe.market");
    client.DefaultRequestHeaders.Add("Language", JsonNamingPolicy.KebabCaseLower.ConvertName(Language.ZhHans.ToString()));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("wmquery-WPF_blazor/2025-12-09 (https://github.com/zms9110750)");
});
http.AddRefitClient<IWarframeMarketApi>(sp => new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(IWarframeMarketApi.V2options),
    UrlParameterKeyFormatter = new CamelCaseUrlParameterKeyFormatter()
});
http.AddResilienceHandler("wm", builder =>
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
        QueueLimit = 6
    }));
});
services.AddRefitClient<IGitee>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(IWarframeMarketApiV1.V1options) }).ConfigureHttpClient(http => http.BaseAddress = new Uri("https://gitee.com/api/v5"));
services.AddMemoryCache();

var host = services.BuildServiceProvider();
var p = host.GetService<IWarframeMarketApi>();


/*var s = await p.GetOrdersItemTopAsync("blind_rage");
Console.WriteLine(s.Content.Data.Sell[0].User.IngameName);
Console.WriteLine(s.Content.Data.Sell[0].Platinum);*/


var git = host.GetService<IGitee>();
var rele = await git.Releases("zms9110750", "Warframe.Market");
Console.WriteLine(rele[0]);
foreach (var item in rele[0].Assets)
{
    Console.WriteLine(item);
}
public record GiteeRelease(long Id, string TagName, string Name, string Body, DateTime CreatedAt, ReleaseAsset[] Assets);

public record ReleaseAsset(string BrowserDownloadUrl, string Name);
