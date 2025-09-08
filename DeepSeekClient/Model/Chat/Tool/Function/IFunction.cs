namespace zms9110750.DeepSeekClient.Model.Chat.Tool.Function;
/// <summary>
/// 函数工具接口
/// </summary>
public interface IToolFunction : ITool
{
	/// <summary>
	/// 函数工具的内容对象
	/// </summary>
	IFunction Function { get; }
	ToolType ITool.Type => ToolType.Function; 
	private protected static NotImplementedException NotSupportException { get; } = new NotImplementedException("Not supported call tool type.");
}

/// <summary>
/// 函数工具的内容对象
/// </summary>
public interface IFunction
{
	/// <summary>
	/// 函数名称
	/// </summary>
	string Name { get; }
}