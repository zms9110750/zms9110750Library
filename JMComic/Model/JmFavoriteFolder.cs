

namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示用户的收藏夹
/// </summary>
/// <param name="Id">收藏夹的唯一标识符</param>
/// <param name="Name">收藏夹的名称</param>
/// <param name="Albums">收藏夹中包含的本子列表</param>
/// <remarks>
/// 用户可以将喜欢的本子添加到收藏夹中，方便后续查找。
/// 一个用户可以有多个收藏夹，用于分类管理。
/// 收藏夹信息需要用户登录后才能访问，包含用户的个人收藏。
/// </remarks>
public record JmFavoriteFolder(
    string Id,
    string Name,
    JmAlbum[] Albums
);