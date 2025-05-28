namespace zms9110750.TreeCollection.Abstract;

public interface IValue<T> : IEquatable<T>
{
	public T Value { get; }
	bool IEquatable<T>.Equals(T? other) => EqualityComparer<T?>.Default.Equals(Value, other);
}