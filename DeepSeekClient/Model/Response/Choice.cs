using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Response.Logprob; 
namespace zms9110750.DeepSeekClient.Model.Response;
/// <summary>
/// AI聊天选择。
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Logprobs">token概率</param>
/// <param name="Message">消息</param> 
/// <param name="FinishReason">结束原因</param>
public record Choice(
	 int Index,
	 LogprobsContainer? Logprobs,
	 MessageAssistant Message,
	 FinishReason? FinishReason);