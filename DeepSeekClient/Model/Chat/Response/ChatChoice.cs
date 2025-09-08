using zms9110750.DeepSeekClient.Model.Chat.Messages;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;

namespace zms9110750.DeepSeekClient.Model.Chat.Response;

/// <summary>
/// AI回应的接口抽象
/// </summary>
public interface IChatChoice : IDelta<IChatChoice>,IIndex
{ 
	/// <summary>
	/// token概率
	/// </summary>
	LogprobsContainer? Logprobs { get; }
	/// <summary>
	/// 消息
	/// </summary>
	MessageAssistant Message { get; }
	/// <summary>
	/// 结束原因
	/// </summary>
	FinishReason? FinishReason { get; }
}


/// <summary>
/// AI聊天选择。
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Logprobs">token概率</param>
/// <param name="Message">消息</param> 
/// <param name="FinishReason">结束原因</param>
public record ChatChoice(
	 int Index,
	 LogprobsContainer? Logprobs,
	 MessageAssistant Message,
	 FinishReason? FinishReason) : IChatChoice
{
	IMerge<IChatChoice> IDelta<IChatChoice>.CreateMerge()
	{
		return new ChatDeltaMerge(this);
	}
}

/// <summary>
/// 模型停止生成 token 的原因。
/// </summary>
public enum FinishReason
{
	/// <summary>
	/// 默认占位符
	/// </summary>
	Null,
	/// <summary>
	/// 模型自然停止生成，或遇到 stop 序列中列出的字符串。
	/// </summary>
	Stop,
	/// <summary>
	/// 输出长度达到了模型上下文长度限制，或达到了 max_tokens 的限制。
	/// </summary>
	Length,
	/// <summary>
	/// 输出内容因触发过滤策略而被过滤。
	/// </summary>
	ContentFilter,
	/// <summary>
	/// 由于后端推理资源受限，请求被打断。
	/// </summary>
	InsufficientSystemResource,
	/// <summary>
	/// 调用工具
	/// </summary>
	ToolCalls,
	/// <summary>
	/// 因我方主动中断网络连接而结束。
	/// </summary>
	ConnectionAborted
}