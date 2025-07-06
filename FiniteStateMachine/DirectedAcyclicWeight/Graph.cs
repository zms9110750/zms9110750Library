namespace FiniteStateMachine.DirectedAcyclicWeight;
/// <summary>
/// 有向无环图
/// </summary>
/// <typeparam name="TValue">值类型</typeparam>
/// <typeparam name="TWeight">权重类型</typeparam>
public abstract class Graph<TValue, TWeight>
{
	/// <summary>
	/// 图中的所有节点（只读）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> Nodes { get; }

	/// <summary>
	/// 图中的所有强连通分量（只读）
	/// </summary>
	public abstract IReadOnlySet<Component<TValue, TWeight>> Components { get; }

	/// <summary>
	/// 游离节点集合（既没有出边也没有入边的节点）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> IsolatedNodes { get; }

	/// <summary>
	/// 无入边节点集合（入度为0的节点）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> SourceNodes { get; }

	/// <summary>
	/// 无出边节点集合（出度为0的节点）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> SinkNodes { get; }
}
