using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Response;

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
	[JsonStringEnumMemberName("content_filter")] ContentFilter,
	/// <summary>
	/// 由于后端推理资源受限，请求被打断。
	/// </summary>
	[JsonStringEnumMemberName("insufficient_system_resource")] InsufficientSystemResource,
	/// <summary>
	/// 调用工具
	/// </summary>
	[JsonStringEnumMemberName("tool_calls")] ToolCalls,
	/// <summary>
	/// 因我方主动中断网络连接而结束。
	/// </summary>
	ConnectionAborted
} 