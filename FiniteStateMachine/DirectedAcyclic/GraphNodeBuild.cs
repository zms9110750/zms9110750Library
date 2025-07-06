namespace FiniteStateMachine.DirectedAcyclic;

public class GraphNodeBuild<TValue>
{
	public TValue Value
	{
		get;
		set
		{
			var old = field;
			field = value;
			ChangedValue?.Invoke(old,   value);
		}
	}
	public event Action<TValue, TValue>? ChangedValue;
	public HashSet<GraphNodeBuild<TValue>> OutEdges { get; set; } = new();
}
