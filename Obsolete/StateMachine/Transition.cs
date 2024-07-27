namespace zms9110750Library.Obsolete.StateMachine;

public readonly record struct Transition<TState>(TState Launch, TState Response, StateTriggerType Type, object? Parameter);
