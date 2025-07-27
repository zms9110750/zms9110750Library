using System.Diagnostics.CodeAnalysis;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Response.Logprob;
using zms9110750.DeepSeekClient.ModelDelta.Messages;
using zms9110750.DeepSeekClient.ModelDelta.Response;

namespace zms9110750.DeepSeekClient.Beta;
/// <summary>
/// 中间补全用的内容的选择列表
/// </summary>
/// <param name="Text">补全的文字</param>
/// <param name="Index">索引</param>
/// <param name="Logprobs">概率列表</param>
/// <param name="FinishReason">结束原因</param>
public record ChoiceFIM(string Text, int Index, LogprobFIM? Logprobs, FinishReason? FinishReason);
