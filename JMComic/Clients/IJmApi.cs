using Refit;
using zms9110750.JMComic.Model;

namespace zms9110750.JMComic.Clients;

/// <summary>
/// JMComic API接口（使用Refit）
/// </summary>
/// <remarks>
/// 注意：网页端API返回HTML，不是JSON。
/// 这些接口主要用于获取HTML页面，然后解析。
/// </remarks>
public interface IJmApi
{
    /// <summary>
    /// 获取本子详情页面（HTML）
    /// </summary>
    /// <param name="albumId">本子ID</param>
    /// <returns>HTML页面内容</returns>
    [Get("/album/{albumId}")]
    Task<string> GetAlbumPageAsync(string albumId);
    
    /// <summary>
    /// 获取章节详情页面（HTML）
    /// </summary>
    /// <param name="chapterId">章节ID</param>
    /// <returns>HTML页面内容</returns>
    [Get("/photo/{chapterId}")]
    Task<string> GetChapterPageAsync(string chapterId);
    
    /// <summary>
    /// 搜索页面（HTML）
    /// </summary>
    /// <param name="searchQuery">搜索关键词</param>
    /// <param name="page">页码</param>
    /// <returns>HTML页面内容</returns>
    [Get("/search/photos")]
    Task<string> SearchPageAsync(
        [AliasAs("search_query")] string searchQuery,
        [AliasAs("page")] int page = 1);
    
    /// <summary>
    /// 分类页面（HTML）
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="page">页码</param>
    /// <returns>HTML页面内容</returns>
    [Get("/albums/{category}")]
    Task<string> GetCategoryPageAsync(
        string category,
        [AliasAs("page")] int page = 1);
}