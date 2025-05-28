using DeepSeekClient.Clint;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace DeepSeekClient.Model.Message;
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(UserMessage), typeDiscriminator: "user")]
[JsonDerivedType(typeof(AssistantMessage), typeDiscriminator: "assistant")]
[JsonDerivedType(typeof(SystemMessage), typeDiscriminator: "system")]
[JsonDerivedType(typeof(ToolMessage), typeDiscriminator: "tool")]
public abstract class Message : WithJsonObject
{
	[System.Text.Json.Serialization.JsonIgnore] public abstract string Role { get; }
	[property: JsonPropertyName("name"), JsonProperty("name")] public string? Name { get; set => Json["name"] = field = value; }
	[property: JsonPropertyName("content"), JsonProperty("content")] public string Content { get; init => Json["content"] = field = value; }
	protected Message(string? content, string? name = null)
	{
		Json["role"] = Role;
		Content = content ?? "";
		Name = name;
	}
	public static UserMessage NewUserMsg(string content, string? name = null)
	{
		return new UserMessage(content, name);
	}
	public static AssistantMessage NewAssistantMsg(string content, string? name = null)
	{
		return new AssistantMessage(content, "", name);
	}
	public static SystemMessage NewSystemMsg(string content, string? name = null)
	{
		return new SystemMessage(content, name);
	}
	public static PrefixMessage NewPrefixMsg(string prefixContent, string? name = null)
	{
		return new PrefixMessage(prefixContent, name);
	}
}
