namespace zms9110750Library.Complete;
#if NET8_0_OR_GREATER 
public readonly record struct Transition<TState>(TState Launch, TState Response, StateTriggerType Type, object? Parameter);
#else
public readonly struct Transition<TState> : IEquatable<Transition<TState>>
{
	#region 字段 
	public TState Launch { get; }
	public TState Response { get; }
	public StateTriggerType Type { get; }
	public object? Parameter { get; }
	public Transition(TState launch, TState response, StateTriggerType type, object? parameter)
	{
		Launch = launch;
		Response = response;
		Type = type;
		Parameter = parameter;
	}
	#endregion
	#region 重写匹配  
	public override bool Equals(object? obj) => obj is Transition<TState> transition && Equals(transition);
	public bool Equals(Transition<TState> other) => EqualityComparer<TState>.Default.Equals(Launch, other.Launch) && EqualityComparer<TState>.Default.Equals(Response, other.Response) && Type == other.Type && EqualityComparer<object>.Default.Equals(Parameter, other.Parameter);
	public override int GetHashCode() => HashCode.Combine(Launch, Response, Type, Parameter);

	public static bool operator ==(Transition<TState> left, Transition<TState> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Transition<TState> left, Transition<TState> right)
	{
		return !(left == right);
	}
	#endregion
}
#endif
