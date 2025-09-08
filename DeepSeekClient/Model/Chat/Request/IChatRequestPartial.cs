namespace zms9110750.DeepSeekClient.Model.Chat.Request;
/// <summary>
/// 聊天请求体的部分接口
/// </summary>
public interface IChatRequestPartialModel
{
	/// <summary>
	/// 聊天模型的ID
	/// </summary>
	string Model { get; }
}
/// <summary>
/// 聊天请求体的部分接口
/// </summary>
public interface IChatRequestPartialPenalty
{
	/// <summary>
	/// 如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
	/// </summary>
	/// <remarks>介于 -2.0 和 2.0 之间的数字。</remarks>
	double? FrequencyPenalty { get; }
	/// <summary>
	/// 如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
	/// </summary>
	/// <remarks>介于 -2.0 和 2.0 之间的数字。</remarks>
	double? PresencePenalty { get; }
}
/// <summary>
/// 聊天请求体的部分接口
/// </summary>
public interface IChatRequestPartialMaxTokens
{
	/// <summary>
	/// 限制一次请求中模型生成 completion 的最大 token 数。输入 token 和输出 token 的总长度受模型的上下文长度的限制。
	/// </summary>
	/// <remarks>介于 1 到 8192 间的整数，如未指定 max_tokens参数，默认使用 4096。</remarks>
	int? MaxTokens { get; }
}
/// <summary>
/// 聊天请求体的部分接口
/// </summary>
public interface IChatRequestPartialStop
{
	/// <summary>
	/// 最多包含 16 个 string 的 list，在遇到这些词时，API 将停止生成更多的 token。
	/// </summary>
	IEnumerable<string>? Stop { get; set; }
}
/// <summary>
/// 聊天请求体的部分接口
/// </summary>
public interface IChatRequestPartialStream
{
	/// <summary>
	/// 如果设置为 True，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
	/// </summary>
	bool? Stream { get; }

	/// <summary>
	/// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
	/// </summary>
	/// <remarks>该选项随<see cref="Stream"/>自动设置。不可手动设置。</remarks>
	IStreamOptions? StreamOptions { get; }
}
/// <summary>
/// 聊天请求体的部分接口
/// </summary>
public interface IChatRequestPartialTemperature
{
	/// <summary>
	/// 采样温度。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。<br/>
	/// 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
	/// </summary>
	/// <remarks>介于 0 和 2 之间</remarks>
	double? Temperature { get; }

	/// <summary>
	/// 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。。<br/>
	///所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。 。<br/>
	///我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改。
	/// </summary>
	/// <remarks>介于 0 和 1 之间</remarks>
	double? TopP { get; }
}

/// <summary>
/// 指定模型必须输出的格式。
/// </summary>
public enum ResponseFormat
{
	/// <summary>
	/// 输出为文本格式。
	/// </summary>
	Text,
	/// <summary>
	/// 输出为JSON格式。
	/// </summary>
	JsonObject
}

/// <summary>
/// 调用工具的要求
/// </summary>
public enum ChatCompletionToolChoice
{
	/// <summary>
	/// AI自己决定是否调用
	/// </summary>
	Auto,
	/// <summary>
	/// 不允许AI调用
	/// </summary>
	None,
	/// <summary>
	/// 必须调用
	/// </summary>
	Required
}
