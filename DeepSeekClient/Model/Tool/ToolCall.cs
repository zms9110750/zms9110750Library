
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace DeepSeekClient.Model.Tool;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolCallFunction), typeDiscriminator: "function")]
public   record ToolCall([property: JsonPropertyName("id"), JsonProperty("id")] string Id)
{
	[property: JsonIgnore] public string Key => $"";
	//[property: JsonIgnore] public abstract string Type { get; }
	//[property: JsonPropertyName("object"), JsonProperty("object")] public abstract ToolEntry Entry { get; }
}
public abstract record ToolEntry([property: JsonPropertyName("name"), JsonProperty("name")] string Name);
