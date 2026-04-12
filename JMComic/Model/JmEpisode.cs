
namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示本子中的一个章节（话）
/// </summary>
/// <param name="ChapterId">章节的唯一标识符</param>
/// <param name="Index">章节在本子中的序号，从1开始</param>
/// <param name="Title">章节的标题</param>
/// <param name="PublishDate">章节的发布日期，格式为"YYYY-MM-DD"</param>
/// <remarks>
/// 章节（Episode）是本子的组成部分，一个本子可能包含多个章节。
/// 每个章节包含多张图片，用户按顺序阅读。
/// 章节信息通常出现在本子详情中，作为章节列表展示。
/// </remarks>
public record JmEpisode(
    string ChapterId,
    int Index,
    string Title,
    string? PublishDate
);