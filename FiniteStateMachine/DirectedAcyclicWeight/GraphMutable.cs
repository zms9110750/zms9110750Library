namespace FiniteStateMachine.DirectedAcyclicWeight;

internal class GraphMutable<TValue, TWeight> : Graph<TValue, TWeight>
{
	public override HashSet<GraphNode<TValue, TWeight>> Nodes { get; } = new();
	public override HashSet<Component<TValue, TWeight>> Components { get; } = new();
	public override HashSet<GraphNode<TValue, TWeight>> IsolatedNodes { get; } = new();
	public override HashSet<GraphNode<TValue, TWeight>> SourceNodes { get; } = new();
	public override HashSet<GraphNode<TValue, TWeight>> SinkNodes { get; } = new();
}
