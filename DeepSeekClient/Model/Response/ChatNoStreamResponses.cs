using DeepSeekClient.JsonConverter;
using DeepSeekClient.Model.Message;
using DeepSeekClient.Model.Tool;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Response;

public record ChatNoStreamResponses(
	 ChatChoice[] Choices,
	string Id,
	long Created,
	string Model,
	string SystemFingerint,
	string Object,
	Usage Usage) : Responses(Id, Created, Model, SystemFingerint, Object, Usage)
{
	[property: JsonPropertyName("choices"), JsonProperty("choices")] public override ChatChoice[] Choices { get; } = Choices;
}
