using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.Model.Response.Delta;

/// <summary>
/// 消息增量
/// </summary>
/// <param name="Content">内容</param>
/// <param name="ReasoningContent">思维链</param>
/// <param name="Name">参与角色名</param>
/// <param name="ToolCalls">工具调用</param>
public record MessageDelta(string? Content, string? ReasoningContent = null, string? Name = null, JsonArray? ToolCalls = null)
{
	[field: AllowNull] StringBuilder ContentBuilder => field ??= new StringBuilder(Content);
	[field: AllowNull] StringBuilder ReasoningContentBuilder => field ??= new StringBuilder(ReasoningContent);
	/// <summary>
	/// 消息内容
	/// </summary>
	public string? Content { get; init; } = Content;
	/// <summary>
	/// 思维链
	/// </summary>
	public string? ReasoningContent { get; init; } = ReasoningContent;
	/// <summary>
	/// 参与角色名
	/// </summary>
	public string? Name { get; init; } = Name;

	/// <summary>
	/// 工具调用
	/// </summary>
	public JsonArray? ToolCalls { get; private set; } = ToolCalls;

	/// <summary>
	/// 转换为完成消息
	/// </summary>
	/// <returns></returns>
	/// <remarks>如果不调用<see cref="Merge(MessageDelta)"/>，是没有意义的</remarks>
	public MessageAssistant ToFinish()
	{
		return new MessageAssistant(ContentBuilder.ToString(), ReasoningContentBuilder.ToString(), Name, ToolCalls.Deserialize(SourceGenerationContext.Default.ListToolCall));
	}

	/// <summary>
	/// 合并消息增量
	/// </summary>
	/// <param name="messageDelta"></param>
	public void Merge(MessageDelta messageDelta)
	{
		ContentBuilder.Append(messageDelta.Content);
		ReasoningContentBuilder.Append(messageDelta.ReasoningContent);
		if (messageDelta.ToolCalls != null)
		{
			ToolCalls ??= new JsonArray();
			foreach (var toolCall in messageDelta.ToolCalls)
			{
				if (toolCall is JsonObject toolCallObj && (int?)toolCallObj["index"] is int indexInt && indexInt < ToolCalls.Count)
				{
					Merge(ToolCalls[indexInt], toolCall);
				}
				else
				{
					if (toolCall != null)
					{
						ToolCalls.Add(toolCall.DeepClone());
					}
				}
			}
		}
	}
	static void Merge(JsonNode? source, JsonNode? target)
	{
		switch (target, source)
		{
			case (JsonObject to, JsonObject so):
				foreach (var (name, node) in to)
				{
					if (so[name] is { } sourceNode)
					{
						Merge(sourceNode, node);
					}
					else
					{
						so[name] = node?.DeepClone();
					}
				}
				break;
			case (JsonValue tv, JsonValue sv) when sv.GetValueKind() == JsonValueKind.String:
				sv.ReplaceWith((string?)sv + (string?)tv);
				break;
		}
	}
}