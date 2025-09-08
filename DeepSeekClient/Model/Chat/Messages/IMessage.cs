using zms9110750.DeepSeekClient.Model.Chat.Tool;
using zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

namespace zms9110750.DeepSeekClient.Model.Chat.Messages;

/// <summary>
/// API请求用的聊天消息
/// </summary>
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(IMessageAssistant))]
public interface IMessage
{
	/// <summary>
	/// 该消息的发起角色
	/// </summary>
	Role Role { get; }

	/// <summary>
	/// 消息内容
	/// </summary>
	string Content { get; }

	/// <summary>
	/// 参与者的名称，为模型提供信息以区分相同角色的参与者。现在没用
	/// </summary>
	string? Name { get; }

	/// <summary>
	/// 工具调用ID。必须和<see cref="IToolCall.Id"/>对应，AI才知道这个消息是回应什么调用
	/// </summary>
	string? ToolCallId { get; }

	/// <summary>
	/// 设置 Prefix 参数。以克隆的方式创建新的实例。
	/// </summary> 
	internal IMessageAssistant WithPrefix(bool prefix = true);
}

/// <summary>
/// API请求用的聊天消息，助手消息专用接口
/// </summary>
public interface IMessageAssistant : IMessage
{

	/// <summary>
	/// (Beta) 设置此参数为 true，来强制模型在其回答中以此 assistant 消息中提供的前缀内容开始。 您必须设置 base_url = "https://api.deepseek.com/beta" 来使用此功能。
	/// </summary>
	bool? Prefix { get; }

	/// <summary>
	/// (Beta) 用于 deepseek-reasoner 模型在对话前缀续写功能下，作为最后一条 assistant 思维链内容的输入。使用此功能时，prefix 参数必须设置为 true。
	/// </summary>
	string? ReasoningContent { get; }
	/// <summary>
	/// 工具调用列表。
	/// </summary>
	IEnumerable<IToolCall>? ToolCalls { get; }

}


/// <summary>
/// 消息角色
/// </summary>
public enum Role
{
	/// <summary>
	/// 空占位符
	/// </summary>
	None,
	/// <summary>
	/// AI助手
	/// </summary>
	Assistant,
	/// <summary>
	/// 平台系统
	/// </summary>
	System,
	/// <summary>
	/// 用户
	/// </summary>
	User,
	/// <summary>
	/// 工具调用
	/// </summary>
	Tool
}