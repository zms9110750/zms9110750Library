namespace zms9110750Library.StateMachine.Mode;
#pragma warning disable RCS1157 // Composite enum value contains undefined flag
/// <summary>
/// 定义状态机在查询时的行为模式，控制查询流程和状态转换行为。
/// 可以使用位标志组合多个行为。
/// </summary>
[Flags]
public enum QueryBehavior
{
	/// <summary>
	/// 无特殊行为
	/// </summary>
	None = 0,

	/// <summary>
	/// 终止查询，不再继续后续处理
	/// </summary>
	Intercept = 1 << 0,

	/// <summary>
	/// 执行状态切换操作
	/// </summary>
	StateSwitch = 1 << 1 | Intercept,

	/// <summary>
	/// 触发离开当前状态的事件
	/// </summary>
	OnExit = 1 << 2 | Intercept,

	/// <summary>
	/// 触发进入新状态的事件  
	/// </summary>
	OnEntry = 1 << 3 | Intercept,

	/// <summary>
	/// 执行完整的状态切换流程，包括状态切换、触发离开当前状态和进入新状态的事件  
	/// </summary>
	Transition = StateSwitch | OnExit | OnEntry | Intercept,

	/// <summary>
	/// 如果当前查询失败，将委托给参数类型的基类型继续查询
	/// </summary>
	DelegateToBaseType = 1 << 8,

	/// <summary>
	/// 如果当前查询失败，将委托给状态的父节点继续查询
	/// </summary>
	DelegateToParentState = 1 << 9,

	/// <summary>
	/// 如果当前查询失败，先委托给参数类型的基类型继续查询，如果仍然失败，再委托给状态的父节点继续查询
	/// </summary>
	DelegateToHierarchy = DelegateToBaseType | DelegateToParentState
}
