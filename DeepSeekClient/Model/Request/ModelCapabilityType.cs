namespace zms9110750.DeepSeekClient.Model.Request;

/// <summary>
/// 采样温度预设情形
/// </summary>
public enum ModelCapabilityType
{
	/// <summary>
	/// 代码生成/数学解题
	/// </summary>
	CodeGeneration = 0,

	/// <summary>
	/// 数据抽取/分析
	/// </summary>
	DataAnalysis = 1000,

	/// <summary>
	/// 通用对话
	/// </summary>
	GeneralConversation = 1300,

	/// <summary>
	/// 翻译
	/// </summary>
	Translation = 1300,

	/// <summary>
	/// 创意类写作/诗歌创作
	/// </summary>
	CreativeWriting = 1500
}