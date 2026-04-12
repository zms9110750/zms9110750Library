namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示禁漫天堂的用户信息
/// </summary>
/// <param name="Id">用户的唯一标识符</param>
/// <param name="Username">用户名</param>
/// <param name="Email">用户邮箱</param>
/// <param name="EmailVerified">邮箱是否已验证</param>
/// <param name="AvatarUrl">用户头像URL</param>
/// <param name="Gender">用户性别</param>
/// <param name="Level">用户等级</param>
/// <param name="Experience">用户经验值</param>
/// <param name="Coins">用户金币数量</param>
/// <param name="FavoriteCount">用户收藏的本子数量</param>
/// <param name="MaxFavorites">用户最大可收藏数量</param>
/// <remarks>
/// 用户信息包含用户在禁漫天堂的个人资料和统计数据。
/// 用户信息需要登录后才能获取，用于个性化功能和权限控制。
/// </remarks>
public record JmUser(
    string Id,
    string Username,
    string? Email,
    bool EmailVerified,
    string? AvatarUrl,
    string? Gender,
    int Level,
    int Experience,
    int Coins,
    int FavoriteCount,
    int MaxFavorites
);