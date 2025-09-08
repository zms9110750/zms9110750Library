using System.Text;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;

namespace zms9110750.DeepSeekClient.Model.Chat.Response;
/// <summary>
/// 中间补全用的内容的选择列表
/// </summary>
/// <param name="Text">补全的文字</param>
/// <param name="Index">索引</param>
/// <param name="Logprobs">概率列表</param>
/// <param name="FinishReason">结束原因</param>
public record FIMChoice(
	int Index,
	string Text,
	LogprobFIM? Logprobs,
	FinishReason? FinishReason) : IDelta<FIMChoice>,IIndex
{
	IMerge<FIMChoice> IDelta<FIMChoice>.CreateMerge()
	{
		return new ChoiceFIMMerge(this);
	}
}

internal class ChoiceFIMMerge(FIMChoice source) : IMerge<FIMChoice>
{
	int Index { get; } = source.Index;
	StringBuilder Text { get; } = new StringBuilder(source.Text);
	List<LogprobFIM> Logprobs { get; } = new List<LogprobFIM>() { source.Logprobs! };
	FinishReason? FinishReason { get; set; } = source.FinishReason;
	public void Merge(FIMChoice source)
	{
		Text.Append(source.Text);
		Logprobs.Add(source.Logprobs!);
		FinishReason ??= source.FinishReason;
	}

	public FIMChoice ToFinish()
	{
		var a = Logprobs.OfType<LogprobFIM>().ToArray();
		return new FIMChoice(Index, Text.ToString(), new LogprobFIM(
			a.SelectMany(s => s.Tokens).ToArray()
			, a.SelectMany(s => s.TokenLogprobs).ToArray()
			, a.SelectMany(s => s.TopLogprobs).ToArray()
			), FinishReason);
	}
}
/// <summary>
/// 中间填充版本的token概率
/// </summary>
/// <param name="Tokens">token列表</param>
/// <param name="TokenLogprobs">token对应的概率列表</param>
/// <param name="TopLogprobs">这个位置的token的候选项及其概率列表</param>
public record LogprobFIM(string[] Tokens, double[] TokenLogprobs, Dictionary<string, double>[] TopLogprobs);