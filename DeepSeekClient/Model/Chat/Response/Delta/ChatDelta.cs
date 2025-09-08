using zms9110750.DeepSeekClient.Model.Chat.Messages;

namespace zms9110750.DeepSeekClient.Model.Chat.Response.Delta;
/// <summary>
/// AI流式聊天选择。
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Logprobs">token概率</param>
/// <param name="Delta">增量消息。仅当流式才包含</param> 
/// <param name="FinishReason">结束原因</param>
public record ChatDelta(
	 int Index,
	 LogprobsContainer? Logprobs,
	 MessageAssistant Delta,
	 FinishReason? FinishReason) : IChatChoice 
{
	MessageAssistant IChatChoice.Message => Delta;
	IMerge<IChatChoice> IDelta<IChatChoice>.CreateMerge()
	{
		return new ChatDeltaMerge(this);
	}
}
internal class ChatDeltaMerge(IChatChoice delta) : IMerge<IChatChoice>
{
	int Index { get; } = delta.Index;
	List<TokenAlternates> Logprobs { get; } = new List<TokenAlternates>(delta.Logprobs?.Content ?? []);
	List<TokenAlternates> LogprobsReasoning { get; } = new List<TokenAlternates>(delta.Logprobs?.ReasoningContent ?? []);
	IMerge<MessageAssistant> Delta { get; set; } = ((IDelta<MessageAssistant>)delta.Message).CreateMerge();
	FinishReason? FinishReason { get; set; } = delta.FinishReason;

	public void Merge(IChatChoice other)
	{
		if (other.Index != Index)
		{
			throw new ArgumentException("Index not match.");
		}
		Logprobs.AddRange(other.Logprobs?.Content ?? []);
		LogprobsReasoning.AddRange(other.Logprobs?.ReasoningContent ?? []);
		Delta.Merge(other.Message);
		FinishReason ??= other.FinishReason;
	}

	public IChatChoice ToFinish()
	{
		LogprobsContainer? container = null;
		if (Logprobs.Count + LogprobsReasoning.Count > 0)
		{
			var logprobs = Logprobs.Count > 0 ? Logprobs.ToArray() : null;
			var logprobsReasoning = LogprobsReasoning.Count > 0 ? LogprobsReasoning.ToArray() : null;
			container = new LogprobsContainer(logprobs, logprobsReasoning);
		}

		return new ChatDelta(Index, container, Delta.ToFinish(), FinishReason ?? Response.FinishReason.ConnectionAborted);
	}
}
