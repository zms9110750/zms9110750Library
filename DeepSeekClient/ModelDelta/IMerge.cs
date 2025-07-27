
namespace zms9110750.DeepSeekClient.ModelDelta;

internal interface IMerge<T>
{
	public void Merge(T source);
	/// <summary>
	/// 合并为完整的对象
	/// </summary> 
	T ToFinish(); 
}  
