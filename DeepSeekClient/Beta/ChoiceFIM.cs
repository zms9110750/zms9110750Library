using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace zms9110750.DeepSeekClient.Beta;
/// <summary>
/// 中间补全用的内容的选择列表
/// </summary>
/// <param name="Text">补全的文字</param>
/// <param name="Index">索引</param>
/// <param name="Logprobs">概率列表</param>
public record ChoiceFIM(string Text, int Index, LogprobFIM? Logprobs)
{
	[field: AllowNull] private StringBuilder TextBuilder => field ??= new StringBuilder(Text);
	[field: AllowNull] private List<LogprobFIM> LogprobsList => field ??= Logprobs == null ? new() : new() { Logprobs };
	/// <summary>
	/// 合并一个增量
	/// </summary>
	/// <param name="value"></param>
	public void Merge(ChoiceFIM value)
	{
		TextBuilder.Append(value.Text);
		if (value.Logprobs != null)
		{
			LogprobsList.Add(value.Logprobs);
		}
	}
	/// <summary>
	/// 生成一个具有把增量内容合并后的结果
	/// </summary>
	/// <returns></returns>
	/// <remarks>如果不调用<see cref="Merge(ChoiceFIM)"/>，是没有意义的</remarks>

	public ChoiceFIM ToFinish()
	{
		return new(TextBuilder.ToString(), Index, new LogprobFIM(
			LogprobsList.SelectMany(s => s.Tokens).ToArray(),
			LogprobsList.SelectMany(s => s.TokenLogprobs).ToArray(),
			LogprobsList.SelectMany(s => s.TopLogprobs).ToArray()
			));
	}
}
