namespace zms9110750Library.StateMachine;

/// <summary>
/// 状态机转换事件
/// </summary>
/// <typeparam name="TState">状态类型</typeparam>
/// <param name="Launch">起始状态</param>
/// <param name="Response">相应状态</param>
/// <param name="Mode">转换方式</param>
/// <param name="ParameType">参数类型</param>
/// <param name="Parameter">参数</param>
/// <remarks>若无参数，则<see cref="ParameType"/>为<see cref="null"/></remarks>
public readonly record struct Transition<TState>(TState Launch, TState Response, TriggerMode Mode, Type? ParameType, object? Parameter);
