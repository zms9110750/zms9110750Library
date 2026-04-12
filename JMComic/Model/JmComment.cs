

namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示本子的评论
/// </summary>
/// <param name="Id">评论的唯一标识符</param>
/// <param name="AlbumId">评论所属的本子ID</param>
/// <param name="UserId">评论用户的ID</param>
/// <param name="Username">评论用户的用户名</param>
/// <param name="AvatarUrl">评论用户的头像URL</param>
/// <param name="Content">评论内容</param>
/// <param name="CreateTime">评论创建时间，格式为时间戳或ISO 8601</param>
/// <param name="LikeCount">评论的点赞数</param>
/// <param name="ReplyTo">回复的目标评论ID，如果是回复评论则为非null</param>
/// <param name="Replies">该评论的回复列表</param>
/// <remarks>
/// 评论是用户对本子的评价和讨论。
/// 评论支持回复功能，形成评论树结构。
/// 评论需要用户登录后才能发布和查看。
/// </remarks>
public record JmComment(
    string Id,
    string AlbumId,
    string UserId,
    string Username,
    string? AvatarUrl,
    string Content,
    string CreateTime,
    int LikeCount,
    string? ReplyTo,
    JmComment[] Replies
);