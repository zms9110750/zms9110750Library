using System.Collections;
using System.Collections.ObjectModel;

namespace FiniteStateMachine.DirectedAcyclicWeight;

/// <summary>
/// 表示图中的强连通分量
/// </summary>
public abstract class Component<TValue, TWeight> : IReadOnlySet<GraphNode<TValue, TWeight>>
{
	/// <summary>
	/// 该分量所属的图
	/// </summary>
	public abstract Graph<TValue, TWeight> Graph { get; }

	/// <summary>
	/// 该分量中的节点
	/// </summary>
	public abstract IReadOnlySet<GraphNode<TValue, TWeight>> Nodes { get; }

	/// <inheritdoc/>
	public int Count => Nodes.Count;
	
	/// <inheritdoc/>
	public bool Contains(GraphNode<TValue, TWeight> item) => Nodes.Contains(item);
	
	/// <inheritdoc/>
	public IEnumerator<GraphNode<TValue, TWeight>> GetEnumerator() => Nodes.GetEnumerator();
	
	/// <inheritdoc/>
	public bool IsProperSubsetOf(IEnumerable<GraphNode<TValue, TWeight>> other) => Nodes.IsProperSubsetOf(other);
	
	/// <inheritdoc/>
	public bool IsProperSupersetOf(IEnumerable<GraphNode<TValue, TWeight>> other) => Nodes.IsProperSupersetOf(other);
	
	/// <inheritdoc/>
	public bool IsSubsetOf(IEnumerable<GraphNode<TValue, TWeight>> other) => Nodes.IsSubsetOf(other);
	
	/// <inheritdoc/>
	public bool IsSupersetOf(IEnumerable<GraphNode<TValue, TWeight>> other) => Nodes.IsSupersetOf(other);
	
	/// <inheritdoc/>
	public bool Overlaps(IEnumerable<GraphNode<TValue, TWeight>> other) => Nodes.Overlaps(other);
	
	/// <inheritdoc/>
	public bool SetEquals(IEnumerable<GraphNode<TValue, TWeight>> other) => Nodes.SetEquals(other);
	
	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Nodes).GetEnumerator();
	
	/// <inheritdoc/>
	public override string ToString()
	{
		return $"Component({Count})[{string.Join(", ", Nodes.Select(s => s.Value))} ]";
	}
}
