using System.Diagnostics.CodeAnalysis;

namespace zms9110750.TreeCollection.Abstract;
public abstract class RootNode<TV, TN>(TV value) : INode<TN>, IValue<TV> where TN : RootNode<TV, TN>
{
	public TV Value { get; set; } = value;
	[field: AllowNull]
	[property: AllowNull]
	public TN Root { get => field ??= Parent?.Root ?? (TN)this; protected set; }
	public int Depth { get => field < 0 ? field = Parent?.Depth + 1 ?? 0 : field; protected set; }
	protected abstract IEnumerable<TN> ChildrenNode { get; }
	public TN? Parent
	{
		get; protected set
		{
			field = value;
			Root = null;
			Depth = -1;
			foreach (var item in ChildrenNode)
			{
				item.Parent = (TN)this;
			}
		}
	}
}