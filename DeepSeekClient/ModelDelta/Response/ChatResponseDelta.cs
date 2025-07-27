using System.Buffers;
using System.IO.Pipelines;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.ModelDelta.Response;

/// <summary>
/// 流式聊天增量合并器
/// </summary>
public sealed class ChatResponseDelta<TDelta> : IAsyncEnumerable<TDelta>, IDisposable
{
	private int _participant;
	private List<TDelta> ChoicesDelta { get; } = new();
	private SemaphoreSlim Semaphore { get; } = new(1);
	private CancellationTokenSource InternalCts { get; }
	/// <summary>
	/// 流结束时返回的Usage
	/// </summary>
	public Task<Usage?> ReadingTask { get; }
	private bool Disposed => InternalCts.IsCancellationRequested;
	private HashSet<IDisposable>? Disposables { get; }
	private Stream Stream { get; } 
	internal ChatResponse<TDelta>? Start { get; private set => field ??= value; }
	/// <summary>
	/// 创建一个新的流式合并器
	/// </summary>
	/// <param name="stream">带有内容的流</param>
	/// <param name="externalCt">取消令牌</param>
	/// <param name="disposables">需要跟着这个实例一起释放的其他东西</param>
	public ChatResponseDelta(Stream stream, CancellationToken externalCt = default, params IEnumerable<IDisposable>? disposables)
	{
		Stream = stream;
		InternalCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
		Disposables = disposables?.ToHashSet();
		ReadingTask = ReadStreamAsync();
	}
	private async Task<Usage?> ReadStreamAsync()
	{
		await Task.Yield();
		using var _this = this;
		var sse = SseParser.Create(Stream, Convert);
		await foreach (var item in sse.EnumerateAsync(InternalCts.Token))
		{
			var chunk = item.Data;
			if ((Start ??= chunk).Id != chunk.Id)
			{
				throw new InvalidOperationException($"Response id not match. expect:[{Start?.Id}],[actual:{chunk.Id}]");
			}

			lock (ChoicesDelta)
			{
				ChoicesDelta.AddRange(chunk.Choices);
				if (Volatile.Read(ref _participant) is > 0 and { } currentParticipants)
				{
					Semaphore.Release(currentParticipants);
				}
			}

			if (chunk.Usage != null)
			{
				return chunk.Usage;
			}
		}
		return null;
	}
	static ChatResponse<TDelta> Convert(string eventType, ReadOnlySpan<byte> data)
	{
		return JsonSerializer.Deserialize<ChatResponse<TDelta>>(data, SourceGenerationContext.NetworkOptions)!;
	}
	/// <summary>
	/// 获取中途的增量数据
	/// </summary>
	public async IAsyncEnumerator<TDelta> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		try
		{
			Interlocked.Increment(ref _participant);
			int i = 0;
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (i < ChoicesDelta.Count)
				{
					yield return ChoicesDelta[i++];
				}
				else if (!Disposed)
				{
					await Semaphore.WaitAsync(cancellationToken);
				}
				else
				{
					break;
				}
			}
		}
		finally
		{
			Interlocked.Decrement(ref _participant);
		}
	}
	/// <summary>
	/// 释放资源
	/// </summary>
	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		InternalCts.Cancel();
		InternalCts.Dispose();
		if (Volatile.Read(ref _participant) is > 0 and { } currentParticipants)
		{
			Semaphore.Release(currentParticipants);
		}
		Stream.Dispose();
		Semaphore.Dispose();
		if (Disposables != null)
		{
			foreach (var disposable in Disposables)
			{
				disposable.Dispose();
			}
		}
	}
}


