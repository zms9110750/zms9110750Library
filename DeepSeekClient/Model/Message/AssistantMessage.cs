using DeepSeekClient.Model.Tool;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Message;

public class AssistantMessage : Message
{
	public override string Role => "assistant";
	public string? ReasoningContent { get; init; }
	[property: JsonPropertyName("tool_calls"), JsonProperty("tool_calls")] public ToolCallFunction[]? ToolCall
	{ get; init => Json["tool_calls"] = new JsonArray([.. (field = value).Select(s => System.Text.Json.JsonSerializer.SerializeToNode(s))]); }
	public AssistantMessage(string? content, string? reasoningContent=null, string? name = null) : base(content, name)
	{
		ReasoningContent = reasoningContent;
	}
}
