using zms9110750.TreeCollection.Ordered;

namespace zms9110750Library.StateMachine.Mode;
public class Argument<TState>;
public class Argument<TState, TArg> : Argument<TState>
{
	public Dictionary<TArg, (QueryBehavior, TState)> StaticState { get; set; }
	public event Func<TArg, (QueryBehavior, TState)> DynamicState;

	public (QueryBehavior, TState) 计算(TArg arg)
	{
		(QueryBehavior, TState) result = (QueryBehavior.None, default);
		if (DynamicState != null)
		{
			foreach (var item in DynamicState.GetInvocationList().Cast<Func<TArg, (QueryBehavior, TState)>>())
			{
				var tempResult = item(arg);
				if (result.Item1.HasFlag(QueryBehavior.Intercept) && tempResult.Item1.HasFlag(QueryBehavior.Intercept))
				{
					throw new InvalidOperationException($"多个动态处理器对参数类型 {typeof(TArg)} 返回了互斥结果");
				}
				result = tempResult;
			}
		}
		if (!result.Item1.HasFlag(QueryBehavior.Intercept))
		{
			StaticState.TryGetValue(arg, out result);
		}
		return result;
	}
}
public class ArgumentTree<TState>
{
	TreeNode<Argument<TState>> Tree;
	Dictionary<Type, Argument<TState>> Arguments;

	public ArgumentTree()
	{
		var root = new Argument<TState, object>();
		Arguments = new Dictionary<Type, Argument<TState>>() { [typeof(object)] = root };
		Tree = new TreeNode<Argument<TState>>(root);
	}
	public Argument<TState, TArg> GetArgument<TArg>()
	{
		return GetArgument(typeof(TArg)) as Argument<TState, TArg>
			?? throw new InvalidOperationException($"未找到参数类型 {typeof(TArg)} 的处理器");
	}
	public Argument<TState> GetArgument(Type type)
	{
		if (!Arguments.TryGetValue(type, out var argument))
		{
			argument = new Argument<TState, object>();
			 

		}
		return argument;
	}
}