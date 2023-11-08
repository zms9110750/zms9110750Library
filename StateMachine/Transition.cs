namespace zms9110750Library.StateMachine;
 
public readonly record struct Transition<TState>(TState Launch, TState Response, StateTriggerType Type, object? Parameter);
 