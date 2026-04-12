

namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示禁漫天堂中的一个本子（专辑）
/// </summary>
/// <param name="Id">本子的唯一标识符</param>
/// <param name="Title">本子的标题</param>
/// <param name="Authors">本子的作者列表，可能包含多个作者</param>
/// <param name="Tags">本子的标签列表，用于分类和搜索</param>
/// <param name="PageCount">本子的总页数（所有章节图片的总和）</param>
/// <param name="PublishDate">本子的首次发布日期，格式为"YYYY-MM-DD"</param>
/// <param name="UpdateDate">本子的最后更新日期，格式为"YYYY-MM-DD"</param>
/// <param name="Description">本子的详细描述</param>
/// <param name="Likes">本子的点赞数，可能包含单位如"1K"</param>
/// <param name="Views">本子的观看次数，可能包含单位如"40K"</param>
/// <param name="CommentCount">本子的评论数量</param>
/// <param name="Episodes">本子包含的章节列表，按顺序排列</param>
/// <param name="ScrambleId">图片解密密钥，用于解密本子中的图片</param>
/// <param name="Works">本子所属的作品系列</param>
/// <param name="Actors">本子中登场的角色列表</param>
/// <param name="RelatedAlbums">相关推荐的本子ID列表</param>
/// <remarks>
/// 本子（Album）是禁漫天堂中的基本单位，包含一个或多个章节（Chapter）。
/// 每个本子有唯一的ID，可以通过ID获取本子的详细信息。
/// 本子信息包含阅读所需的所有元数据，如作者、标签、描述等。
/// </remarks>
public record JmAlbum(
    string Id,
    string Title,
    string[] Authors,
    string[] Tags,
    int PageCount,
    string? PublishDate,
    string? UpdateDate,
    string? Description,
    string? Likes,
    string? Views,
    int CommentCount,
    JmEpisode[] Episodes,
    string ScrambleId,
    string[] Works,
    string[] Actors,
    string[] RelatedAlbums
);