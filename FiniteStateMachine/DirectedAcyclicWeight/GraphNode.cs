namespace FiniteStateMachine.DirectedAcyclicWeight;

/// <summary>
/// 图节点
/// </summary>
/// <typeparam name="TValue">值类型</typeparam>
/// <typeparam name="TWeight">权重类型</typeparam>
public abstract class GraphNode<TValue, TWeight>
{
	/// <summary>
	/// 节点的值
	/// </summary>
	public abstract TValue Value { get; }

	/// <summary>
	/// 从该节点出发的出边（只读）
	/// </summary>
	public abstract IReadOnlyDictionary<GraphNode<TValue, TWeight>, TWeight> OutEdges { get; }

	/// <summary>
	/// 指向该节点的入边（只读）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> InEdges { get; }

	/// <summary>
	/// 节点所属的连通分量
	/// </summary>
	public abstract Component<TValue, TWeight> Component { get; }

	/// <summary>
	/// 从该节点通过出边可达的所有节点（包括自身）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> OutReachableNodes { get; }

	/// <summary>
	/// 可以通过入边到达该节点的所有节点（包括自身）
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> InReachableNodes { get; }

	/// <summary>
	/// 节点的层级（到达最远的0入度节点的路径长度）
	/// </summary>
	public abstract int Level { get; } 

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"TNode( {Value} )[{string.Join(", ", OutEdges.Select(s => $"{s.Key}({s.Value})"))} ]";
	}
}
