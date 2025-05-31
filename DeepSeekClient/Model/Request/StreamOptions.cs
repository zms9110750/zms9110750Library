using System.Diagnostics.CodeAnalysis;

namespace zms9110750.DeepSeekClient.Model.Request;
/// <summary>
/// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
/// </summary>
/// <param name="IncludeUsage">如果设置为 true，在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。<br/>
///此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。<br/>
///所有其他块也将包含一个 usage 字段，但其值为 null。</param>
public record StreamOptions(bool IncludeUsage)
{
	/// <summary>
	/// 选项为true的共享实例。
	/// </summary>
	[field: AllowNull] public static StreamOptions EnableIncludeUsage => field ??= new StreamOptions(true);

	/// <summary>
	/// 选项为false的共享实例。
	/// </summary>
	[field: AllowNull] public static StreamOptions DisableIncludeUsage => field ??= new StreamOptions(false);
}
