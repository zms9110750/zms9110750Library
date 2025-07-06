using System.Text;
using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.ModelDelta.Tool;
namespace zms9110750.DeepSeekClient.ModelDelta.Messages;

public class MessageAssistantMerge() : IMerge<MessageAssistant>
{
	StringBuilder Content { get; } = new StringBuilder();
	StringBuilder ReasoningContent { get; } = new StringBuilder();
	List<ToolCallMerge> ToolCalls { get; } = new List<ToolCallMerge>();
	public MessageAssistantMerge(MessageAssistant source) : this()
	{
		Merge(source);
	}

	public void Merge(MessageAssistant source)
	{
		Content.Append(source.Content);
		ReasoningContent.Append(source.ReasoningContent);
		if (source.ToolCalls != null)
		{
			foreach (var toolCall in source.ToolCalls)
			{
				if (toolCall.Index < ToolCalls.Count)
				{
					ToolCalls[toolCall.Index].Merge(toolCall);
				}
				else if (toolCall.Index == ToolCalls.Count)
				{
					ToolCalls.Add(ToolCallMerge.Create(toolCall));
				}
				else
				{
					throw new ArgumentException("Invalid index of ToolCall");
				}
			}
		}
	}
	public MessageAssistant ToFinish()
	{
		return new MessageAssistant(Content.ToString(), ReasoningContent?.ToString(), null, ToolCalls.Count == 0 ? null : ToolCalls.Select(s => s.ToFinish()).ToArray());
	}
}
