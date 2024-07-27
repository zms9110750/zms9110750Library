namespace zms9110750Library.StateMachine;

public static class StateConfigurationExtension
{
	#region 拆分委托
	public static IEnumerable<T> EnumInvocationList<T>(this T? func) where T : Delegate
	{
		return func?.GetInvocationList().OfType<T>() ?? [];
	}
	#endregion
	#region 事件同时启动
	public static Task WhenAll<TArg>(this Func<TArg, Task>? fun, TArg arg)
	{
		return fun == null
			   ? Task.CompletedTask
			   : Task.WhenAll(fun.EnumInvocationList().Select(func => func.Invoke(arg)));
	}
	public static Task WhenAll(this Func<Task>? fun)
	{
		return fun == null
			   ? Task.CompletedTask
			   : Task.WhenAll(fun.EnumInvocationList().Select(func => func.Invoke()));
	}
	#endregion
	#region StateConfiguration注册和撤销
	public static StateTransitionTable<TState, TArg>.StaticTableVoucher? Register<TState, TArg>(this StateConfiguration<TState> configuration, TArg arg, TState state, TriggerMode type) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration);
		return configuration.Table<TArg>().Register(arg, state, type);
	}
	public static IDisposable Register<TState, TArg>(this StateConfiguration<TState> configuration, Func<TArg, (TState, TriggerMode)> func) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration);
		return configuration.Table<TArg>().Register(func);
	}
	#endregion
	#region StateMachine注册和撤销

	public static StateTransitionTable<TState, TArg>.StaticTableVoucher? Register<TState, TArg>(this StateMachine<TState> machine, TState state, TState response, TArg arg, TriggerMode type) where TArg : notnull where TState : notnull
	{
		ArgumentNullException.ThrowIfNull(machine);
		return machine[state].Register(arg, response, type);
	}
	public static IDisposable Register<TState, TArg>(this StateMachine<TState> machine, TState state, Func<TArg, (TState, TriggerMode)> func) where TArg : notnull where TState : notnull
	{
		ArgumentNullException.ThrowIfNull(machine);
		return machine[state].Register(func);
	}
	#endregion
}
