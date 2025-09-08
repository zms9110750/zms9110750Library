using System.Text;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;
using zms9110750.DeepSeekClient.Model.Chat.Tool;
namespace zms9110750.DeepSeekClient.Model.Chat.Messages;

/// <summary>
/// 消息基类
/// </summary>
/// <param name="Content">消息内容</param>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="MessageUser"/></item>
/// <item><see cref="MessageAssistant"/></item>
/// <item><see cref="MessageSystem"/></item>
/// <item><see cref="MessageTool"/></item>
/// </list>
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(MessageUser), typeDiscriminator: "user")]
[JsonDerivedType(typeof(MessageAssistant), typeDiscriminator: "assistant")]
[JsonDerivedType(typeof(MessageSystem), typeDiscriminator: "system")]
[JsonDerivedType(typeof(MessageTool), typeDiscriminator: "tool")]
public abstract record Message(string Content) : IMessage
{
	/// <summary>
	/// 消息内容
	/// </summary>
	public string Content { get; set; } = Content;
	string? IMessage.ToolCallId => (this as MessageTool)?.ToolCallId;
	string? IMessage.Name => null;
	Role IMessage.Role => this switch
	{
		MessageAssistant => Role.Assistant,
		MessageSystem => Role.System,
		MessageTool => Role.Tool,
		MessageUser => Role.User,
		_ => throw new NotImplementedException()
	};
	/// <summary>
	/// 创建普通用户消息
	/// </summary>
	/// <param name="content">消息内容</param> 
	/// <returns></returns>
	public static MessageUser NewUserMsg(string content)
	{
		return new MessageUser(content);
	}
	/// <summary>
	/// 创建一个语言模型消息
	/// </summary>
	/// <param name="content">消息内容</param> 
	/// <returns></returns>
	public static MessageAssistant NewAssistantMsg(string content)
	{
		return new MessageAssistant(content, null);
	}
	/// <summary>
	/// 创建系统/平台消息
	/// </summary>
	/// <param name="content"></param> 
	/// <returns></returns>
	public static MessageSystem NewSystemMsg(string content)
	{
		return new MessageSystem(content);
	}

	IMessageAssistant IMessage.WithPrefix(bool prefix)
	{
		return this is MessageAssistant assistant ? (assistant with { Prefix = prefix }) : throw new InvalidOperationException("Only assistant message can have prefix.");
	}
}

/// <summary>
/// 用户消息
/// </summary>
/// <param name="Content">消息内容</param>
public record MessageUser(string Content) : Message(Content);

/// <summary>
/// 工具调用消息
/// </summary>
/// <param name="Content">消息内容</param>
/// <param name="ToolCallId">工具调用ID。必须和<see cref="IToolCall.Id"/>对应，AI才知道这个消息是回应什么调用</param>
public record MessageTool(string Content, string ToolCallId) : Message(Content)
{
	/// <summary>
	/// 工具调用ID。必须和<see cref="IToolCall.Id"/>对应，AI才知道这个消息是回应什么调用
	/// </summary>
	public string ToolCallId { get; set; } = ToolCallId;
}

/// <summary>
/// 系统消息
/// </summary>
/// <param name="Content">消息内容</param> 
public record MessageSystem(string Content) : Message(Content);

/// <summary>
/// 语言模型消息
/// </summary>
/// <param name="Content">消息内容</param>
/// <param name="ReasoningContent">思维链内容</param> 
/// <param name="ToolCalls">工具调用请求。使用<see cref="ToolKit.Invoke(IToolCall)"/>生成一个工具消息</param>
public record MessageAssistant(
	string Content,
	string? ReasoningContent = null,
	List<IToolCall>? ToolCalls = null
	) : Message(Content), IDelta<MessageAssistant>, IMessageAssistant
{
	/// <summary>
	/// 思维链内容
	/// </summary>
	public string? ReasoningContent { get; set; } = ReasoningContent;
	/// <summary>
	/// 前缀内容开关
	/// </summary>
	public bool? Prefix { get; internal init; }

	/// <summary>
	/// 工具调用请求。使用<see cref="ToolKit.Invoke(IToolCall)"/>生成一个工具消息
	/// </summary>
	[JsonConverter(typeof(MessageAssistantToolCallArrayConverter))] public List<IToolCall>? ToolCalls { get; set; } = ToolCalls;

	IEnumerable<IToolCall>? IMessageAssistant.ToolCalls => ToolCalls?.Count > 0 ? ToolCalls : null;

	IMerge<MessageAssistant> IDelta<MessageAssistant>.CreateMerge()
	{
		return new MessageAssistantMerge(this);
	}

	string? IMessageAssistant.ReasoningContent => Prefix == true ? ReasoningContent : null;
}





internal class MessageAssistantMerge(MessageAssistant message) : IMerge<MessageAssistant>
{
	StringBuilder Content { get; } = new StringBuilder(message.Content);
	StringBuilder ReasoningContent { get; } = new StringBuilder(message.ReasoningContent);
	List<IToolCall> ToolCalls { get; } = new List<IToolCall>(message.ToolCalls ?? []);

	public void Merge(MessageAssistant other)
	{
		Content.Append(other.Content);
		ReasoningContent.Append(other.ReasoningContent);
		ToolCalls.AddRange(other.ToolCalls ?? []);
	}

	public MessageAssistant ToFinish()
	{
		var p = ToolCalls.GroupBy(t => t.Index)
				.Select(s =>
				{
					using var e = s.GetEnumerator();
					e.MoveNext();
					var merge = e.Current.CreateMerge();
					while (e.MoveNext())
					{
						merge.Merge(e.Current);
					}
					return merge.ToFinish();
				})
				.ToList();
		return new MessageAssistant(Content.ToString(), ReasoningContent.ToString(), p.Count > 0 ? p : null);
	}
}