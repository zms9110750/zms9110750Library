using System.Runtime.CompilerServices; 
using zms9110750.DeepSeekClient.Beta;
using zms9110750.DeepSeekClient.Model.Response;

namespace zms9110750.DeepSeekClient.ModelDelta.Response;
public static class ChatResponseDeltaExtensions
{
	public static TaskAwaiter<ChatResponse<Choice>> GetAwaiter(this ChatResponseDelta<ChoiceDelta> delta)
	{
		return Task.Run(async () =>
			{
				List<ChoiceMerge> merge = new List<ChoiceMerge>(1);
				await foreach (var msg in delta)
				{
					if (msg.Index < merge.Count)
					{
						merge[msg.Index].Merge(msg);
					}
					else if (msg.Index == merge.Count)
					{
						merge.Add(new ChoiceMerge(msg));
					}
					else
					{
						throw new InvalidOperationException("Invalid index");
					}
				}
				return delta?.Start?.With(merge.Select(s => s.ToFinish().ToChoice()))!;
			}).GetAwaiter();
	}
	public static TaskAwaiter<ChatResponse<ChoiceFIM>> GetAwaiter(this ChatResponseDelta<ChoiceFIM> delta)
	{
		return Task.Run(async () =>
		{
			List<ChoiceFIMMerge> merge = new List<ChoiceFIMMerge>(1);
			await foreach (var msg in delta)
			{
				if (msg.Index < merge.Count)
				{
					merge[msg.Index].Merge(msg);
				}
				else if (msg.Index == merge.Count)
				{
					merge.Add(new ChoiceFIMMerge(msg));
				}
				else
				{
					throw new InvalidOperationException("Invalid index");
				}
			}
			return delta?.Start?.With(merge.Select(s => s.ToFinish()))!;
		}).GetAwaiter();
	}
}
