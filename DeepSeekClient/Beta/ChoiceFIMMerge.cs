using System.Text;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.ModelDelta;

namespace zms9110750.DeepSeekClient.Beta;

public class ChoiceFIMMerge(ChoiceFIM source) : IMerge<ChoiceFIM>
{
	int Index { get; } = source.Index;
	StringBuilder Text { get; } = new StringBuilder(source.Text);
	List<LogprobFIM> Logprobs { get; } = new List<LogprobFIM>() { source.Logprobs! };
	FinishReason? FinishReason { get; set; } = source.FinishReason;
	public void Merge(ChoiceFIM source)
	{
		Text.Append(source.Text);
		Logprobs.Add(source.Logprobs!);
		FinishReason ??= source.FinishReason;
	}

	public ChoiceFIM ToFinish()
	{
		var a = Logprobs.OfType<LogprobFIM>().ToArray();


		return new ChoiceFIM(Text.ToString(), Index, new LogprobFIM(
			a.SelectMany(s => s.Tokens).ToArray()
			, a.SelectMany(s => s.TokenLogprobs).ToArray()
			, a.SelectMany(s => s.TopLogprobs).ToArray()
			), FinishReason);
	}
}
