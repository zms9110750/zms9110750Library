using System.Runtime.CompilerServices;
using zms9110750.DeepSeekClient.Model.Chat.Messages;
using zms9110750.DeepSeekClient.Model.Chat.Request;
using zms9110750.DeepSeekClient.Model.Chat.Response;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;
using zms9110750.DeepSeekClient.Model.Chat.Tool;
using zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

namespace zms9110750.DeepSeekClient;
/// <summary>
/// 工具箱的扩展方法
/// </summary>
public static class ToolKitExtensions
{
	/// <summary>
	/// 添加一个函数工具
	/// </summary>
	/// <param name="toolKit"></param>
	/// <param name="function"></param>
	/// <returns></returns>
	public static FunctionRequest Add(this ToolKit toolKit, Delegate function)
	{
		var fun = new FunctionRequest(function);
		toolKit.Add(fun);
		return fun;
	}
}


/// <summary>
/// 为<see cref="ChatResponseDelta{T}"/>提供await支持
/// </summary>
public static class ChatResponseDeltaExtensions
{
	/// <summary>
	/// 获取<see cref="ChatResponse{ChoiceDelta}"/>的合并结果
	/// </summary> 
	/// <exception cref="InvalidOperationException"></exception>
	public static TaskAwaiter<IChatResponse<T>?> GetAwaiter<T>(this IAsyncEnumerable<IChatResponse<T>> delta) where T : IDelta<T>, IIndex
	{
		return delta.MergeAsync().GetAwaiter();
	}


	/// <summary>
	/// 获取<see cref="ChatResponse{ChoiceDelta}"/>的合并结果
	/// </summary> 
	/// <exception cref="InvalidOperationException">序列中存在null值或Id不一致</exception>
	public static async Task<IChatResponse<T>?> MergeAsync<T>(this IAsyncEnumerable<IChatResponse<T>> delta) where T : IDelta<T>, IIndex
	{
		IChatResponse<T>? last = null;
		List<T> merge = new List<T>();
		await foreach (var item in delta)
		{
			if (item == null)
			{
				throw new InvalidOperationException("Invalid response. item is null.");
			}
			if (last != null && last.Id != item.Id)
			{
				throw new InvalidOperationException($"Response id not match. expect:[{last?.Id}],[actual:{item.Id}]");
			}
			last = item;
			merge.AddRange(item.Choices);
		}
		if (last == null)
		{
			return null;
		}
		var p = merge.GroupBy(t => t.Index)
				.Select(s =>
				{
					using var e = s.GetEnumerator();
					e.MoveNext();
					var merge = ((IDelta<T>)e.Current).CreateMerge();
					while (e.MoveNext())
					{
						merge.Merge(e.Current);
					}
					return merge.ToFinish();
				}).ToArray();
		return new ChatResponse<T>(last.Id, last.Object, last.Created, last.Model, last.SystemFingerint, p, last.Usage);
	}
}

/// <summary>
/// 为<see cref="ChatRequest"/>提供便捷方法
/// </summary>
public static class ChatRequestExtensions
{
	/// <summary>
	/// 添加一条用户消息
	/// </summary>
	/// <param name="request"></param>
	/// <param name="message"></param>
	public static void MessageAddUser(this ChatRequest request, string message)
	{
		request.MessagesDefault.Add(Message.NewUserMsg(message));
	}
	/// <summary>
	/// 添加一条助手消息
	/// </summary>
	/// <param name="request"></param>
	/// <param name="message"></param>
	public static void MessageAddAssistant(this ChatRequest request, string message)
	{
		request.MessagesDefault.Add(Message.NewAssistantMsg(message));
	}
	/// <summary>
	/// 添加一条系统消息
	/// </summary>
	/// <param name="request"></param>
	/// <param name="message"></param>
	public static void MessageAddSystem(this ChatRequest request, string message)
	{
		request.MessagesDefault.Add(Message.NewSystemMsg(message));
	}
}


/// <summary>
/// TokenProbability扩展方法
/// </summary>
public static class TokenProbabilityExtensions
{
	/// <summary>
	/// 转为小数概率
	/// </summary> 
	public static double ToProbability(this TokenProbability tokenProbability) => Math.Exp(tokenProbability.Logprob);
}