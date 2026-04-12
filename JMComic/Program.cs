using zms9110750.JMComic;
/* 
using var client = new JmClient();
var p=client.BuildImageUrl("1424817", 1); // 示例：构建章节1424817的第一张图片URL

Console.WriteLine(p);*/

/// <summary>
/// JMComic下载器主程序（简化版）
/// </summary>
/// <remarks>
/// 直接下载图片，不需要API调用。
/// 需要手动指定章节ID和图片数量。
/// </remarks>
class Program
{
    private static readonly string DownloadPath = @"X:\共享目录\测试文件\jm1091571";
    
    // 需要手动配置的信息
    private static readonly Dictionary<string, int> Chapters = new()
    {
        // 格式：章节ID = 图片数量
        // 示例：{"1424817", 20} 表示章节1424817有20张图片
        // 需要您手动查找这些信息
        ["1424817"]=79
    };
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("    JMComic 图片下载器（简化版）");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        if (Chapters.Count == 0)
        {
            Console.WriteLine("❌ 错误：请先在Program.cs中配置章节信息");
            Console.WriteLine("格式：章节ID = 图片数量");
            Console.WriteLine("示例：{\"1424817\", 20} 表示章节1424817有20张图片");
            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return;
        }
        
        try
        {
            // 创建下载目录
            Directory.CreateDirectory(DownloadPath);
            Console.WriteLine($"下载目录: {DownloadPath}");
            Console.WriteLine($"章节数: {Chapters.Count}");
            Console.WriteLine();
            
            // 创建客户端
            using var client = new JmClient();
            Console.WriteLine("✓ 客户端初始化成功");
            Console.WriteLine($"API域名: {string.Join(", ", client.ApiDomains)}");
            Console.WriteLine($"图片域名: {string.Join(", ", client.ImageDomains)}");
            Console.WriteLine();
            
            // 测试域名连通性
            Console.WriteLine("测试域名连通性...");
            var workingDomains = await client.TestDomainsAsync();
            if (workingDomains.Length == 0)
            {
                Console.WriteLine("❌ 所有域名都不可用，请检查网络连接");
                return;
            }
            Console.WriteLine($"✓ 可用域名: {string.Join(", ", workingDomains)}");
            Console.WriteLine();
            
            // 下载所有章节
            await DownloadAllChaptersAsync(client);
            
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("✅ 下载完成！");
            Console.WriteLine($"所有文件已保存到: {DownloadPath}");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("❌ 下载失败！");
            Console.WriteLine($"错误: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"内部错误: {ex.InnerException.Message}");
            }
            
            Console.WriteLine("========================================");
        }
        
        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
    
    /// <summary>
    /// 下载所有章节
    /// </summary>
    private static async Task DownloadAllChaptersAsync(JmClient client)
    {
        Console.WriteLine("开始下载章节...");
        Console.WriteLine();
        
        int chapterIndex = 1;
        foreach (var (chapterId, imageCount) in Chapters)
        {
            Console.WriteLine($"章节 {chapterIndex}/{Chapters.Count}: ID={chapterId}, 图片数={imageCount}");
            
            try
            {
                // 创建章节目录
                var chapterDir = Path.Combine(DownloadPath, $"第{chapterIndex:000}话_{chapterId}");
                Directory.CreateDirectory(chapterDir);
                
                // 下载所有图片
                var images = await client.DownloadChapterImagesAsync(chapterId, imageCount, cancellationToken: default);
                
                // 保存图片
                int successCount = 0;
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].Length > 0)
                    {
                        var filePath = Path.Combine(chapterDir, $"{i + 1:000}.webp"); 
                        await File.WriteAllBytesAsync(filePath, images[i]);
                        successCount++;
                    }
                }
                
                Console.WriteLine($"  ✓ 章节下载完成: {successCount}/{imageCount} 张图片");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ 章节下载失败: {ex.Message}");
            }
            
            Console.WriteLine();
            chapterIndex++;
        }
    }
}