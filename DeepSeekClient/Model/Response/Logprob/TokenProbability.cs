using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Response.Logprob;
/// <summary>
/// token概率
/// </summary>
/// <param name="Token">token</param>
/// <param name="Logprob">对数概率</param>
public record TokenProbability(
	string Token,
	double Logprob
)
{
	/// <summary>
	/// 转为小数概率
	/// </summary>
	[JsonIgnore] public double Probability => Math.Exp(Logprob);
}
