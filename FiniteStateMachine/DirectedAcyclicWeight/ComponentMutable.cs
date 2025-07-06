namespace FiniteStateMachine.DirectedAcyclicWeight;

internal class ComponentMutable<TValue, TWeight> : Component<TValue, TWeight>
{
	public override Graph<TValue, TWeight> Graph => GraphBuild;
	public required Graph<TValue, TWeight> GraphBuild { get; set; }
	public override HashSet<GraphNode<TValue, TWeight>> Nodes { get; } = new();
}
