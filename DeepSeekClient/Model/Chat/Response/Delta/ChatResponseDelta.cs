using Nito.AsyncEx;
using System.Net.ServerSentEvents;
namespace zms9110750.DeepSeekClient.Model.Chat.Response.Delta;
/// <summary>
/// 流式聊天增量合并器
/// </summary>
public class ChatResponseDelta<TDelta> : IAsyncEnumerable<IChatResponse<TDelta>>, IDisposable
{
	private List<ChatResponse<TDelta>> Delta { get; } = [];
	private CancellationTokenSource InternalCts { get; }
	private bool Disposed => InternalCts.IsCancellationRequested;
	private HashSet<IDisposable> Disposables { get; } = [];
	private AsyncManualResetEvent Manual { get; } = new();
	internal Task EndTask { get; }

	/// <summary>
	/// 创建一个新的流式合并器
	/// </summary>
	/// <param name="stream">带有内容的流</param>
	/// <param name="externalCt">取消令牌</param>
	/// <param name="disposables">需要跟着这个实例一起释放的其他东西</param>
	public ChatResponseDelta(Stream stream, CancellationToken externalCt = default, params IEnumerable<IDisposable>? disposables)
	{
		InternalCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
		Disposables.Add(stream);
		Disposables.Add(InternalCts);
		foreach (var disposable in disposables ?? [])
		{
			Disposables.Add(disposable);
		}
		EndTask = ReadStreamAsync(stream);
	}
	private async Task ReadStreamAsync(Stream stream)
	{
		try
		{
			await Task.Yield();
			ChatResponse<TDelta>? Last = null;
			var sse = SseParser.Create(stream, Convert);
			await foreach (var item in sse.EnumerateAsync(InternalCts.Token))
			{
				if (item.Data == null)
				{
					break;
				}
				if (Last != null && Last.Id != item.Data.Id)
				{
					throw new InvalidOperationException($"Response id not match. expect:[{Last?.Id}],[actual:{item.Data.Id}]");
				}
				Last = item.Data;
				lock (Delta)
				{
					Delta.Add(item.Data);
					Manual.Set();
				}
			}
		}
		finally
		{
			Dispose();
		}
	}
	static ChatResponse<TDelta>? Convert(string eventType, ReadOnlySpan<byte> data)
	{
		return data.SequenceEqual("[DONE]"u8)
				? null
				: JsonSerializer.Deserialize<ChatResponse<TDelta>>(data, PublicSourceGenerationContext.NetworkOptions)!;
	}
	/// <inheritdoc/>
	public void Dispose()
	{
		lock (InternalCts)
		{
			if (Disposed)
			{
				return;
			}
			InternalCts.Cancel();
		}
		Manual.Set();
		if (Disposables != null)
		{
			foreach (var disposable in Disposables)
			{
				disposable.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public async IAsyncEnumerator<IChatResponse<TDelta>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		int i = 0;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (i < Delta.Count)
			{
				yield return Delta[i++];
			}
			else if (!Disposed)
			{
				try
				{
					await Manual.WaitAsync(cancellationToken).WaitAsync(InternalCts.Token);
					Manual.Reset();
				}
				catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
				{
					// 内部取消时，没有人能再把他们放出了。不进行重设。
				}
				catch
				{
					Manual.Reset();
					throw;
				}
			}
			else
			{
				break;
			}
		}
		await EndTask;
	}
}
