namespace zms9110750.DeepSeekClient.Model.ModelList;

/// <summary>
/// 模型
/// </summary>
/// <param name="Id">模型的标识符</param>
/// <param name="Object">对象的类型，其值仅可能为[model]</param>
/// <param name="OwnedBy">拥有该模型的组织。</param>
public record ChatModel(
	 string Id,
	 string Object,
	 string OwnedBy)
{
	/// <summary>
	/// 普通模型
	/// </summary>
	public static ChatModel V3 { get; } = new ChatModel("deepseek-chat", "model", "deepseek");
	/// <summary>
	/// 推理模型
	/// </summary>
	public static ChatModel R1 { get; } = new ChatModel("deepseek-reasoner", "model", "deepseek");
}

