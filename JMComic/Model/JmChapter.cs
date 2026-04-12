

namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示一个完整的章节详情，包含所有图片信息
/// </summary>
/// <param name="Id">章节的唯一标识符</param>
/// <param name="Title">章节的标题</param>
/// <param name="AlbumId">所属本子的ID</param>
/// <param name="SortOrder">章节的排序序号</param>
/// <param name="ScrambleId">图片解密密钥，继承自本子或章节特有</param>
/// <param name="PageFileNames">本章节所有图片的文件名数组</param>
/// <param name="ImageDomain">图片CDN域名，用于构建图片URL</param>
/// <param name="FirstImageUrl">第一张图片的完整URL，可能包含查询参数</param>
/// <remarks>
/// 章节详情（Chapter）包含阅读所需的所有信息，特别是图片URL的构建方式。
/// 通过ImageDomain和PageFileNames可以构建所有图片的下载URL。
/// 章节详情通常通过单独的API调用获取，包含比JmEpisode更详细的信息。
/// </remarks>
public record JmChapter(
    string Id,
    string Title,
    string AlbumId,
    int SortOrder,
    string ScrambleId,
    string[] PageFileNames,
    string? ImageDomain,
    string? FirstImageUrl
);