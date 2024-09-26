using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using zms9110750Library.NodeTree;
using zms9110750Library.StateMachine.Abstract;
namespace zms9110750Library.StateMachine;


public abstract class HierarchyStatusQuery<TState> : IHierarchyStatusQuery<TState> where TState : notnull
{
	private TreeNode<TState> this[TState state] => _tree.GetOrAdd(state, key => new TreeNode<TState>(key));
	private readonly ConcurrentDictionary<TState, TreeNode<TState>> _tree = new ConcurrentDictionary<TState, TreeNode<TState>>();

	/// <summary>
	/// 设置状态的子状态
	/// </summary>
	/// <param name="substate">作为超类的状态</param>
	/// <param name="child">子状态</param>
	/// <remarks>如果没有任何子状态，改为把自己独立出来</remarks>
	public void SetChildState(TState substate, params ReadOnlySpan<TState> child)
	{
		if (child.Length == 0)
		{
			this[substate].Parent = null;
		}
		var target = this[substate];
		foreach (var item in child)
		{
			this[item].Parent = target;
		}
	}
	public IEnumerable<TState> GetLeftToRootPath(TState left, TState right)
	{
		var ancestor = this[left] & this[right];
		return (this[left] | ancestor).Select(x => x.Value!)?.DefaultIfEmpty(left) ?? [left];

	}
	public IEnumerable<TState> GetRootToRightPath(TState left, TState right)
	{
		var ancestor = this[left] & this[right];
		return (ancestor | this[right]).Select(x => x.Value!)?.DefaultIfEmpty(right) ?? [right];
	}

	public bool TryGetCommonAncestor(TState left, TState right, [MaybeNullWhen(false)] out TState ancestor)
	{
		var ancestorNode = (this[left] & this[right]);
		if (ancestorNode != null)
		{
			ancestor = ancestorNode.Value!;
			return true;
		}
		else
		{
			ancestor = default; // 或者提供适当的默认值
			return false;
		}

	}
}
