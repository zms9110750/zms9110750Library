using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Response;
/*
public record FillInMiddleResponses(
	FillInMiddleResponses.Choice[] Choices,
	string Id,
	long Created,
	string model,
	string Systemfingerint,
	string Object,
	Usage Usage) : Responses(Id, Created, model, Systemfingerint, Object, Usage)
{
	[property: JsonPropertyName("choices"), JsonProperty("choices")] public override Choice[] Choices { get; } = Choices;
	public record Choice([property: JsonPropertyName("text"), JsonProperty("text")] string Text) : ChoicesCommon
	{
		[property: JsonPropertyName("logprobs"), JsonProperty("logprobs")] public Logprob? Logprobs { get; set; }
		public record Logprob(
			[property: JsonPropertyName("text_offset"), JsonProperty("text_offset")] int[] TextOffset,
			[property: JsonPropertyName("token_logprobs"), JsonProperty("token_logprobs")] double[] TokenLogprobs,
			[property: JsonPropertyName("tokens"), JsonProperty("tokens")] string[] Tokens,
			[property: JsonPropertyName("top_logprobs"), JsonProperty("top_logprobs")] ChatNoStreamResponses.ChatChoice.Logprob.LogprobContent.TopLogprob[] TopLogprobs);
	}
}
*/