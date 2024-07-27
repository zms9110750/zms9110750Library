namespace zms9110750Library.StateMachine;
[Flags]
public enum TriggerMode
{
	#region Flage
	/// <summary>
	/// 未注册
	/// </summary>
	Unregistered = 0,
	/// <summary>
	/// 查询到此状态时不应该继续查询。
	/// </summary>
	Intercept = 1,
	/// <summary>
	/// 切换到目标状态
	/// </summary>
	SwitchState = 1 | 2,
	/// <summary>
	/// 触发退出
	/// </summary>
	TriggerExit = 1 | 4,
	/// <summary>
	/// 触发进入
	/// </summary>
	TriggerEntry = 1 | 8,
	#endregion
	/// <summary>
	/// 转换，触发自己到目标间的退出和进入。自我转换无事发生。
	/// </summary>
	Transition = 1 | 2 | 4 | 8
}