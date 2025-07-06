using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Tool.FunctionCall;
using zms9110750.DeepSeekClient.Model.Tool.FunctionTool;

namespace zms9110750.DeepSeekClient.Model.Tool;
/// <summary>
/// 工具箱
/// </summary>
public class ToolKit : ICollection<Tool>
{
	/// <inheritdoc/>
	public int Count => ((ICollection<Tool>)ToolSet).Count;

	/// <inheritdoc/>
	public bool IsReadOnly => ((ICollection<Tool>)ToolSet).IsReadOnly;


	HashSet<Tool> ToolSet { get; } = new HashSet<Tool>();

	/// <inheritdoc/>
	public void Add(Tool item)
	{
		((ICollection<Tool>)ToolSet).Add(item);
	}

	/// <inheritdoc/>
	public void Clear()
	{
		((ICollection<Tool>)ToolSet).Clear();
	}

	/// <inheritdoc/>
	public bool Contains(Tool item)
	{
		return ((ICollection<Tool>)ToolSet).Contains(item);
	}

	/// <inheritdoc/>
	public void CopyTo(Tool[] array, int arrayIndex)
	{
		((ICollection<Tool>)ToolSet).CopyTo(array, arrayIndex);
	}

	/// <inheritdoc/>
	public IEnumerator<Tool> GetEnumerator()
	{
		return ((IEnumerable<Tool>)ToolSet).GetEnumerator();
	}

	/// <summary>
	/// 根据请求查找并调用工具
	/// </summary>
	/// <param name="call"></param>
	/// <param name="options"></param>
	/// <returns>工具调用的工具消息</returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>目前只有函数工具</remarks>
	public MessageTool Invoke(ToolCall call, JsonSerializerOptions? options = null)
	{
		switch (call)
		{
			case ToolCallFunction toolCallFunction:
				var tool = ToolSet.OfType<ToolFunction>().Single(t => t.Function.Name == toolCallFunction.Function.Name);
				var result = tool.Invoke(toolCallFunction, options);
				return new MessageTool(result.ToJsonString(options ?? SourceGenerationContext.UnsafeRelaxed), toolCallFunction.Id);
			default:
				throw new InvalidOperationException();
		}
	}
	/// <inheritdoc/>
	public bool Remove(Tool item)
	{
		return ((ICollection<Tool>)ToolSet).Remove(item);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)ToolSet).GetEnumerator();
	}
}


