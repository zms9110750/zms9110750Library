namespace DeepSeekClient.Model.Message;

public class ToolMessage : Message
{
	public override string Role => "tool";
	public string ToolCallId { get; init => Json["tool_call_id"] = field = value; }
	public ToolMessage(string content, string toolCallId, string? name = null) : base(content, name)
	{
		ToolCallId = toolCallId;
	}
}
