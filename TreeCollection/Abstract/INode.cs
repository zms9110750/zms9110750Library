using System.Diagnostics.CodeAnalysis;

namespace zms9110750.TreeCollection.Abstract;

public interface INode<T> where T : INode<T>
{
	T? Parent { get; }
	T Root { get; }
	int Depth { get; }
}
