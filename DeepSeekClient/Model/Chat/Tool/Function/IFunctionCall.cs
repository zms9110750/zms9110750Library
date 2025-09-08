using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;

namespace zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

/// <summary>
/// 函数工具的调用请求
/// </summary>
public interface IToolCallFunction : IToolFunction, IToolCall, IDelta<IToolCallFunction>
{
	/// <summary>
	/// 函数工具的调用请求内容
	/// </summary>
	new IFunctionCallComplete Function { get; }
	IFunction IToolFunction.Function => Function;
	ToolType ITool.Type => ToolType.Function;
}
/// <summary>
/// 函数工具的调用请求内容对象
/// </summary> 
public interface IFunctionCall : IFunction
{
	/// <summary>
	/// 函数参数的Json字符串
	/// </summary>
	string Arguments { get; }
}
/// <summary>
/// 完全可用的函数工具的调用请求内容对象
/// </summary>
public interface IFunctionCallComplete : IFunctionCall
{
	/// <summary>
	/// 函数参数的Json对象
	/// </summary>
	JsonObject ArgumentsJson { get; }
}