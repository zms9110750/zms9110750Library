using zms9110750.DeepSeekClient.Model.ModelList;

namespace DeepSeekClient.Model.ModelList;
/// <summary>
/// 模型响应
/// </summary>
/// <param name="Object">其值仅可能为[list]</param>
/// <param name="Data">模型列表</param>
public record ModelResponse(
	  string Object,
	  List<ChatModel> Data);

