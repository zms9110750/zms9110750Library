namespace zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

/// <summary>
/// 函数工具描述接口
/// </summary>
public interface IToolRequestFunction : IToolFunction, IToolRequest
{
	/// <summary>
	/// 函数工具描述内容对象
	/// </summary>
	new IFunctionRequest Function { get; }
	IFunction IToolFunction.Function => Function;
	/// <summary>
	/// 调用函数工具
	/// </summary>
	/// <param name="call">提供参数的函数调用请求</param>
	/// <returns></returns>
	string Invok(IToolCallFunction call);
	string IToolRequest.Invok(IToolCall call)
	{
		return call is IToolCallFunction functionCall ? Invok(functionCall) : throw NotSupportException;
	}
}
/// <summary>
/// 函数工具描述内容对象
/// </summary>
public interface IFunctionRequest : IFunction
{
	/// <summary>
	/// 函数作用介绍
	/// </summary>
	string? Description { get; }
	/// <summary>
	/// 如果设置为 true，API 将在函数调用中使用 strict 模式，以确保输出始终符合函数的 JSON schema 定义。
	/// </summary>
	bool? Strict { get; }
	/// <summary>
	/// 参数列表内容体
	/// </summary>
	IFunctionParameter Parameters { get; }
}

/// <summary>
/// 参数列表
/// </summary>
public interface IFunctionParameter
{
	/// <summary>
	/// 架构类型，始终为object
	/// </summary>
	public string Type => "object";
	/// <summary>
	/// 参数列表
	/// </summary>
	public IReadOnlyDictionary<string, JsonObject> Properties { get; }
	/// <summary>
	/// 必填参数列表
	/// </summary>
	public IEnumerable<string> Required { get; }
}
