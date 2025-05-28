namespace zms9110750.TreeCollection.Abstract;
public static class TreeNodeExtensions
{
	public static T? GetParent<T>(this T node) where T : INode<T>
	{
		return node.Parent;
	}
	public static T GetRequiredParent<T>(this T node) where T : INode<T>
	{
		return node.Parent ?? throw new InvalidOperationException("node has no parent");
	}
	public static T? GetRoot<T>(this T node) where T : INode<T>
	{
		return node.Root;
	}
	public static int GetDepth<T>(this T node) where T : INode<T>
	{
		return node.Depth;
	}

	/// <summary>
	/// 查找两个节点的最近公共父节点
	/// </summary>
	public static T? CommonParent<T>(this T node1, T node2) where T : class, INode<T>
	{

		// 1. Null check
		if (node1 == null || node2 == null || node1.Root != node2.Root)
			return null;

		// 3. Depth balancing
		var left = node1;
		var right = node2;
		while (left.Depth > right.Depth)
		{
			left = left.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + left);
		}

		while (right.Depth > left.Depth)
		{
			right = right.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + right);
		}

		// 4. Synchronous traversal
		while (left != null && right != null)
		{
			if (ReferenceEquals(left, right))
				return left;

			left = left.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + left);
			right = right.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + right);
		}

		// This should never be reached if tree is properly implemented
		throw new InvalidOperationException("Unexpected tree implementation: Failed to find common parent");
	}
	 
	/// <summary>
	/// 迭代到根节点并验证深度递减（适用于IRootNode）
	/// </summary>
	public static IEnumerable<TNode> IterateToRoot<TNode>(this TNode node)
		where TNode : class,   INode<TNode>
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
