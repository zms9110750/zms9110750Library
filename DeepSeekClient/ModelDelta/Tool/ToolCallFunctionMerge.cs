using System.Text;
using zms9110750.DeepSeekClient.Model.Tool.Functions;

namespace zms9110750.DeepSeekClient.ModelDelta.Tool;

public class ToolCallFunctionMerge(ToolCallFunction initial) : ToolCallMerge<ToolCallFunction>(initial)
{
	public string Name { get; } = initial.Function.Name;
	StringBuilder Arguments { get; } = new StringBuilder(initial.Function.Arguments);
	public override void Merge(ToolCallFunction source)
	{
		Arguments.Append(source.Function.Arguments);
	}
	public override ToolCallFunction ToFinish()
	{
		return new ToolCallFunction(Index, Id, new ToolCallFunctionChoice(Name, Arguments.ToString()));
	}
}