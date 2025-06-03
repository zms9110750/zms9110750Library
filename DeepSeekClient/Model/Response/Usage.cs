namespace zms9110750.DeepSeekClient.Model.Response;
/// <summary>
/// 表示API调用的token使用情况统计
/// </summary>
/// <param name="CompletionTokens">完成部分消耗的token数量</param>
/// <param name="PromptTokens">提示部分消耗的token数量</param>
/// <param name="PromptCacheHitTokens">提示缓存命中的token数量</param>
/// <param name="PromptCacheMissTokens">提示缓存未命中的token数量</param>
/// <param name="TotalTokens">总消耗token数量</param>
/// <param name="CompletionTokensDetails">完成部分token的详细使用情况</param>
/// <param name="PromptTokensDetails">提示部分token的详细使用情况</param>
public record Usage(
	int CompletionTokens,
	int PromptTokens,
	int PromptCacheHitTokens,
	int PromptCacheMissTokens,
	int TotalTokens,
	Usage.CompletionTokensDetail CompletionTokensDetails,
	Usage.PromptTokensDetail PromptTokensDetails)
{
	/// <summary>
	/// 完成部分token的详细使用情况
	/// </summary>
	/// <param name="ReasoningTokens">用于思维链推理的token数量</param>
	public record CompletionTokensDetail(int ReasoningTokens);

	/// <summary>
	/// 提示部分token的详细使用情况
	/// </summary>
	/// <param name="CachedTokens">从缓存中读取的token数量</param>
	public record PromptTokensDetail(int CachedTokens);
}