#pragma warning disable CS1573 // 参数在 XML 注释中没有匹配的 param 标记(但其他参数有)

namespace zms9110750.JMComic.Configuration;

/// <summary>
/// JMComic客户端配置
/// </summary>
/// <param name="ApiDomains">API域名列表</param>
/// <param name="ImageDomains">图片域名列表</param>
/// <remarks>
/// 最小配置：只需要域名列表。
/// </remarks>
public record JmOptions(
    string[] ApiDomains,
    string[] ImageDomains
)
{
    /// <summary>
    /// 默认配置（使用网页端域名）
    /// </summary>
    public static JmOptions Default { get; } = new JmOptions(
        ApiDomains: new[] 
        {
            "18comic.ink",
            "18comic.vip", 
            "jmcomic-zzz.one",
            "jmcomic-zzz.org",
            "jm18c-oec.cc",
            "jm18c-oecclu.com",
            "jm18c-oec.club",
            "jmcomic-bibo.me",
            "jmcomic-meme.cc",
            "jmcomic-meme.club"
        },
        ImageDomains: new[]
        {
            "jm18c-oec.cc",
            "jm18c-oecclu.com",
            "jm18c-oec.club",
            "jmcomic-bibo.me",
            "jmcomic-meme.cc",
            "jmcomic-meme.club"
        }
    );
    
    /// <summary>
    /// 验证配置
    /// </summary>
    public void Validate()
    {
        if (ApiDomains.Length == 0)
            throw new ArgumentException("至少需要一个API域名");
        
        if (ImageDomains.Length == 0)
            throw new ArgumentException("至少需要一个图片域名");
    }
}