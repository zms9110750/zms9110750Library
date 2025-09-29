using zms9110750.InterfaceImplAsExtensionGenerator.Config;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace zms9110750.TreeCollection.Abstract;

/// <summary>
/// 为接口提供扩展方法。以便调用默认实现
/// </summary>
[ExtendWithInterfaceImpl(typeof(INode<>))]
public static partial class NodeExtension
{
	static InvalidOperationException NoParentException => field ??= new InvalidOperationException("node has no parent");

	extension<TNode>(INode<TNode> node) where TNode : INode<TNode>
	{
		/// <summary>
		/// 获取父节点，如果没有父节点，则抛出异常
		/// </summary>
		public TNode RequiredParent
		{
			get => node.Parent ?? throw NoParentException;
		}
	}

	extension<TNode>(TNode node) where TNode : class, INode<TNode>
	{
		/// <summary>
		/// 查找两个节点的最近公共祖先节点
		/// </summary>
		/// <param name="node2">进行比较的另一个节点</param>
		/// <returns>没有公共节点或任意一方为null时，返回null。<br/>两个节点有父子关系时，返回父节点</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public TNode? CommonParent(TNode node2)
		{
			if (node == null || node2 == null || node.Root != node2.Root)
			{
				return null;
			}

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
				{
					return left;
				}

				left = left.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + left);
				right = right.Parent ?? throw new InvalidOperationException("Unexpected tree implementation: Broken parent chain at node " + right);
			}

			throw new InvalidOperationException("Unexpected tree implementation: Failed to find common parent");
		}

		/// <summary>
		/// 迭代到根节点
		/// </summary>
		/// <remarks>包含自身</remarks>
		public IEnumerable<TNode> IterateToRoot()
		{
			if (node == null)
			{
				yield break;
			}

			var current = node;
			int? lastDepth = null;

			while (current != null)
			{
				yield return current;

				// 验证深度严格递减
				if (current.Depth >= lastDepth)
				{
					throw new InvalidOperationException($"Invalid depth sequence at node {current}: {current.Depth} >= {lastDepth}");
				}

				lastDepth = current.Depth;
				current = current.Parent;
			}
		}
	}

}
