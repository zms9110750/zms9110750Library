using DeepSeekClient.JsonConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Tool;
public record ToolCallFunction(string Id, ToolEntryFunction Entry) : ToolCall(Id)
{
	//[System.Text.Json.Serialization.JsonIgnore] public override string Type => "function";
	[property: JsonPropertyName("function"), JsonProperty("function")] public   ToolEntryFunction Entry { get; } = Entry;
}
public   record ToolEntryFunction(string Name, [property: JsonPropertyName("arguments"), JsonProperty("arguments")] string? Arguments) : ToolEntry(Name);