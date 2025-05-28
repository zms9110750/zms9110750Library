using DeepSeekClient.Model.Tool;
using DeepSeekClient.NewModel.Tool;

namespace DeepSeekClient.Model.Tool;

public static class ToolExtensions
{
	public static void Add(this ToolKit kit, Delegate function)
	{
		kit.Add(new FunctionTool(function));
	} 
}