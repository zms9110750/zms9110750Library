using DeepSeekClient.Model.Message;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Response;
public abstract record Responses(
	[property: JsonPropertyName("id"), JsonProperty("id")] string Id,
	[property: JsonPropertyName("created"), JsonProperty("created")] long Created,
	[property: JsonPropertyName("model"), JsonProperty("model")] string Model,
	[property: JsonPropertyName("system_fingerprint"), JsonProperty("system_fingerprint")] string SystemFingerint,
	[property: JsonPropertyName("object"), JsonProperty("object")] string Object,
	[property: JsonPropertyName("usage"), JsonProperty("usage")] Usage Usage)
{
	[property: JsonPropertyName("choices"), JsonProperty("choices")] public abstract ChoicesCommon[] Choices { get; }
}
public  record Responses<T>(
	 string Id,
	 long Created,
	 string Model,
	 string SystemFingerint,
	 string Object,
	 Usage Usage,
	 T[] Choices) : Responses(Id, Created, Model, SystemFingerint, Object, Usage) where T : ChoicesCommon
{
	[property: JsonPropertyName("choices"), JsonProperty("choices")] public override T[] Choices { get; } = Choices;
}
