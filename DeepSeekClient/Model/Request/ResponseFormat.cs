using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Request;
/// <summary>
/// 指定模型必须输出的格式。
/// </summary>
public enum ResponseFormat
{
	/// <summary>
	/// 输出为文本格式。
	/// </summary>
	[JsonStringEnumMemberName("text")] Text,
	/// <summary>
	/// 输出为JSON格式。
	/// </summary>
	[JsonStringEnumMemberName("json_object")] JsonObject
}

