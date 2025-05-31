using zms9110750.DeepSeekClient.Model.Tool.FunctionTool;

namespace zms9110750.DeepSeekClient.Model.Tool.FunctionCall;

/// <summary>
/// 工具箱扩展。
/// </summary>
public static class ToolCallExtensions
{
	/// <summary>
	/// 接受委托作为参数自动创建<see cref="ToolFunction"/>
	/// </summary>
	/// <param name="kit"></param>
	/// <param name="function"></param>
	public static void Add(this ToolKit kit, Delegate function)
	{
		kit.Add(new ToolFunction(function));
	}
}


