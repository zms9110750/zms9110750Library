using DeepSeekClient.Model.Response;

namespace zms9110750.DeepSeekClient.Beta;
/// <summary>
/// 中间补全的回应类
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Object">对象的类型, 其值为 chat.completion。</param>
/// <param name="Created">创建聊天完成时的 Unix 时间戳（以秒为单位）。</param>
/// <param name="Model">生成该 completion 的模型名。</param>
/// <param name="SystemFingerint">模型运行时的后端配置的指纹。</param>
/// <param name="Choices">模型生成的补全内容的选择列表。但是永远只有一个元素。</param>
/// <param name="Usage">该对话补全请求的用量信息。</param>
public record ChatResponseFIM(
	 string Id,
	 string Object,
	 long Created,
	 string Model,
	 string SystemFingerint,
	 List<ChoiceFIM> Choices,
	 Usage Usage);
