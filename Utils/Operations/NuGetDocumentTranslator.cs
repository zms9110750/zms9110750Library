using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NeoSmart.Caching.Sqlite;
using NuGet.Configuration;
using OpenAI;
using OpenAI.Chat;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using System.ClientModel;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using System.Xml;
using System.Xml.Linq;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace zms9110750.Utils.Operations;

/// <summary>
/// NuGet 文档翻译器
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1873:避免进行可能成本高昂的日志记录", Justification = "<挂起>")]
public class NuGetDocumentTranslator
{
    private readonly ChatClient _chatClient;
    private readonly IFusionCache _fusionCache;
    private readonly ResiliencePipeline<string> _pipeline;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化翻译器
    /// </summary>
    /// <param name="apiKey">DeepSeek API Key</param>
    /// <param name="endpoint">API 端点</param>
    /// <param name="model">模型名称</param>
    /// <param name="cachePath">缓存文件路径（为 null 时使用内存缓存）</param>
    /// <param name="logger">日志记录器（可选，不传则使用 NullLogger）</param>
    public NuGetDocumentTranslator(string apiKey, string endpoint, string model, string? cachePath = "cache.sqlite.db", ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;

        // 1. 创建 ChatClient
        _chatClient = new ChatClient(model, new ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        });

        // 2. 创建 FusionCache
        var cache = new FusionCache(new FusionCacheOptions
        {
            DefaultEntryOptions = new FusionCacheEntryOptions
            {
                DistributedCacheDuration = TimeSpan.FromDays(365 * 1000)
            }
        });

        // 3. 如果有 cachePath，则添加二级缓存
        if (!string.IsNullOrEmpty(cachePath))
        {
            var sqliteCache = new SqliteCache(new SqliteCacheOptions { CachePath = cachePath });
            var serializer = new FusionCacheSystemTextJsonSerializer();
            cache.SetupDistributedCache(sqliteCache, serializer);
        }

        // 4. 添加 FusionCache 日志事件（Trace 级别）
        cache.Events.FactorySuccess += (sender, args) =>
        {
            _logger.LogTrace("[FusionCache] 缓存命中成功: {Key}", args.Key);
        };
        cache.Events.FactoryError += (sender, args) =>
        {
            _logger.LogTrace("[FusionCache] 缓存工厂失败: {Key}", args.Key);
        };
        cache.Events.Set += (sender, args) =>
        {
            _logger.LogTrace("[FusionCache] 缓存设置成功: {Key}", args.Key);
        };
        cache.Events.Remove += (sender, args) =>
        {
            _logger.LogWarning("[FusionCache] 缓存移除成功: {Key}", args.Key);
        };

        _fusionCache = cache;
        _pipeline = BuildPipeline();
    }

    /// <summary>
    /// 构建 Polly 弹性管道
    /// </summary>
    private ResiliencePipeline<string> BuildPipeline()
    {
        return new ResiliencePipelineBuilder<string>()
            .AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueLimit = 3600
            }))
            .AddRetry(new RetryStrategyOptions<string>
            {
                ShouldHandle = new PredicateBuilder<string>()
                    .Handle<RateLimiterRejectedException>(),
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogDebug("[Polly] 限流重试 {Attempt}/{MaxRetry}, 等待 {Delay}ms",
                        args.AttemptNumber, 4, args.RetryDelay.TotalMilliseconds);
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions<string>
            {
                ShouldHandle = new PredicateBuilder<string>()
                    .Handle<XmlException>(),
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(1),
                OnRetry = args =>
                {
                    _logger.LogWarning("[Polly] XML解析重试 {Attempt}/{MaxRetry}, 等待 {Delay}ms",
                        args.AttemptNumber, 4, args.RetryDelay.TotalMilliseconds);
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// 翻译单个 XML 文档（直接传入 XDocument）
    /// </summary>
    /// <param name="sourceDoc">源 XML 文档</param>
    /// <returns>翻译后的 XML 文档</returns>
    public async Task<XDocument> TranslateDocumentAsync(XDocument sourceDoc)
    {
        var members = sourceDoc.Element("doc")?.Element("members")
            ?? throw new InvalidOperationException("XML 格式不正确，缺少 doc/members 节点");

        var targetMembers = new XElement("members");
        var items = members.Elements().ToList();
        var total = items.Count;
        var processed = 0;

        await Parallel.ForEachAsync(items, async (item, cancellationToken) =>
        {
            var cacheKey = item.ToString();
            var translatedXml = await _fusionCache.GetOrSetAsync(
                cacheKey,
                async cancel =>
                {
                    return await _pipeline.ExecuteAsync(async token =>
                    {
                        // 底层执行，不记录日志
                        var completion = await _chatClient.CompleteChatAsync(
                          ["""
                       以下内容是c#文档注释的xml文件。保持xml格式，翻译为中文。
                       你的输出必须是可以XElement.Parse(resert)的字符串
                       你的输出必须带```xml代码块```。
                       """,
                        item.ToString()
                          ], cancellationToken: token)
                            .WaitAsync(TimeSpan.FromSeconds(60), token);

                        var rawText = completion.Value.Content[0].Text;
                        var match = Regex.Match(rawText, @"```xml\s*(.*?)\s*```", RegexOptions.Singleline);
                        if (match.Success)
                        {
                            rawText = match.Groups[1].Value;
                        }
                        XElement? element = XElement.Parse(rawText);
                        if ((string?)element.Attribute("name") != (string?)item.Attribute("name"))
                        {
                            throw new XmlException("翻译结果的XML格式不正确，name 属性不匹配");
                        }

                        return rawText;
                    }, cancel);
                },
                token: cancellationToken);

            var parsed = XElement.Parse(translatedXml);
            if ((string?)parsed.Attribute("name") != (string?)item.Attribute("name"))
            {
                await _fusionCache.RemoveAsync(cacheKey, token: cancellationToken);
                return;
            }

            lock (targetMembers)
            {
                targetMembers.Add(parsed);
                var current = Interlocked.Increment(ref processed);
                // 进度用 Information 级别
                _logger.LogInformation("翻译进度: {Current}/{Total}", current, total);
                Console.WriteLine($"{current,-4}/{total}");
            }
        });

        return new XDocument(new XElement("doc", targetMembers));
    }

    /// <summary>
    /// 翻译单个 XML 文档文件
    /// </summary>
    /// <param name="sourceXmlPath">源 XML 文件路径</param>
    public async Task TranslateDocumentAsync(string sourceXmlPath)
    {
        if (!File.Exists(sourceXmlPath))
        {
            throw new FileNotFoundException("原始XML文件不存在", sourceXmlPath);
        }

        var dir = Path.GetDirectoryName(sourceXmlPath)!;
        var cultureDir = Path.Combine(dir, CultureInfo.CurrentCulture.Parent.Name);
        Directory.CreateDirectory(cultureDir);
        var savePath = Path.Combine(cultureDir, Path.GetFileName(sourceXmlPath));

        var sourceDoc = XDocument.Load(sourceXmlPath);
        var translatedDoc = await TranslateDocumentAsync(sourceDoc);

        translatedDoc.Save(savePath);
        _logger.LogInformation("翻译完成，保存至: {Path}", savePath);
    }

    /// <summary>
    /// 翻译指定 NuGet 包的所有版本
    /// </summary>
    /// <param name="packageName">包名（如 Newtonsoft.Json）</param>
    public async Task TranslateAllVersionsAsync(string packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
            throw new ArgumentException("包名不能为空", nameof(packageName));

        var settings = Settings.LoadDefaultSettings(null);
        var globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);

        if (string.IsNullOrEmpty(globalPackagesFolder))
        {
            _logger.LogError("无法获取 NuGet 全局包文件夹");
            return;
        }

        var packageDir = Path.Combine(globalPackagesFolder, packageName.ToLowerInvariant());

        if (!Directory.Exists(packageDir))
        {
            _logger.LogWarning("未找到包 {PackageName} 的缓存目录: {Path}", packageName, packageDir);
            return;
        }

        var versionDirs = Directory.GetDirectories(packageDir);
        _logger.LogInformation("找到 {Count} 个版本: {Versions}", versionDirs.Length,
            string.Join(", ", versionDirs.Select(Path.GetFileName)));

        var cultureDirName = CultureInfo.CurrentCulture.Parent.Name;

        foreach (var versionDir in versionDirs)
        {
            var version = Path.GetFileName(versionDir);
            var nupkgFile = Directory.EnumerateFiles(versionDir, "*.nupkg").FirstOrDefault();
            if (nupkgFile == null)
            {
                _logger.LogDebug("版本 {Version} 没有 .nupkg 文件，跳过", version);
                continue;
            }

            _logger.LogInformation("处理 [{Package}] v{Version}", packageName, version);

            using var archive = ZipFile.OpenRead(nupkgFile);

            var xmlEntries = archive.Entries
                .Where(e => e.Name.Equals($"{packageName}.xml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!xmlEntries.Any())
            {
                _logger.LogDebug("版本 {Version} 没有与包名同名的 XML 文件: {PackageName}.xml", version, packageName);
                continue;
            }

            foreach (var xmlEntry in xmlEntries)
            {
                var xmlDir = Path.GetDirectoryName(xmlEntry.FullName) ?? "";
                var outputDir = Path.Combine(versionDir, xmlDir, cultureDirName);
                Directory.CreateDirectory(outputDir);

                try
                {
                    _logger.LogInformation("翻译 [{Package}] v{Version}: {File}", packageName, version, xmlEntry.Name);

                    using var stream = xmlEntry.Open();
                    using var reader = new StreamReader(stream);
                    var xmlContent = await reader.ReadToEndAsync();

                    var sourceDoc = XDocument.Parse(xmlContent);
                    var translatedDoc = await TranslateDocumentAsync(sourceDoc);

                    var outputFile = Path.Combine(outputDir, xmlEntry.Name);
                    translatedDoc.Save(outputFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "翻译失败 [{Package}] v{Version}: {File}", packageName, version, xmlEntry.Name);
                }
            }
        }

        _logger.LogInformation("完成翻译 {PackageName}", packageName);
    }
}