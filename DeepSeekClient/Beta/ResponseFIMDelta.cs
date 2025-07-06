using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.Beta;
/// <summary>
/// 中间填充响应的流式合并器
/// </summary>
public class ResponseFIMDelta : IAsyncEnumerable<ChoiceFIM>, IDisposable
{
	private const string StreamDoneSign = "[DONE]";
	private const string StreamDataSign = "data: ";
	private const int StreamDataLength = 6;
	private List<ChoiceFIM> ChoicesMerge { get; } = new();
	private List<ChoiceFIM> ChoicesAll { get; } = new();
	private HashSet<Channel<ChoiceFIM>> Observers { get; } = new();
	private CancellationTokenSource InternalCts { get; }
	private Task<ChatResponseFIM?> ReadingTask { get; }
	private bool Disposed => InternalCts.IsCancellationRequested;
	private HashSet<IDisposable>? Disposables { get; }
	private ChatResponseFIM? Start { get; set; }
	Stream Stream { get; }
	/// <summary>
	/// 创建一个新的流式合并器
	/// </summary>
	/// <param name="stream">带有内容的流</param>
	/// <param name="externalCt">取消令牌</param>
	/// <param name="disposables">需要跟着这个实例一起释放的其他东西</param>
	public ResponseFIMDelta(Stream stream, CancellationToken externalCt = default, params IEnumerable<IDisposable>? disposables)
	{
		Stream = stream;
		InternalCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
		Disposables = disposables?.ToHashSet();
		ReadingTask = ReadStreamAsync();
	}
	/// <summary>
	/// 获取最后合并的结果
	/// </summary>
	/// <returns></returns>
	public TaskAwaiter<ChatResponseFIM?> GetAwaiter() => ReadingTask.GetAwaiter();

	/// <summary>
	/// 获取中途的增量数据
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async IAsyncEnumerator<ChoiceFIM> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		for (int i = 0; i < ChoicesAll.Count; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			yield return ChoicesAll[i];
		}
		Channel<ChoiceFIM> channel;
		lock (Observers)
		{
			if (Disposed)
			{
				yield break;
			}
			channel = Channel.CreateUnbounded<ChoiceFIM>();
			Observers.Add(channel);
		}

		try
		{
			await foreach (var choice in channel.Reader.ReadAllAsync(cancellationToken))
			{
				yield return choice;
			}
		}
		finally
		{
			lock (Observers)
			{
				Observers.Remove(channel);
			}
		}
	}

	private async Task<ChatResponseFIM?> ReadStreamAsync()
	{
		await Task.Yield();
		using (this)
		using (var reader = new StreamReader(Stream))
		{
			string? line;
			while (!reader.EndOfStream && !InternalCts.IsCancellationRequested)
			{
				try
				{
					line = await reader.ReadLineAsync(InternalCts.Token);
					if ((line?.StartsWith(StreamDataSign)) != true)
					{
						continue;
					}
					line = line.Substring(StreamDataLength);
					if (line == StreamDoneSign)
					{
						break;
					}
				}
				catch (OperationCanceledException)
				{
					break;
				}
				var chunk = JsonSerializer.Deserialize(line, SourceGenerationContext.Default.ChatResponseFIM)!;
				if ((Start ??= chunk).Id != chunk.Id)
				{
					throw new InvalidOperationException($"Response id not match. expect:[{Start?.Id} ],[actual:{chunk.Id}]");
				}

				foreach (var choice in chunk.Choices)
				{
					ChoicesAll.Add(choice);
					if (choice.Index < ChoicesMerge.Count)
						ChoicesMerge[choice.Index].Merge(choice);
					else
						ChoicesMerge.Add(choice);

					lock (Observers)
					{
						foreach (var ch in Observers)
						{
							ch.Writer.TryWrite(choice);
						}
					}
				}
				if (chunk.Usage != null)
				{
					return Start with
					{
						Choices = ChoicesMerge.ConvertAll(c => c.ToFinish()),
						Usage = chunk.Usage // 包含最新Usage数据
					};
				}
			}
			return Start == null ? null : Start with
			{
				Choices = ChoicesMerge.ConvertAll(c => c.ToFinish())
			};
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
		lock (Observers)
		{
			foreach (var ch in Observers)
			{
				ch.Writer.Complete();
			}
			Observers.Clear();
		}
		Stream.Dispose();
		if (Disposables != null)
		{
			foreach (var disposable in Disposables)
			{
				disposable.Dispose();
			}
		}
	}
}
