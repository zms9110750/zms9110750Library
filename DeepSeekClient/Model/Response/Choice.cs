using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Response.Delta;
using zms9110750.DeepSeekClient.Model.Response.Logprob;

namespace zms9110750.DeepSeekClient.Model.Response;
/// <summary>
/// AI聊天选择。
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Logprobs">token概率</param>
/// <param name="Message">消息</param>
/// <param name="Delta">增量消息。仅当流式才包含</param>
/// <param name="Text">FIM的文本</param>
/// <param name="FinishReason">结束原因</param>
public record Choice(
	 int Index,
	 LogprobsContainer? Logprobs,
	 MessageAssistant Message,
	 MessageDelta? Delta,
	 string? Text,
	 FinishReason? FinishReason)
{
	/// <summary>
	/// 把增量消息合并为完整消息。
	/// </summary>
	/// <returns></returns>
	public Choice ToFinish()
	{
		return new Choice(
			Index,
			Logprobs,
			Delta?.ToFinish()!,
			null,
			null,
			FinishReason);
	}
}
