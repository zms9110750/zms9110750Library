using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using Refit;
using System.Formats.Tar;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.RateLimiting;
using WarframeMarketQuery;
using WarframeMarketQuery.API;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model.Items;
using HttpClient = System.Net.Http.HttpClient;
var services = new ServiceCollection();

var http = services.AddHttpClient<WarframeMarketApi>(client =>
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
services.AddMemoryCache();

var host = services.BuildServiceProvider();
var p = host.GetService<IWarframeMarketApi>();


/*var s = await p.GetOrdersItemTopAsync("blind_rage");
Console.WriteLine(s.Content.Data.Sell[0].User.IngameName);
Console.WriteLine(s.Content.Data.Sell[0].Platinum);*/



/*await streamingDownloader.DownloadFileAsync("https://gitee.com/zms9110750/Warframe.Market/releases/download/v0.1.0/WM%E8%AE%A2%E5%8D%95%E4%B8%AD%E8%8B%B1%E7%BF%BB%E8%AF%91%E5%99%A8(windos-64%E4%BD%8D%EF%BC%89.zip"
    , "X:\\测试.zip"
     );
*/
//await TarGzProgressProcessor.CompressDirectoryWithProgressAsync("B:\\Bandizip", "B:\\Bandizip3.tgz", progress);
//await TarGzProgressProcessor.ExtractToDirectoryWithProgressAsync("B:\\Bandizip3.tgz", "B:\\Bandizip3", progress);


//var hddt=host.GetService<IHttpClientFactory>().CreateClient("jpjp");

var subject = new Subject<long>();

// 作为被观察者，可以被订阅
var subscription = subject.Subscribe(
    value => Console.WriteLine($"下载进度: {value} bytes")
);

/*
TarGzDownloader Streaming = new TarGzDownloader(progress: subject);
await Streaming.DownloadAndExtractTarGzAsync("https://gitee.com/zms9110750/Warframe.Market/releases/download/v0.1.0/win-x64.tgz", "X:\\win-x64");
*/

// 字节转换为人类可读的文件大小
long bytes = 1024;
Console.WriteLine(bytes.Bytes());        // 输出: 1 KB
Console.WriteLine(bytes.Bytes().Humanize()); // 输出: 1 KB

long bytes2 = 1500;
Console.WriteLine(bytes2.Bytes());       // 输出: 1.46 KB

long bytes3 = 1024 * 1024+95858;
Console.WriteLine(bytes3.Megabytes());       // 输出: 1 MB

// 使用更详细的控制
long fileSize = 1234567890L;
Console.WriteLine(fileSize.Bytes().ToString("#.##"));  // 输出: 1.15 GB
Console.WriteLine(fileSize.Bytes().Humanize());        // 输出: 1.15 GB
Console.WriteLine(fileSize.Bytes().Humanize("#.## mb"));    // 输出: 1,177.38 MB
string currentDirectory = Environment.CurrentDirectory;
Console.WriteLine($"当前工作目录: {currentDirectory}");

Console.WriteLine(File.Exists("C:\\Users\\16229\\source\\repos\\zms9110750Library\\WarframeMarketQueryWPF\\bin\\Debug\\net10.0-windows10.0.17763.0\\v0.1.0/WM订单中英翻译器(windos-64位）.zip"));
Console.WriteLine(Directory.Exists("C:\\Users\\16229\\source\\repos\\zms9110750Library\\WarframeMarketQueryWPF\\bin\\Debug\\net10.0-windows10.0.17763.0\\v0.1.0/WM订单中英翻译器(windos-64位）.zip"));
/// <summary>
/// 支持进度报告的 TarGz 压缩/解压处理器
/// </summary>
/// <summary>
/// 支持进度报告的 TarGz 解压器
/// </summary>
public class TarGzDownloader(HttpClient? httpClient = null, IObserver<long>? progress = null)
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    /// <summary>
    /// 下载并解压 .tar.gz 文件
    /// </summary>
    public async Task DownloadAndExtractTarGzAsync(
        string url,
        string extractPath,
        CancellationToken cancellationToken = default)
    {
        // 1. 发送请求获取响应
        using var response = await _httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        // 2. 获取总大小
        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

        // 3. 创建解压目录
        Directory.CreateDirectory(extractPath);

        // 4. 获取响应流
        await using var responseStream = await response.Content.ReadAsStreamAsync();

        // 5. 使用 ProgressStream 包装
        await using var progressStream = new ProgressStream(PipeReader.Create(responseStream).AsStream(), progress);

        // 6. 创建 GZip 解压流
        await using var gzipStream = new GZipStream(progressStream, CompressionMode.Decompress);

        await TarFile.ExtractToDirectoryAsync(gzipStream, extractPath, overwriteFiles: true, cancellationToken: cancellationToken);

    }
}