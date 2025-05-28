using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Response;

public record Usage(
		[property: JsonPropertyName("completion_tokens"), JsonProperty("completion_tokens")] int CompletionTokens,
		[property: JsonPropertyName("prompt_tokens"), JsonProperty("prompt_tokens")] int PromptTokens,
		[property: JsonPropertyName("prompt_cache_hit_tokens"), JsonProperty("prompt_cache_hit_tokens")] int PromptCacheHitTokens,
		[property: JsonPropertyName("prompt_cache_miss_tokens"), JsonProperty("prompt_cache_miss_tokens")] int PromptCacheMissTokens,
		[property: JsonPropertyName("total_tokens"), JsonProperty("total_tokens")] int TotalTokens,
		[property: JsonPropertyName("completion_tokens_details"), JsonProperty("completion_tokens_details"), Obsolete("文档存在。但是返回Json没有")] Usage.CompletionTokensDetail CompletionTokensDetails,
	    [property: JsonPropertyName("prompt_tokens_details"), JsonProperty("prompt_tokens_details")] Usage.PromptTokensDetail PromptTokensDetails)
{
	public record CompletionTokensDetail([property: JsonPropertyName("reasoning_tokens"), JsonProperty("reasoning_tokens")] int ReasoningTokens);
	public record PromptTokensDetail([property: JsonPropertyName("cached_tokens"), JsonProperty("cached_tokens")] int CachedTokens);
}