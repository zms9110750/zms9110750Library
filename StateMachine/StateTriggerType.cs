namespace zms9110750Library.StateMachine;
public enum StateTriggerType
{ 
	/// <summary>
	/// 未注册，转换表没有登记这种转换
	/// </summary>
	Unregistered,
    /// <summary>
    /// 忽略，因为条件不满足而继续查询转换表
    /// </summary>
	Ignore,
	/// <summary>
	/// 转换，触发自己到目标间的退出和进入。自我转换无事发生。
	/// </summary>
	Transition,
    /// <summary>
    /// 激发，用于不转换状态时执行预定操作。例如从空闲，移动，奔跑激发转身。
    /// </summary>
    Excite,
    /// <summary>
    /// 拦截，在转换表中查询到此状态时不应该继续查询。
    /// </summary>
    Intercept,
    /// <summary>
    /// 无过程，转换状态但不触发进入退出事件。
    /// </summary>
    NoProcess
}