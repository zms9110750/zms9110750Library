using zms9110750.DeepSeekClient.Model.Chat.Messages;
using zms9110750.DeepSeekClient.Model.Chat.Tool;

namespace zms9110750.DeepSeekClient.Model.Chat.Request;
/// <summary>
/// API请求用的聊天请求体
/// </summary>
public interface IChatRequest : IChatRequestPartialModel, IChatRequestPartialPenalty, IChatRequestPartialMaxTokens, IChatRequestPartialStop, IChatRequestPartialStream, IChatRequestPartialTemperature
{
	/// <summary>
	/// 消息列表
	/// </summary>
	IEnumerable<IMessage> Messages { get; }

	/// <summary>
	/// 一个 object，指定模型必须输出的格式。 
	/// 设置为 { "type": "json_object" } 以启用 JSON 模式，该模式保证模型生成的消息是有效的 JSON。
	/// </summary>
	/// <remarks>注意: 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。<br/>
	/// 否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。<br/>
	/// 此外，如果 finish_reason="length"，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。</remarks>
	IResponseFormatWrap? ResponseFormat { get; }
	/// <summary>
	/// 工具集
	/// </summary> 
	IEnumerable<IToolRequest>? Tools { get; }

	/// <summary>
	/// 强制调用工具。<see cref="ChatCompletionToolChoice"/>设置强制模式。或一个<see cref="ITool"/>指定必须调用的工具。
	/// </summary> 
	JsonNode? ToolChoice { get; }

	/// <summary>
	/// 是否返回所输出 token 的对数概率。如果为 true，则在 message 的 content 中返回每个输出 token 的对数概率。
	/// </summary>
	/// <remarks>此项不允许手动修改。当<see cref="TopLogprobs"/>不为null时，此项自动设置为true。<br/>
	/// 此外，不允许和<see cref="Model"/>为R1时同时使用，若如此，此项自动设置为false。</remarks>
	bool? Logprobs { get; }

	/// <summary>
	/// 指定每个输出位置返回输出概率 top N 的 token，且返回这些 token 的对数概率。指定此参数时，logprobs 必须为 true。
	/// </summary>
	/// <remarks>介于 0 和 20 之间，默认为 0。</remarks>
	int? TopLogprobs { get; }

	/// <summary>
	/// 前缀补全开关。为true且最后一项消息是<see cref="MessageAssistant"/>时会作为补全消息
	/// </summary>
	[JsonIgnore] bool Prefix { get; }
}
/// <summary>
/// （Beta）FIM的请求体
/// </summary>
/// <remarks>
/// 不支持R1<br/>
/// "echo"不能和 "suffix" 同时使用<br/>
/// "echo"不能和"logprobs"同时使用
/// </remarks>
[Obsolete("This API is in beta testing and may change in future releases. Use with caution.")]
public interface IFIMRequest : IChatRequestPartialModel, IChatRequestPartialPenalty, IChatRequestPartialMaxTokens, IChatRequestPartialStop, IChatRequestPartialStream, IChatRequestPartialTemperature
{
	/// <summary>
	/// 模型名称。限制为deepseek-chat
	/// </summary>
	/// <remarks>FIM暂不支持R1模型，此项自动设置为deepseek-chat</remarks>
	[Obsolete("FIM currently does not support the R1 model, this option is automatically set to deepseek-chat.")]
	new string Model => "deepseek-chat";
	/// <summary>
	/// 用于生成完成内容的提示
	/// </summary>
	string Prompt { get; }
	/// <summary>
	/// 在输出中，把 prompt 的内容也输出出来
	/// </summary>
	/// <remarks>当<see cref="Suffix"/>或<see cref="Logprobs"/>存在时自动退避为null</remarks>
	bool? Echo { get; }
	/// <summary>
	/// 制定被补全内容的后缀。
	/// </summary>
	string? Suffix { get; }
	/// <summary>
	/// 制定输出中包含 logprobs 最可能输出 token 的对数概率，包含采样的 token。例如，如果 logprobs 是 20，API 将返回一个包含 20 个最可能的 token 的列表。API 将始终返回采样 token 的对数概率，因此响应中可能会有最多 logprobs+1 个元素。
	/// </summary>
	/// <remarks>介于 0 和 20 之间，默认为 0。</remarks>
	int? Logprobs { get; }
}


/// <summary>
/// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
/// </summary>
public interface IStreamOptions
{
	/// <summary>
	/// 如果设置为 true，在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。<br/>
	///此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。<br/>
	///所有其他块也将包含一个 usage 字段，但其值为 null。
	/// </summary>
	bool? IncludeUsage { get; }
}

/// <summary>
/// 一个 object，指定模型必须输出的格式。
/// </summary>
public interface IResponseFormatWrap
{
	/// <summary>
	/// 指定模型必须输出的格式。允许的值 : [text, json_object]。如果设置为json，需要有用户消息或系统消息告知格式。
	/// </summary>
	ResponseFormat FormatType { get; }
}
