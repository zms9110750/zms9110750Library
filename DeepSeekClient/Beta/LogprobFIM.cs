namespace zms9110750.DeepSeekClient.Beta;
/// <summary>
/// 中间填充版本的token概率
/// </summary>
/// <param name="Tokens">token列表</param>
/// <param name="TokenLogprobs">token对应的概率列表</param>
/// <param name="TopLogprobs">这个位置的token的候选项及其概率列表</param>
public record LogprobFIM(string[] Tokens, double[] TokenLogprobs, Dictionary<string, double> []TopLogprobs); 