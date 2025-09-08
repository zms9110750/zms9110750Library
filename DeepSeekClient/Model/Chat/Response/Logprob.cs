namespace zms9110750.DeepSeekClient.Model.Chat.Response;
/// <summary>
/// 概率列表容器
/// </summary>
/// <param name="Content">内容的概率列表</param>
/// <param name="ReasoningContent">思维链的概率列表</param>
public record LogprobsContainer(TokenAlternates[]? Content, TokenAlternates[]? ReasoningContent = null);

/// <summary>
/// token概率
/// </summary>
/// <param name="Token">token</param>
/// <param name="Logprob">对数概率</param>
public record TokenProbability(
	string Token,
	double Logprob
);

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


