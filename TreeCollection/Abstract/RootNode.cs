using System.Diagnostics.CodeAnalysis;

namespace zms9110750.TreeCollection.Abstract;
/// <summary>
/// 实现了基本属性的抽象类
/// </summary>
/// <typeparam name="TValue">节点储存值的类型</typeparam>
/// <typeparam name="TNode">自我约束</typeparam>
/// <param name="value">初始值</param>
public abstract class RootNode<TValue, TNode>(TValue value) : INode<TNode>, IValue<TValue> where TNode : RootNode<TValue, TNode>
{
	/// <summary>
	/// 结点值
	/// </summary>
	public TValue Value { get; set; } = value;

	/// <inheritdoc/> 
	[property: AllowNull]
	public TNode Root { get => field ??= Parent?.Root ?? (TNode)this; protected set; }
	/// <inheritdoc/>
	public int Depth { get => field < 0 ? field = Parent?.Depth + 1 ?? 0 : field; private set; }

	/// <summary>
	/// 子结点集合
	/// </summary>
	/// <remarks><see cref="Parent"/>的set会自动递归子节点。需要引用这个属性进行遍历</remarks>
	protected abstract IEnumerable<TNode> ChildrenNode { get; }

	/// <summary>
	/// 父结点
	/// </summary>
	/// <remarks>派生类进行set时，会自动重置
	/// <list type="bullet">
	/// <item><see cref="Root"/></item>
	/// <item><see cref="Depth"/></item>
	/// <item><see cref="ChildrenNode"/>的<see cref="Parent"/></item>
	/// </list>
	/// </remarks>
	public TNode? Parent
	{
		get; protected set
		{
			field = value;
			Root = null;
			Depth = -1;
			foreach (var item in ChildrenNode)
			{
				item.Parent = (TNode)this;
			}
		}
	}
}