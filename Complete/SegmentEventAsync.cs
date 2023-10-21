namespace zms9110750Library.Complete;
internal class SegmentEventAsync<TMoment> : IAsyncEnumerable<TMoment> where TMoment : struct, Enum
{
	public event Func<IAsyncEnumerator<TMoment>>? EventHandlers;
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859", Justification = "<挂起>")]
	static readonly IEnumerable<TMoment> moments = Enum.GetValues<TMoment>();

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2012:正确使用 ValueTask", Justification = "<挂起>")]
	public async IAsyncEnumerator<TMoment> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		#region 初始化 
		IEnumerator<TMoment> enumeratorMoment = moments.GetEnumerator();
		Queue<IAsyncEnumerator<TMoment>> queueHandler = new Queue<IAsyncEnumerator<TMoment>>();
		Queue<ValueTask<bool>>? queueTask = new Queue<ValueTask<bool>>();
		HashSet<ValueTask> dispose = new HashSet<ValueTask>();
		foreach (var item in EventHandlers?.GetInvocationList().OfType<Func<IAsyncEnumerator<TMoment>>>() ?? Enumerable.Empty<Func<IAsyncEnumerator<TMoment>>>())
		{
			queueHandler.Enqueue(item.Invoke());
		}
		foreach (var item in queueHandler)
		{
			queueTask.Enqueue(item.MoveNextAsync());
		}
		#endregion
		while (!cancellationToken.IsCancellationRequested)
		{
			#region 等待所有异步执行完毕，并剔除不再需要的迭代器 
			foreach (var item in queueTask)
			{
				var p = queueHandler.Dequeue();
				if (await item.ConfigureAwait(true))
				{
					queueHandler.Enqueue(p);
				}
				else
				{
					dispose.Add(p.DisposeAsync());
				}
			}
			#endregion
			#region 等待不需要的迭代器完成释放
			foreach (var item in dispose)
			{
				await item.ConfigureAwait(true);
			}
			queueTask.Clear();
			dispose.Clear();
			#endregion
			if (!enumeratorMoment.MoveNext())
			{
				break;
			}
			yield return enumeratorMoment.Current;
			#region 运行下一轮迭代器
			foreach (var item in queueHandler)
			{
				if (item.Current.Equals(enumeratorMoment.Current))
				{
					queueTask.Enqueue(item.MoveNextAsync());
				}
				else
				{
					queueTask.Enqueue(ValueTask.FromResult(true));
				}
			}
			#endregion
		}
		enumeratorMoment.Dispose();
		queueHandler.Clear();
	}
}
