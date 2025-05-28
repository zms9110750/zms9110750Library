using DeepSeekClient.JsonConverter;
using DeepSeekClient.Model.Message;
using DeepSeekClient.Model.Tool;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Response;

public abstract record ChoicesCommon
{
	[property: JsonPropertyName("finish_reason"), JsonProperty("finish_reason")] public Finish? FinishReason { get; set; }
	[property: JsonPropertyName("index"), JsonProperty("index")] public int Index { get; set; }
}
public record ChatChoice(
			[property: JsonPropertyName("message"), JsonProperty("message")]AssistantMessage Message,
			[property: JsonPropertyName("logprobs"), JsonProperty("logprobs"),
			System.Text.Json.Serialization.JsonConverter(typeof(LogprobChatTextJsonConverter)),
			Newtonsoft.Json.JsonConverter(typeof(LogprobChatNewtonsoftConverter))] Logprob[] Logprobs
		) : ChoicesCommon;
public record FIMChoices([property: JsonPropertyName("text"), JsonProperty("text")] FillInMiddleMessage Text,
	[property: JsonPropertyName("logprobs"), JsonProperty("logprobs"),
	System.Text.Json.Serialization.JsonConverter(typeof(LogprobFIMJsonConverter)),
	 Newtonsoft.Json.JsonConverter(typeof(LogprobFIMJsonConverter))] Logprob[] Logprobs)
	: ChoicesCommon;
public record ChatChoices(
	[property: JsonPropertyName("logprobs"), JsonProperty("logprobs"),
	System.Text.Json.Serialization.JsonConverter(typeof(LogprobFIMJsonConverter)),
	 Newtonsoft.Json.JsonConverter(typeof(LogprobFIMJsonConverter))] Logprob[] Logprobs)
	: ChoicesCommon;
public record LogprobContent(
	[property: JsonPropertyName("token"), JsonProperty("token")] string Token,
	[property: JsonPropertyName("logprob"), JsonProperty("logprob")] double Logprob);

public record Logprob(
	string Token,
	double Logprob,
	[property: JsonPropertyName("top_logprobs"), JsonProperty("top_logprobs")] LogprobContent[] TopLogprobs)
	: LogprobContent(Token, Logprob);
