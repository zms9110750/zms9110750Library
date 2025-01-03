using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Nito.AsyncEx;
using zms9110750Library.StateMachine.Mode;

namespace zms9110750Library.StateMachine;
/// <summary>
/// 状态机
/// </summary>
/// <typeparam name="TState">状态类型</typeparam>
public abstract class StateMachine<TState>(TState state) : IAsyncDisposable where TState : notnull
{

	#region 字段 
	volatile int _disposed;
	readonly AsyncLock _lock = new AsyncLock();
	readonly ConcurrentDictionary<TState, StateConfiguration<TState>> _configuration = new ConcurrentDictionary<TState, StateConfiguration<TState>>();
	ServiceProvider _serviceProvider = InitService();

	#endregion
	#region 属性
	/// <summary>
	/// 是否已释放
	/// </summary>
	public bool IsDisposed => _disposed != 0;

	/// <summary>
	/// 当前状态
	/// </summary>
	/// <remarks>切换状态必须使用<see cref="Transition(TState, TriggerMode)"/>方法，以等待未完成的转换。</remarks>
	public TState State { get; protected set; } = state;

	/// <summary>
	/// 当前状态的 <seealso href="StateConfiguration"/> 
	/// </summary>
	public StateConfiguration<TState> CurrentConfiguration => this[State];

	/// <summary>
	/// 获取该状态下的 <seealso href="StateConfiguration"/>
	/// </summary>
	/// <param name="state">状态</param>
	/// <returns>状态配置</returns>
	public StateConfiguration<TState> this[TState state] => _configuration.GetOrAdd(state, CreateNewStateConfiguration);
	#endregion

	#region 方法
	/// <summary>
	/// 初始化服务
	/// </summary>	
	/// <returns>服务提供者</returns> 
	private static ServiceProvider InitService()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped(typeof(StateTransitionTable<,>));
		serviceCollection.AddTransient(sp => sp.CreateScope());
		serviceCollection.AddTransient<StateConfiguration<TState>>();
		return serviceCollection.BuildServiceProvider();
	}

	private StateConfiguration<TState> CreateNewStateConfiguration(TState arg) => _serviceProvider.GetRequiredService<StateConfiguration<TState>>();

	/// <summary>
	/// 转换状态
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="state">目标状态</param>
	/// <param name="mode">转换方式</param>
	/// <param name="arg">参数</param>
	/// <param name="useArg">是否使用参数</param>
	/// <returns>等待转换完成的任务</returns>
	protected virtual async ValueTask Transition<TArg>(TState state, TriggerMode mode, TArg arg = default!, bool useArg = false) where TArg : notnull
	{
		ObjectDisposedException.ThrowIf(IsDisposed, this);
		if (!mode.HasFlag(TriggerMode.Intercept))
		{
			return;
		}
		if (mode.HasFlag(TriggerMode.StateSwitch))
		{
			State = state;
		}
		if (mode.HasFlag(TriggerMode.OnExit))
		{
			await (useArg ? CurrentConfiguration.TransitionExitAsync(arg) : CurrentConfiguration.TransitionExitAsync());
		}
		if (mode.HasFlag(TriggerMode.OnEntry))
		{
			await (useArg ? this[state].TransitionEntryAsync(arg) : this[state].TransitionEntryAsync());
		}
	}

	/// <summary>
	/// 无参数转换
	/// </summary>
	/// <param name="state">目标状态</param>
	/// <param name="mode">转换方式</param>
	/// <returns>等待转换完成的任务</returns>
	public async Task Transition(TState state, TriggerMode mode = TriggerMode.Transition)
	{
		using var scope = await _lock.LockAsync();
		await Transition<object>(state, mode);
	}

	/// <summary>
	/// 有参数转换
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="state">目标状态</param>
	/// <param name="arg">参数</param>
	/// <param name="mode">转换方式</param>
	/// <returns>等待转换完成的任务</returns>
	public async Task Transition<TArg>(TState state, TArg arg, TriggerMode mode = TriggerMode.Transition) where TArg : notnull
	{
		using var scope = await _lock.LockAsync();
		await Transition(state, mode, arg, true);
	}

	/// <summary>
	/// 根据参数计算转换目标和方式
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>等待转换完成的任务</returns>
	public async ValueTask Transition<TArg>(TArg arg) where TArg : notnull
	{
		using var scope = await _lock.LockAsync();
		TriggerMode mode = CurrentConfiguration.Consult(arg, out TState state);
		await Transition(state, mode, arg, true);
	}


	/// <summary>
	/// 异步释放
	/// </summary>
	/// <returns>等待释放完成的任务</returns>
	public async ValueTask DisposeAsync()
	{
		using var scope = await _lock.LockAsync();
		_serviceProvider.Dispose();
		GC.SuppressFinalize(this);
	}
	#endregion
}

