using zms9110750Library.StateMachine.Abstract;
using zms9110750Library.StateMachine.Extension;
using zms9110750Library.StateMachine.Mode;

namespace zms9110750Library.StateMachine;
public class StateTransitionTable<TState, TArg> : ITransitionEvent<TArg> where TArg : notnull
{
	/// <summary>
	/// 静态转换表
	/// </summary>
	private readonly Dictionary<TArg, StaticTableVoucher> _staticTable = [];
	/// <summary>
	/// 动态转换表
	/// </summary>
	public event TryFetch<TArg, TState, TriggerMode>? DynamicTable;
	public event Func<TArg, ValueTask>? OnEntryFrom;
	public event Func<TArg, ValueTask>? OnExitFrom;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2119:密封满足私有接口的方法", Justification = "<挂起>")]
	public ValueTask TransitionEntryAsync(TArg arg)
	{
		return OnEntryFrom.WhenAll(arg);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2119:密封满足私有接口的方法", Justification = "<挂起>")]
	public ValueTask TransitionExitAsync(TArg arg)
	{
		return OnExitFrom.WhenAll(arg);
	}

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
			type = func.Invoke(arg, out state);
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
	/// 记录已注册参数的凭证
	/// </summary>
	/// <param name="stateTransitionTable">配置表</param>
	/// <param name="arg">参数</param>
	/// <param name="state">目标状态</param>
	/// <param name="mode">转换方式</param>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:嵌套类型应不可见", Justification = "<挂起>")]
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
	#endregion
}
