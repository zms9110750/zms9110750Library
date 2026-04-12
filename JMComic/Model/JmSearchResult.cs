
namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示搜索结果的单个条目
/// </summary>
/// <param name="Id">本子的唯一标识符</param>
/// <param name="Title">本子的标题</param>
/// <param name="Tags">本子的标签列表</param>
/// <param name="TotalResults">搜索到的总结果数</param>
/// <remarks>
/// 搜索结果是简化的本子信息，用于列表展示。
/// 获取完整信息需要调用获取本子详情的API。
/// 搜索结果通常包含在分页响应中，用于展示搜索或分类列表。
/// </remarks>
public record JmSearchResult(
    string Id,
    string Title,
    string[] Tags,
    int TotalResults
);