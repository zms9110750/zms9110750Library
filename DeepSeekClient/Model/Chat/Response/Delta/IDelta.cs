namespace zms9110750.DeepSeekClient.Model.Chat.Response.Delta;
/// <summary>
/// 提供一个方法创建自己的合并器
/// </summary>
public interface IDelta<T>
{

	/// <summary>
	/// 创建自己的合并器
	/// </summary>
	/// <returns></returns>
	IMerge<T> CreateMerge();
}


/// <summary>
/// 提供索引属性
/// </summary>
public interface IIndex
{
	/// <summary>
	/// 索引
	/// </summary>
	int Index { get; }

}
/// <summary>
/// 提供合并方法和一个方法将合并结果转换为完整的对象
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMerge<T>
{

	/// <summary>
	/// 将目标对象合并到当前对象
	/// </summary>
	/// <param name="other"></param>
	void Merge(T other);
	/// <summary>
	/// 合并为完整的对象
	/// </summary> 
	T ToFinish();
}