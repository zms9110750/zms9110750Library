using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Response.Logprob;
using zms9110750.DeepSeekClient.ModelDelta.Messages;

namespace zms9110750.DeepSeekClient.ModelDelta.Response;

public class ChoiceMerge(ChoiceDelta source) : IMerge<ChoiceDelta>
{
	int Index { get; } = source.Index;
	List<TokenAlternates>? Logprobs { get; } = source.Logprobs?.Content.ToList();
	MessageAssistantMerge Delta { get; } = new MessageAssistantMerge(source.Delta);
	FinishReason? FinishReason { get; set; } = source.FinishReason;
	public void Merge(ChoiceDelta source)
	{
		if (Logprobs != null && source.Logprobs != null)
		{
			Logprobs.AddRange(source.Logprobs.Content);
		}
		Delta.Merge(source.Delta);
		FinishReason??= source.FinishReason;
	}

	public ChoiceDelta ToFinish()
	{
		return new ChoiceDelta(Index, Logprobs == null ? null : new LogprobsContainer(Logprobs), Delta.ToFinish(), FinishReason);
	}
}
