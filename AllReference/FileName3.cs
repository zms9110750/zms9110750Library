
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using OpenAiReference;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using System.Globalization;
using System.Threading.RateLimiting;
using System.Xml;
using System.Xml.Linq;
using FusionCacheReference;

class DocumentXml
{
    ServiceProvider? build
    {
        get
        {
            if (field is null)
            {
                var build = new ServiceCollection();
                build.AddFusionCacheAndSqliteCache(); 
                field = build.AddChatClient().
                         BuildServiceProvider();

            }
            return field;
        }
    }



    public async Task TranslateDocumentXml(string sourceXmlPath)
    {
        if (!File.Exists(sourceXmlPath))
        {
            throw new FileNotFoundException("原始XML文件不存在", sourceXmlPath);
        }
        string dir = Path.GetDirectoryName(sourceXmlPath)!;
        Directory.CreateDirectory(Path.Combine(dir, CultureInfo.CurrentCulture.Parent.Name));
        string savePath = Path.Combine(dir, CultureInfo.CurrentCulture.Parent.Name, Path.GetFileName(sourceXmlPath));


        XDocument source = XDocument.Load(sourceXmlPath);
        XElement members = source.Element("doc")!.Element("members")!;
        XElement membersTranslate = File.Exists(savePath) ? XDocument.Load(savePath).Element("doc")!.Element("members")! : new XElement("members");
        ArgumentNullException.ThrowIfNull(membersTranslate);
        members.ReplaceWith(membersTranslate);

        // 3. 构建策略管道
        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                ShouldHandle = new PredicateBuilder<string>().HandleResult(x => x is null)
                .Handle<XmlException>()
            })
            .AddRateLimiterAndRetry(10, 30, TimeSpan.FromSeconds(4))
            .Build();

        ChatClient client = build.GetKeyedService<ChatClient>("DeepSeek")!;
        var hybridcache = build.GetRequiredService<HybridCache>();

        int count = 0;
        int sumCount = members.Elements().Count();
        await Parallel.ForEachAsync(members.Elements(), async (item, cance1) =>
        {
            var resert = await hybridcache.GetOrCreateAsync(item.ToString(), cance2 => pipeline.ExecuteAsync(async cance3 =>
            {
                ChatCompletion completion = await client.CompleteChatAsync("""
						以下内容是c#文档注释的xml文件。保持xml格式，翻译为中文。
						你的输出必须是可以XElement.Parse(resert)的字符串，
						不要带```xml
						""", item.ToString()).WaitAsync(TimeSpan.FromSeconds(60), cance3);

                var text = completion.Content[0].Text;
                XElement e = XElement.Parse(text);
                return (string?)e.Attribute("name") != (string?)item.Attribute("name") ? null! : text;
            }), cancellationToken: cance1);
            var e = XElement.Parse(resert);
            if ((string?)e.Attribute("name") != (string?)item.Attribute("name"))
            {
                await hybridcache.RemoveAsync(item.ToString(), cance1);
                return;
            }
            lock (membersTranslate)
            {
                membersTranslate.Add(e);
                Console.WriteLine($"{++count,-4}/{sumCount}");
            }
        });

        source.Save(savePath);
    }
}
public static class ResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// 构建多层限流策略管道
    /// </summary>
    /// <param name="builder">管道构建器</param>
    /// <param name="replenishmentRatePerSecond">每秒令牌补充量</param>
    /// <param name="maxBurst">最高突发量（令牌桶容量）</param>
    /// <param name="expectedCompletionTimeInSeconds">期望完成时间（秒）</param>
    /// <returns>配置好的弹性管道</returns>
    public static ResiliencePipelineBuilder<T> AddRateLimiterAndRetry<T>(this ResiliencePipelineBuilder<T> builder,
        int replenishmentRatePerSecond, int maxBurst = 0,
       TimeSpan expectedCompletionTimeInSeconds = default)
    {
        if (maxBurst <= 0)
        {
            maxBurst = replenishmentRatePerSecond;
        }
        if (expectedCompletionTimeInSeconds == default)
        {
            expectedCompletionTimeInSeconds = TimeSpan.FromSeconds(maxBurst / replenishmentRatePerSecond);
        }
        return builder
            // 最外层：无限重试（捕获所有限流错误）
            .AddRetry(new RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<RateLimiterRejectedException>(),
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromSeconds(maxBurst) / replenishmentRatePerSecond
            })
            // 中间层：并发限流器（带可选的排队）
            .AddRateLimiter(new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = (int)(maxBurst / expectedCompletionTimeInSeconds.TotalSeconds + replenishmentRatePerSecond) + 1,
                QueueLimit = (int)(replenishmentRatePerSecond * expectedCompletionTimeInSeconds.TotalSeconds) + 1
            }))
            .AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = maxBurst,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1) / replenishmentRatePerSecond,
                TokensPerPeriod = 1
            }));
    }

    /// <summary>
    /// 构建多层限流策略管道
    /// </summary>
    /// <param name="builder">管道构建器</param>
    /// <param name="replenishmentRatePerSecond">每秒令牌补充量</param>
    /// <param name="maxBurst">最高突发量（令牌桶容量）</param>
    /// <param name="expectedCompletionTimeInSeconds">期望完成时间（秒）</param>
    /// <returns>配置好的弹性管道</returns>
    public static ResiliencePipelineBuilder AddRateLimiterAndRetry(this ResiliencePipelineBuilder builder,
        int replenishmentRatePerSecond, int maxBurst = 0,
       TimeSpan expectedCompletionTimeInSeconds = default)
    {
        if (maxBurst <= 0)
        {
            maxBurst = replenishmentRatePerSecond;
        }
        if (expectedCompletionTimeInSeconds == default)
        {
            expectedCompletionTimeInSeconds = TimeSpan.FromSeconds(maxBurst / replenishmentRatePerSecond);
        }
        return builder
            // 最外层：无限重试（捕获所有限流错误）
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RateLimiterRejectedException>(),
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromSeconds(maxBurst) / replenishmentRatePerSecond,
            })
            // 中间层：并发限流器（带可选的排队）
            .AddRateLimiter(new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = (int)(maxBurst / expectedCompletionTimeInSeconds.TotalSeconds + replenishmentRatePerSecond) + 1,
                QueueLimit = (int)(replenishmentRatePerSecond * expectedCompletionTimeInSeconds.TotalSeconds) + 1
            }))
            .AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = maxBurst,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1) / replenishmentRatePerSecond,
                TokensPerPeriod = 1
            }));
    }
}