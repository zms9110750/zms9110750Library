using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Response;

public record ChatStreamResponses(
	ChatStreamResponses.Choice[] Choices,
	string Id,
	long Created,
	string Model,
	string Systemfingerint,
	string Object,
	Usage Usage) : Responses(Id, Created, Model, Systemfingerint, Object, Usage)
{
	[property: JsonPropertyName("choices"), JsonProperty("choices")] public override Choice[] Choices { get; } = Choices;
	public record Choice([property: JsonPropertyName("delta"), JsonProperty("delta")] Message.AssistantMessage Delta) : ChoicesCommon;
}