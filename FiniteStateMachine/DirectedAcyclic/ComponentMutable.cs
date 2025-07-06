using FiniteStateMachine.DirectedAcyclicWeight;

namespace FiniteStateMachine.DirectedAcyclic;

internal class ComponentMutable<TValue> : Component<TValue>
{
	public override Graph<TValue> Graph => GraphBuild;
	public required Graph<TValue> GraphBuild { get; set; }
	public override HashSet<GraphNode<TValue>> Nodes { get; } = new();
}
