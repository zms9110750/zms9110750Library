namespace zms9110750Library.StateMachine.Mode;

[Flags]
public enum TriggerMode
{
	/// <summary>  
	/// 无特殊行为  
	/// </summary>  
	None = 0,

	/// <summary>  
	/// 阻止进一步查询  
	/// </summary>  
	Intercept = 1,

	/// <summary>  
	/// 指示状态切换  
	/// </summary>  
	StateSwitchFlag = 2,

	/// <summary>  
	/// 触发离开当前状态的标志  
	/// </summary>  
	OnExitFlag = 4,

	/// <summary>  
	/// 触发进入新状态的标志  
	/// </summary>  
	OnEntryFlag = 8,

	/// <summary>  
	/// 仅执行状态切换  
	/// </summary>  
	StateSwitch = Intercept | StateSwitchFlag,

	/// <summary>  
	/// 仅在离开当前状态时触发  
	/// </summary>  
	OnExit = Intercept | OnExitFlag,

	/// <summary>  
	/// 仅在进入新状态时触发  
	/// </summary>  
	OnEntry = Intercept | OnEntryFlag,

	/// <summary>  
	/// 执行状态切换，并在离开和进入时触发  
	/// </summary>  
	Transition = Intercept | StateSwitch | OnExitFlag | OnEntryFlag
}
