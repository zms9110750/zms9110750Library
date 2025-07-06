namespace FiniteStateMachine.DirectedAcyclicWeight;

internal class GraphNodeMutable<TValue, TWeight> : GraphNode<TValue, TWeight>
{
	public override TValue Value => BuildValue;
	public required TValue BuildValue { get; set; }
	public override Dictionary<GraphNode<TValue, TWeight>, TWeight> OutEdges { get; } = new();
	public override HashSet<GraphNode<TValue, TWeight>> InEdges { get; } = new();
	public override Component<TValue, TWeight> Component => BuildComponent;
	public ComponentMutable<TValue, TWeight> BuildComponent { get; set; }
	public override HashSet<GraphNode<TValue, TWeight>> OutReachableNodes { get; } = new();
	public override HashSet<GraphNode<TValue, TWeight>> InReachableNodes { get; } = new();
	public override int Level => BuildLevel;
	public int BuildLevel { get; set; }
}
