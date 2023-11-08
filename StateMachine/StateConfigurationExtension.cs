using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zms9110750Library.StateMachine;
public static class StateConfigurationExtension
{  
	#region StateConfiguration注册和撤销
	public static void Register<TState, TArg>(this StateConfiguration<TState> configuration, TArg arg, TState state, StateTriggerType type) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration);
		configuration.Table<TArg>().Register(arg, state, type);
	}
	public static void Revoke<TState, TArg>(this StateConfiguration<TState> configuration, TArg arg) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration);
		configuration.Table<TArg>().Revoke(arg);
	}
	public static LinkedListNode<Func<TArg, (TState, StateTriggerType)>> Register<TState, TArg>(this StateConfiguration<TState> configuration, Func<TArg, (TState, StateTriggerType)> func) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration);
		return configuration.Table<TArg>().Register(func);
	}
	public static void Revoke<TState, TArg>(this StateConfiguration<TState> configuration, LinkedListNode<Func<TArg, (TState, StateTriggerType)>> node) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration);
		configuration.Table<TArg>().Revoke(node);
	}
	#endregion
	#region StateMachine注册和撤销

	public static void Register<TState, TArg>(this StateMachine<TState> machine, TState state, TState response, TArg arg, StateTriggerType type) where TArg : notnull where TState : notnull
	{
		ArgumentNullException.ThrowIfNull(machine);
		machine[state].Register(arg, response, type);
	}
	public static void Revoke<TState, TArg>(this StateMachine<TState> machine, TState state, TArg arg) where TArg : notnull where TState : notnull
	{
		ArgumentNullException.ThrowIfNull(machine);
		machine[state].Revoke(arg);
	}
	public static LinkedListNode<Func<TArg, (TState, StateTriggerType)>> Register<TState, TArg>(this StateMachine<TState> machine, TState state, Func<TArg, (TState, StateTriggerType)> func) where TArg : notnull where TState : notnull
	{
		ArgumentNullException.ThrowIfNull(machine);
		return machine[state].Register(func);
	}
	public static void Revoke<TState, TArg>(this StateMachine<TState> machine, TState state, LinkedListNode<Func<TArg, (TState, StateTriggerType)>> node) where TArg : notnull where TState : notnull
	{
		ArgumentNullException.ThrowIfNull(machine);
		machine[state].Revoke(node);
	}
	#endregion
}
