namespace zms9110750.DeepSeekClient.Model.Response.Logprob;

/// <summary>
/// token概率及候选项
/// </summary>
/// <param name="Token">token</param>
/// <param name="Logprob">概率</param>
/// <param name="TopLogprobs">候选项</param>
public record TokenAlternates(
	string Token,
	double Logprob,
	TokenProbability[] TopLogprobs
) : TokenProbability(Token, Logprob);
