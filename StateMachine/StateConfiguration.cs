using System.Collections.Concurrent;

namespace zms9110750Library.StateMachine;
/// <summary>
/// 状态配置表
/// </summary>
/// <typeparam name="TState">状态类型</typeparam>
public class StateConfiguration<TState>
{
	/// <summary>
	/// 进入事件
	/// </summary>
	public event Func<Task>? OnEntry;

	/// <summary>
	/// 退出事件
	/// </summary>
	public event Func<Task>? OnExit;

	/// <summary>
	/// 转换表
	/// </summary>
	readonly ConcurrentDictionary<Type, object> _transitionTable = new ConcurrentDictionary<Type, object>();

	#region 注册
	/// <summary>
	/// 注册转换事件
	/// </summary>
	/// <param name="func">事件</param>
	/// <param name="revokeType">时机</param>
	/// <returns>注销凭证</returns>
	public IDisposable Register(Func<Task> func, RegistrationTiming revokeType)
	{
		switch (revokeType)
		{
			case RegistrationTiming.Entry:
				OnEntry += func;
				break;
			case RegistrationTiming.Exit:
				OnExit += func;
				break;
		}
		return new RegisterVoucher(this, func, revokeType);
	}
	#endregion
	
	#region 进入
	/// <summary>
	/// 进入事件
	/// </summary>
	/// <returns>事件执行完毕</returns>
	public Task Entry()
	{
		return OnEntry.WhenAll();
	}

	/// <summary>
	/// 进入事件
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>事件执行完毕</returns>
	public async Task Entry<TArg>(TArg arg) where TArg : notnull
	{ 
		await Entry();
		await Table<TArg>().EntryArg(arg);
	}
	#endregion

	#region 退出
	/// <summary>
	/// 退出事件
	/// </summary>
	/// <returns>事件执行完毕</returns>
	public Task Exit()
	{
		return OnExit.WhenAll();
	}

	/// <summary>
	/// 退出事件
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>时间执行完毕</returns>
	public async Task Exit<TArg>(TArg arg) where TArg : notnull
	{
		await Table<TArg>().ExitArg(arg);
		await Exit();
	}
	#endregion

	#region 转换表
	/// <summary>
	/// 获取指定参数类型的转换表
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <returns>转换表</returns>
	public StateTransitionTable<TState, TArg> Table<TArg>() where TArg : notnull
	{
		return (_transitionTable.GetOrAdd(typeof(TArg), static _ => new StateTransitionTable<TState, TArg>()) as StateTransitionTable<TState, TArg>)!;
	}
	/// <summary>
	/// 查验当前参数如何转换
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <param name="state">目标状态</param>
	/// <returns>转换方式</returns>
	/// 
	public TriggerMode Consult<TArg>(TArg arg, out TState state) where TArg : notnull
	{
		return Table<TArg>().Consult(arg, out state);
	}

	#endregion
	/// <summary>
	/// 注册凭证
	/// </summary>
	/// <param name="configuration">解绑的配置</param>
	/// <param name="func">解绑的委托</param>
	/// <param name="type">解绑类型</param>
	private class RegisterVoucher(StateConfiguration<TState> configuration, Func<Task> func, RegistrationTiming type) : IDisposable
	{
		private bool _disposed ;

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
					configuration.OnEntry -= func;
					break;
				case RegistrationTiming.Exit:
					configuration.OnExit -= func;
					break;
			}
		}
	}
}
