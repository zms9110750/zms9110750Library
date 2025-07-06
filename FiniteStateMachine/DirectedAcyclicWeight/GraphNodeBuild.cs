namespace FiniteStateMachine.DirectedAcyclicWeight;

/// <summary>
/// 节点构建器
/// </summary>
/// <typeparam name="TValue">值类型</typeparam>
/// <typeparam name="TWeight">权重类型</typeparam>
public class GraphNodeBuild<TValue, TWeight>
{
	/// <summary>
	/// 节点值
	/// </summary>
	public required TValue Value
	{
		get;
		set
		{
			var old = field;
			field = value;
			ChangedValue?.Invoke(old, value);
		}
	}
	/// <summary>
	/// 更改节点值事件
	/// </summary>
	public event Action<TValue, TValue>? ChangedValue;

	/// <summary>
	/// 节点出边
	/// </summary>
	public Dictionary<GraphNodeBuild<TValue, TWeight>, TWeight> OutEdges { get; set; } = new();
}
