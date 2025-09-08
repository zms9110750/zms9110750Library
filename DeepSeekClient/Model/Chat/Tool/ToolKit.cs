using System.Collections;
using zms9110750.DeepSeekClient.Model.Chat.Messages;
using zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

namespace zms9110750.DeepSeekClient.Model.Chat.Tool;

/// <summary>
/// 工具箱
/// </summary>
public class ToolKit : ICollection<IToolRequest>
{
	/// <inheritdoc/>
	bool ICollection<IToolRequest>.IsReadOnly => ((ICollection<IToolRequest>)ToolSet).IsReadOnly;
	/// <inheritdoc/>
	public int Count => ToolSet.Count;

	HashSet<IToolRequest> ToolSet { get; } = [];
	/// <summary>
	/// 强制调用选定的工具。
	/// </summary>
	public ITool? ToolCall
	{
		get;
		set => field = value is IToolRequest tool && Contains(tool) ? value
			: throw new ArgumentOutOfRangeException(nameof(value), value, "The tool is not in the tool set.");
	}
	/// <summary>
	/// 是否强制要求调用工具。由AI任选工具进行调用。
	/// </summary>
	public bool ToolCallRequire { get; set; }
	/// <summary>
	/// 根据请求查找并调用工具
	/// </summary> 
	/// <returns>工具调用的工具消息</returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>目前只有函数工具</remarks>
	public MessageTool Invoke(IToolCall call)
	{
		string result;
		switch (call)
		{
			case IToolCallFunction toolCallFunction:
				var tool = ToolSet.OfType<IToolRequestFunction>()
					.Single(t => t.Function.Name == toolCallFunction.Function.Name);
				result = tool.Invok(toolCallFunction);
				break;
			default:
				throw new InvalidOperationException();
		}
		return new MessageTool(result, call.Id);
	}
	/// <inheritdoc/>
	public void Add(IToolRequest item)
	{
		((ICollection<IToolRequest>)ToolSet).Add(item);
	}

	/// <inheritdoc/>
	public void Clear()
	{
		ToolCall = null;
		((ICollection<IToolRequest>)ToolSet).Clear();
	}

	/// <inheritdoc/>
	public bool Contains(IToolRequest item)
	{
		return ((ICollection<IToolRequest>)ToolSet).Contains(item);
	}

	/// <inheritdoc/>
	public void CopyTo(IToolRequest[] array, int arrayIndex)
	{
		((ICollection<IToolRequest>)ToolSet).CopyTo(array, arrayIndex);
	}

	/// <inheritdoc/>
	public IEnumerator<IToolRequest> GetEnumerator()
	{
		return ((IEnumerable<IToolRequest>)ToolSet).GetEnumerator();
	}

	/// <inheritdoc/>
	public bool Remove(IToolRequest item)
	{
		if (ToolCall == item)
		{
			ToolCall = null;
		}
		return ((ICollection<IToolRequest>)ToolSet).Remove(item);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)ToolSet).GetEnumerator();
	}
}

