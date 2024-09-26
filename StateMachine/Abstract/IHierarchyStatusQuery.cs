namespace zms9110750Library.StateMachine.Abstract;


/// <summary>
/// IHierarchyStatusQuery 接口用于查询层级状态信息。
/// </summary>
/// <typeparam name="TState">状态的类型。</typeparam>
public interface IHierarchyStatusQuery<TState>
{
	/// <summary>
	/// 获取从左侧节点到根节点的路径。
	/// </summary>
	/// <param name="left">左侧节点。</param>
	/// <param name="right">右侧节点。</param>
	/// <returns>从左侧节点到根节点的路径集合。</returns>
	IEnumerable<TState> GetLeftToRootPath(TState left, TState right);

	/// <summary>
	/// 获取从根节点到右侧节点的路径。
	/// </summary>
	/// <param name="left">左侧节点。</param>
	/// <param name="right">右侧节点。</param>
	/// <returns>从根节点到右侧节点的路径集合。</returns>
	IEnumerable<TState> GetRootToRightPath(TState left, TState right); 

	/// <summary>
	/// 获取左侧节点和右侧节点的公共祖先。
	/// </summary>
	/// <param name="left">左侧节点。</param>
	/// <param name="right">右侧节点。</param>
	/// <param name="ancestor">公共祖先。</param>
	/// <returns>是否找到公共祖先。</returns>
	bool TryGetCommonAncestor(TState left, TState right, out TState ancestor);
}