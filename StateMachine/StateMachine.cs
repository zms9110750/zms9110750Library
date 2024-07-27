using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using zms9110750Library.TreeNode;

namespace zms9110750Library.StateMachine;

public sealed class StateMachine<TState>(TState state) : IAsyncDisposable, IAsyncEnumerable<Transition<TState>> where TState : notnull
{
	#region 字段
	readonly AsyncSemaphoreWrapper _lock = new AsyncSemaphoreWrapper();
	readonly ConcurrentDictionary<TState, StateConfiguration<TState>> _configuration = new ConcurrentDictionary<TState, StateConfiguration<TState>>();
	readonly ConcurrentDictionary<TState, TreeNode<TState>> _tree = new ConcurrentDictionary<TState, TreeNode<TState>>();
	readonly HashSet<Queue<Transition<TState>>> _notice = [];
	#endregion

	#region 获取配置 
	/// <summary>
	/// 当前状态
	/// </summary>
	/// <remarks>切换状态必须使用<see cref="Transition(TState, TriggerMode)"/>方法，以等待未完成的转换。</remarks>
	public TState State { get; private set; } = state;


	/// <summary>
	/// 当前状态的配置
	/// </summary>
	public StateConfiguration<TState> CurrentConfiguration => this[State];

	/// <summary>
	/// 获取该状态下的配置
	/// </summary>
	/// <param name="state">状态</param>
	/// <returns>状态配置</returns>
	public StateConfiguration<TState> this[TState state] => _configuration.GetOrAdd(state, _ => new StateConfiguration<TState>());

	/// <summary>
	/// 获取目标状态下指定参数类型的转换表
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="state">目标状态</param>
	/// <returns>转换表</returns>
	public StateTransitionTable<TState, TArg> Table<TArg>(TState state) where TArg : notnull
	{
		return this[state].Table<TArg>();
	}
	#endregion

	#region 查看和设置层级状态
	/// <summary>
	/// 是否处于某个状态中
	/// </summary>
	/// <param name="state">检查的状态</param>
	/// <returns>处于参数的状态里</returns>
	public bool IsInState(TState state)
	{
		return _tree.TryGetValue(state, out var target)
			&& _tree.TryGetValue(State, out var current)
			&& (current & target) == target;
	}

	/// <summary>
	/// 设置状态的子状态
	/// </summary>
	/// <param name="substate">作为超类的状态</param>
	/// <param name="child">子状态</param>
	/// <remarks>把自己作为自己的子状态，改为把自己从超类中独立出来</remarks>
	public void SetChildState(TState substate, params ReadOnlySpan<TState> child)
	{
		var target = _tree.GetOrAdd(substate, key => new TreeNode<TState>(key));
		foreach (var item in child)
		{
			_tree.GetOrAdd(item, key => new TreeNode<TState>(key)).Parent = item.Equals(substate) ? null : target;
		}
	}

	#endregion

	#region 转换    

	/// <summary>
	/// 设置状态
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="state">目标状态</param>
	/// <param name="mode">转换方式</param>
	/// <param name="arg">参数</param>
	/// <param name="hasArg">是否调用带参数的触发事件</param>
	/// <returns>等待转换完成的任务</returns>
	private async Task Transition<TArg>(TState state, TriggerMode mode, TArg arg = default!, bool hasArg = false) where TArg : notnull
	{
		var old = State;
		var target = _tree!.GetValueOrDefault(state, null);
		var current = _tree!.GetValueOrDefault(State, null);
		var ancestor = target & current;
		if (mode.HasFlag(TriggerMode.SwitchState))
		{
			State = state;
		}
		if (mode.HasFlag(TriggerMode.TriggerExit))
		{
			if (hasArg)
			{
				foreach (var item in (current | ancestor).Select(n => n.Value!).DefaultIfEmpty(State))
				{
					await this[item].Exit(arg);
				}
			}
			else
			{
				foreach (var item in (current | ancestor).Select(n => n.Value!).DefaultIfEmpty(State))
				{
					await this[item].Exit();
				}
			}
		}
		if (mode.HasFlag(TriggerMode.TriggerEntry))
		{
			if (hasArg)
			{
				foreach (var item in (ancestor | target).Select(n => n.Value!).DefaultIfEmpty(state))
				{
					await this[item].Entry(arg);
				}
			}
			else
			{
				foreach (var item in (ancestor | target).Select(n => n.Value!).DefaultIfEmpty(state))
				{
					await this[item].Entry();
				}
			}
		}
		foreach (var item in _notice)
		{
			item.Enqueue(new Transition<TState>(old, state, mode, hasArg ? typeof(TArg) : null, arg));
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
		using var scope = await _lock.EnterScopeAsync();
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
		using var scope = await _lock.EnterScopeAsync();
		await Transition(state, mode, arg, true);
	}

	/// <summary>
	/// 根据参数计算转换目标和方式
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>等待转换完成的任务</returns>
	public async Task Transition<TArg>(TArg arg) where TArg : notnull
	{
		using var scope = await _lock.EnterScopeAsync();
		TState state;
		TriggerMode mode;
		if (_tree.TryGetValue(State, out var node))
		{
			do
			{
				mode = this[node.Value!].Consult(arg, out state);
				node = node.Parent;
			} while (node != null && !mode.HasFlag(TriggerMode.Intercept));
		}
		else
		{
			mode = CurrentConfiguration.Consult(arg, out state);
		}
		await Transition(state, mode, arg, true);
	}
	#endregion

	#region 接口     
	public ValueTask DisposeAsync() => _lock.DisposeAsync();
	public async IAsyncEnumerator<Transition<TState>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		if (_lock.IsDisposed)
		{
			yield break;
		}
		Queue<Transition<TState>> queue = new Queue<Transition<TState>>();
		_notice.Add(queue);
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				if (queue.TryDequeue(out var result))
				{
					yield return result;
				}
				else if (!_lock.IsDisposed)
				{ 
                    await _lock.ExitScopeAsync(cancellationToken);
				}
				else
				{
					break;
				}
			}
			cancellationToken.ThrowIfCancellationRequested();
		}
		finally
		{
			_notice.Remove(queue);
			queue.Clear();
		}
	}
	#endregion
}