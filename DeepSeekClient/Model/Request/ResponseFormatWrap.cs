using System.Diagnostics.CodeAnalysis;

namespace zms9110750.DeepSeekClient.Model.Request;

/// <summary>
/// 模型输出格式的API拟合包装器
/// </summary>
/// <param name="Type">格式设置</param>
/// <remarks>即便设置了Json格式也要告知AI有什么字段</remarks>
public record ResponseFormatWrap(ResponseFormat Type)
{
	/// <summary>
	/// 输出为文本格式的包装器
	/// </summary>
	[field: AllowNull] public static ResponseFormatWrap Text => field ??= new ResponseFormatWrap(ResponseFormat.Text);

	/// <summary>
	/// 输出为JSON格式的包装器
	/// </summary>
	[field: AllowNull] public static ResponseFormatWrap JsonObject => field ??= new ResponseFormatWrap(ResponseFormat.JsonObject);
}

