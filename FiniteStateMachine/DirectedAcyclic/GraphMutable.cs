namespace FiniteStateMachine.DirectedAcyclic;

internal class GraphMutable<TValue> : Graph<TValue>
{
	public override HashSet<GraphNode<TValue>> Nodes { get; } = new();
	public override HashSet<Component<TValue>> Components { get; } = new();
	public override HashSet<GraphNode<TValue>> IsolatedNodes { get; } = new();
	public override HashSet<GraphNode<TValue>> SourceNodes { get; } = new();
	public override HashSet<GraphNode<TValue>> SinkNodes { get; } = new();
}
