using zms9110750.DeepSeekClient.Model.Tool;
using zms9110750.DeepSeekClient.Model.Tool.Functions;
namespace zms9110750.DeepSeekClient.ModelDelta.Tool;

public abstract class ToolCallMerge<T>(ToolCall initial) : ToolCallMerge(initial), IMerge<T> where T : ToolCall
{
	public abstract void Merge(T source);
	public override void Merge(ToolCall source)
	{
		if (source is T toolCall)
		{
			Merge(toolCall);
		}
		else
		{
			throw new ArgumentException("Invalid type of ToolCall");
		}
	}
	public override abstract T ToFinish();
}
public abstract class ToolCallMerge(ToolCall initial) : IMerge<ToolCall>
{
	public int Index { get; } = initial.Index;
	public string Id { get; } = initial.Id;

	public abstract void Merge(ToolCall source);
	public abstract ToolCall ToFinish();
	public static ToolCallMerge Create(ToolCall initial)
	{
		return initial switch
		{
			ToolCallFunction toolCallFunction => new ToolCallFunctionMerge(toolCallFunction),
			_ => throw new ArgumentException("Invalid type of ToolCall"),
		};
	}
}
