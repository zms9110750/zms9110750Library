namespace zms9110750Library.StateMachine.Mode;
public delegate TReturn TryFetch<TInput, TResult, TReturn>(TInput input, out TResult result);