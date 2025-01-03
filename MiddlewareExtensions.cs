namespace zms9110750Library;

public static class MiddlewareExtensions
{
	public static void Run<TContext>(this Action<TContext, Action> action, TContext context)
	{
		ArgumentNullException.ThrowIfNull(action);
		List<Action<TContext, Action>> delegates = [.. Delegate.EnumerateInvocationList(action)];

		static void ExecuteMiddleware(int index, TContext context, List<Action<TContext, Action>> delegates)
		{
			if (index < delegates.Count)
			{
				delegates[index](context, () => ExecuteMiddleware(index + 1, context, delegates));
			}
		} 
		// 开始执行第一个委托
		ExecuteMiddleware(0, context, delegates);
	}
}