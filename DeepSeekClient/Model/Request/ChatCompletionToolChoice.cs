using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Request;
/// <summary>
/// 调用工具的要求
/// </summary>
public enum ChatCompletionToolChoice
{
	/// <summary>
	/// AI自己决定是否调用
	/// </summary>
	[JsonStringEnumMemberName("auto")] Auto,
	/// <summary>
	/// 不允许AI调用
	/// </summary>
	[JsonStringEnumMemberName("none")] Nonde,
	/// <summary>
	/// 必须调用
	/// </summary>
	[JsonStringEnumMemberName("required")] Required
}
