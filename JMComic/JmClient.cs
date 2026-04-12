using System.Net.Http.Headers;
using Refit;
using zms9110750.JMComic.Clients;
using zms9110750.JMComic.Configuration;
using zms9110750.JMComic.Model;
using zms9110750.JMComic.Serialization;

namespace zms9110750.JMComic;

/// <summary>
/// JMComic客户端（简化版：直接下载图片）
/// </summary>
/// <remarks>
/// 最小实现：直接构建图片URL并下载，不需要API调用。
/// 基于已知的URL模式：https://cdn-msp.{domain}/media/photos/{photoId}/{imageName}
/// </remarks>
public class JmClient : IDisposable
{
    private readonly JmOptions _options;
    private bool _disposed;
    
    /// <summary>
    /// API域名列表
    /// </summary>
    public string[] ApiDomains { get; }
    
    /// <summary>
    /// 图片域名列表
    /// </summary>
    public string[] ImageDomains { get; }
    
    /// <summary>
    /// 初始化客户端
    /// </summary>
    /// <param name="options">配置选项</param>
    public JmClient(JmOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
        
        ApiDomains = options.ApiDomains;
        ImageDomains = options.ImageDomains;
    }
    
    /// <summary>
    /// 使用默认配置初始化客户端
    /// </summary>
    public JmClient() : this(JmOptions.Default)
    {
    }
    
    /// <summary>
    /// 构建图片URL
    /// </summary>
    /// <param name="photoId">章节ID</param>
    /// <param name="imageIndex">图片索引（从1开始）</param>
    /// <param name="fileExtension">文件扩展名，默认为".webp"</param>
    /// <returns>完整的图片URL</returns>
    public string BuildImageUrl(string photoId, int imageIndex, string fileExtension = ".webp")
    {
        // 图片域名使用简单的轮询
        var index = Environment.TickCount % ImageDomains.Length;
        var domain = ImageDomains[index];
        
        // 格式化图片文件名：00001.webp, 00002.webp, ...
        var imageName = $"{imageIndex:00000}{fileExtension}";
        
        return $"https://cdn-msp.{domain}/media/photos/{photoId}/{imageName}";
    }
    
    /// <summary>
    /// 下载图片
    /// </summary>
    /// <param name="imageUrl">图片URL</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>图片字节数组</returns>
    public async Task<byte[]> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        // 图片下载使用简单的重试逻辑
        var attempts = 0;
        const int maxAttempts = 3;
        
        while (attempts < maxAttempts)
        {
            try
            {
                using var httpClient = CreateHttpClient();
                
                var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                // 使用第一个API域名作为Referer
                request.Headers.Referrer = new Uri($"https://{ApiDomains[0]}/");
                
                var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            catch (Exception ex) when (attempts < maxAttempts - 1)
            {
                attempts++;
                Console.WriteLine($"[图片下载重试 {attempts}/{maxAttempts}] {ex.Message}");
                await Task.Delay(1000 * attempts, cancellationToken); // 指数退避
            }
        }
        
        throw new HttpRequestException($"图片下载失败，已重试{maxAttempts}次: {imageUrl}");
    }
    
    /// <summary>
    /// 下载并解码图片
    /// </summary>
    /// <param name="imageUrl">图片URL</param>
    /// <param name="scrambleId">解密密钥</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>解码后的图片字节数组</returns>
    public async Task<byte[]> DownloadAndDecodeImageAsync(
        string imageUrl, 
        string scrambleId, 
        CancellationToken cancellationToken = default)
    {
        var imageData = await DownloadImageAsync(imageUrl, cancellationToken);
        
        if (JmImageDecoder.NeedsDecoding(scrambleId))
        {
            return JmImageDecoder.Decode(imageData, scrambleId);
        }
        
        return imageData;
    }
    
    /// <summary>
    /// 下载章节的所有图片
    /// </summary>
    /// <param name="chapterId">章节ID</param>
    /// <param name="imageCount">图片数量</param>
    /// <param name="scrambleId">解密密钥（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>图片数据列表</returns>
    public async Task<byte[][]> DownloadChapterImagesAsync(
        string chapterId, 
        int imageCount,
        string? scrambleId = null,
        CancellationToken cancellationToken = default)
    {
        var results = new byte[imageCount][];
        
        for (int i = 0; i < imageCount; i++)
        {
            var imageIndex = i + 1;
            var imageUrl = BuildImageUrl(chapterId, imageIndex);
            
            try
            {
                Console.Write($"  图片 {imageIndex:000}/{imageCount:000}... ");
                
                if (!string.IsNullOrEmpty(scrambleId))
                {
                    results[i] = await DownloadAndDecodeImageAsync(imageUrl, scrambleId, cancellationToken);
                }
                else
                {
                    results[i] = await DownloadImageAsync(imageUrl, cancellationToken);
                }
                
                Console.WriteLine("✓");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ({ex.Message})");
                results[i] = Array.Empty<byte>(); // 空数组表示失败
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// 测试域名连通性
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>可用的域名列表</returns>
    public async Task<string[]> TestDomainsAsync(CancellationToken cancellationToken = default)
    {
        var workingDomains = new List<string>();
        
        Console.WriteLine("测试域名连通性...");
        
        // 测试API域名
        foreach (var domain in ApiDomains)
        {
            try
            {
                using var httpClient = CreateHttpClient();
                var url = $"https://{domain}/";
                var response = await httpClient.GetAsync(url, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    workingDomains.Add(domain);
                    Console.WriteLine($"  ✓ {domain}");
                }
                else
                {
                    Console.WriteLine($"  ✗ {domain} (HTTP {response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ {domain} ({ex.Message})");
            }
        }
        
        return workingDomains.ToArray();
    }
    
    /// <summary>
    /// 创建HttpClient
    /// </summary>
    private HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestHeaders =
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" }
            }
        };
        
        return httpClient;
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
    
    ~JmClient()
    {
        Dispose();
    }
}