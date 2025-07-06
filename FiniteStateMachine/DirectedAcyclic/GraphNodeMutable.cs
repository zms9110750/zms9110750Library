namespace FiniteStateMachine.DirectedAcyclic;

internal class GraphNodeMutable<TValue> : GraphNode<TValue>
{
	public override TValue Value => BuildValue;
	public required TValue BuildValue { get; set; }
	public override HashSet<GraphNode<TValue>> OutEdges { get; } = new();
	public override HashSet<GraphNode<TValue>> InEdges { get; } = new();
	public override Component<TValue> Component => BuildComponent;
	public ComponentMutable<TValue> BuildComponent { get; set; }
	public override HashSet<GraphNode<TValue>> OutReachableNodes { get; } = new();
	public override HashSet<GraphNode<TValue>> InReachableNodes { get; } = new();
	public override int Level => BuildLevel;
	public int BuildLevel { get; set; }
}
