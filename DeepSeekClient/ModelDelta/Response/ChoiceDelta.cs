using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Response.Logprob;

namespace zms9110750.DeepSeekClient.ModelDelta.Response;
/// <summary>
/// AI聊天选择。
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Logprobs">token概率</param>
/// <param name="Delta">增量消息。仅当流式才包含</param> 
/// <param name="FinishReason">结束原因</param>
public record ChoiceDelta(
	 int Index,
	 LogprobsContainer? Logprobs,
	 MessageAssistant Delta,
	 FinishReason? FinishReason)
{
	public Choice ToChoice()
	{
		return new Choice(Index, Logprobs, Delta, FinishReason);
	}
}
