namespace zms9110750.TreeCollection.Abstract;
/// <summary>
/// 提供<see cref="INode{TNode}"/>的扩展方法。并为实现类提供对接口默认实现的调用。
/// </summary>
public static class NodeExtensions
{
	/// <inheritdoc cref="INode{T}.Parent" path="/summary"/>
	/// <typeparam name="T">节点类型</typeparam>
	/// <param name="node">扩展方法依附的实例</param>
	/// <remarks>当接口具有默认实现时，此方法以扩展方法提供成员调用</remarks>
	/// <seealso cref="INode{T}.Parent"/>
	public static T? GetParent<T>(this T node) where T : INode<T>
	{
		return node.Parent;
	}


	/// <remarks>父节点为null时，抛出异常</remarks>
	/// <exception cref="InvalidOperationException"></exception>
	/// <inheritdoc cref="GetParent{T}(T)"/> 
	public static T GetRequiredParent<T>(this T node) where T : INode<T>
	{
		return node.Parent ?? throw new InvalidOperationException("node has no parent");
	}
	/// <inheritdoc cref="INode{T}.Root" path="/summary"/>
	/// <inheritdoc cref="GetParent{T}(T)"/> 
	public static T? GetRoot<T>(this T node) where T : INode<T>
	{
		return node.Root;
	}

	/// <inheritdoc cref="INode{T}.Depth" path="/summary"/>
	/// <inheritdoc cref="GetParent{T}(T)"/> 
	public static int GetDepth<T>(this T node) where T : INode<T>
	{
		return node.Depth;
	}

	/// <summary>
	/// 查找两个节点的最近公共祖先节点
	/// </summary>
	/// <param name="node"><inheritdoc cref="GetParent{T}(T)" path="/param[@name='node']"/></param>
	/// <param name="node2">进行比较的另一个节点</param>
	/// <returns>公共祖先节点</returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>没有公共节点或任意一方为null时，返回null。<br/>两个节点有父子关系时，返回父节点</remarks>
	/// <inheritdoc cref="GetParent{T}(T)"/> 
	public static T? CommonParent<T>(this T node, T node2) where T : class, INode<T>
	{

		if (node == null || node2 == null || node.Root != node2.Root)
			return null;

		var left = node;
		var right = node2;
		while (left.Depth > right.Depth)
		{
			left = left.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + left);
		}

		while (right.Depth > left.Depth)
		{
			right = right.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + right);
		}

		while (left != null && right != null)
		{
			if (ReferenceEquals(left, right))
				return left;

			left = left.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + left);
			right = right.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + right);
		}

		throw new InvalidOperationException("Unexpected tree implementation: Failed to find common parent");
	}

	/// <summary>
	/// 迭代到根节点
	/// </summary>
	/// <remarks>包含自身</remarks>
	/// <inheritdoc cref="GetParent{T}(T)"/> 
	public static IEnumerable<TNode> IterateToRoot<TNode>(this TNode node)
		where TNode : class, INode<TNode>
	{
		if (node == null)
			yield break;

		var current = node;
		int? lastDepth = null;

		while (current != null)
		{
			yield return current;

			// 验证深度严格递减
			if (lastDepth.HasValue && current.Depth >= lastDepth)
			{
				throw new InvalidOperationException(
					$"Invalid depth sequence at node {current}: {current.Depth} >= {lastDepth}");
			}

			lastDepth = current.Depth;
			current = current.Parent;
		}
	}
}
