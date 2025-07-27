namespace WarframeMarketLibrary.Help;

/// <summary>
/// 缓存策略
/// </summary>
public enum CachePolicy
{
	/// <summary>
	/// 瞬间
	/// </summary>
	Moment,
	/// <summary>
	/// 分钟
	/// </summary>
	Minute,
	/// <summary>
	/// 天
	/// </summary>
	Day,
	/// <summary>
	/// 永久
	/// </summary>
	Permanent,
	/// <summary>
	/// 统计
	/// </summary>
	Statistic
}