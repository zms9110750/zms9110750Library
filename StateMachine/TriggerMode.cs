namespace zms9110750Library.StateMachine;
[Flags]
public enum TriggerMode
{
	#region Flage
	/// <summary>
	/// 什么也不做
	/// </summary>
	None = 0,
	/// <summary>
	/// 查询到此状态时不应该继续查询。
	/// </summary>
	Intercept = 1,
	/// <summary>
	/// 切换状态标识
	/// </summary>
	SwitchStateFlag = 2,
	/// <summary>
	/// 退出标识
	/// </summary>
	TriggerExitFlag = 4,
	/// <summary>
	/// 进入标识
	/// </summary>
	TriggerEntryFlag = 8,
	#endregion
	/// <summary>
	/// 仅切换到目标状态
	/// </summary>
	SwitchState = Intercept | SwitchStateFlag,
	/// <summary>
	/// 仅触发退出
	/// </summary>
	TriggerExit = Intercept | TriggerExitFlag,
	/// <summary>
	/// 仅触发进入
	/// </summary>
	TriggerEntry = Intercept | TriggerEntryFlag,
	/// <summary>
	/// 转换，触发自己到目标间的退出和进入。自我转换无事发生。
	/// </summary>
	Transition = Intercept | SwitchState | TriggerEntry | TriggerExit
}