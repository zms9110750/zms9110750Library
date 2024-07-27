using System;

namespace zms9110750Library.StateMachine;

/// <summary>
/// 配置转换表
/// </summary>
/// <typeparam name="TState">配置类型</typeparam>
/// <typeparam name="TArg">参数类型</typeparam>
public class StateTransitionTable<TState, TArg> where TArg : notnull
{
	#region 字段
	/// <summary>
	/// 静态转换表
	/// </summary>
	private readonly Dictionary<TArg, StaticTableVoucher> _staticTable = [];
	/// <summary>
	/// 动态转换表
	/// </summary>
	public event Func<TArg, (TState, TriggerMode)>? DynamicTable;
	/// <summary>
	/// 进入事件
	/// </summary>
	public event Func<TArg, Task>? OnEntryFrom;
	/// <summary>
	/// 退出事件
	/// </summary>
	public event Func<TArg, Task>? OnExitFrom;
	#endregion
	#region 执行委托
	/// <summary>
	/// 触发进入事件
	/// </summary>
	/// <param name="arg">参数</param>
	/// <returns>等待事件处理完毕</returns>
	public Task EntryArg(TArg arg)
	{
        return OnEntryFrom.WhenAll(arg);
	}
	/// <summary>
	/// 触发退出事件
	/// </summary>
	/// <param name="arg">参数</param>
	/// <returns>等待参数处理完毕</returns>
	public Task ExitArg(TArg arg)
	{
		return OnExitFrom.WhenAll(arg);
	}
	#endregion
	#region 注册进入退出
	/// <summary>
	/// 注册转换事件
	/// </summary>
	/// <param name="func">执行委托</param>
	/// <param name="revokeType">时机</param>
	/// <returns>注销凭证</returns>
	public IDisposable Register(Func<TArg, Task> func, RegistrationTiming revokeType)
	{
		switch (revokeType)
		{
			case RegistrationTiming.Entry:
				OnEntryFrom += func;
				break;
			case RegistrationTiming.Exit:
				OnExitFrom += func;
				break;
		}
		return new RegisterVoucher(this, func, revokeType);
	}
	#endregion
	#region 注册转换表
	/// <summary>
	/// 注册静态转换表
	/// </summary>
	/// <param name="arg">参数</param>
	/// <param name="state">目标状态</param>
	/// <param name="type">转换方式</param>
	/// <returns>可以修改和释放这个参数的转换凭证</returns>
	/// <remarks>如果这个参数已经注册过，则返回null</remarks>
	public StaticTableVoucher? Register(TArg arg, TState state, TriggerMode type)
	{
		return _staticTable.ContainsKey(arg) ? null : (_staticTable[arg] = new StaticTableVoucher(this, arg, state, type));
	}

	/// <summary>
	/// 注册动态表
	/// </summary>
	/// <param name="func">注册的委托</param>
	/// <returns>释放这个接口以移除这个委托</returns>
	public IDisposable Register(Func<TArg, (TState, TriggerMode)> func)
	{
		DynamicTable += func;
		return new DynamicTableVoucher(this, func);
	}

	#endregion
	/// <summary>
	/// 计算这个参数的转换方式
	/// </summary>
	/// <param name="arg">参数</param>
	/// <param name="state">目标状态</param>
	/// <returns>转换方式</returns>
	public TriggerMode Consult(TArg arg, out TState state)
	{
		TriggerMode type = default;
		foreach (var func in DynamicTable.EnumInvocationList())
		{
			(state, type) = func.Invoke(arg);
			if (type.HasFlag(TriggerMode.Intercept))
			{
				return type;
			}
		}
		if (_staticTable.TryGetValue(arg, out var result))
		{
			(state, type) = result;
			return type;
		}
		state = default!;
		return default;
	}
	#region 内部类

	/// <summary>
	/// 移除动态表的凭证
	/// </summary>
	/// <param name="stateTransitionTable">配置表</param>
	/// <param name="func">应当移除的委托</param>
	private class DynamicTableVoucher(StateTransitionTable<TState, TArg> stateTransitionTable, Func<TArg, (TState, TriggerMode)> func) : IDisposable
	{
		public void Dispose() => stateTransitionTable.DynamicTable -= func;
	}

	/// <summary>
	/// 记录已注册参数的凭证
	/// </summary>
	/// <param name="stateTransitionTable">配置表</param>
	/// <param name="arg">参数</param>
	/// <param name="state">目标状态</param>
	/// <param name="mode">转换方式</param>
	public sealed class StaticTableVoucher(StateTransitionTable<TState, TArg> stateTransitionTable, TArg arg, TState state, TriggerMode mode) : IDisposable
	{
		private bool _disposed;
		public void Deconstruct(out TState state, out TriggerMode type)
		{
			state = State;
			type = Type;
		}
		/// <summary>
		/// 目标状态
		/// </summary>
		public TState State { get; set; } = state;
		/// <summary>
		/// 转换方式
		/// </summary>
		public TriggerMode Type { get; set; } = mode;
		/// <summary>
		/// 从静态表移除这个方案
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			if (stateTransitionTable._staticTable.TryGetValue(arg, out var revoke) && revoke == this)
			{
				stateTransitionTable._staticTable.Remove(arg);
			}
		}
	}

	/// <summary>
	/// 注册凭证
	/// </summary>
	/// <param name="configuration">转换表</param>
	/// <param name="func">解绑的委托</param>
	/// <param name="type">解绑类型</param>
	private class RegisterVoucher(StateTransitionTable<TState, TArg> configuration, Func<TArg, Task> func, RegistrationTiming type) : IDisposable
	{
		private bool _disposed  ;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			switch (type)
			{
				case RegistrationTiming.Entry:
					configuration.OnEntryFrom -= func;
					break;
				case RegistrationTiming.Exit:
					configuration.OnExitFrom -= func;
					break;
			}
		}
	}
	#endregion
}