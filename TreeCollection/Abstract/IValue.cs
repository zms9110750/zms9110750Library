namespace zms9110750.TreeCollection.Abstract;
/// <summary>
/// 包含值属性的接口
/// </summary>
/// <typeparam name="T">值类型</typeparam>
public interface IValue<T> : IEquatable<T>
{
	/// <summary>
	/// 值属性
	/// </summary>
	public T Value { get; }
	bool IEquatable<T>.Equals(T? other) => EqualityComparer<T?>.Default.Equals(Value, other);
}