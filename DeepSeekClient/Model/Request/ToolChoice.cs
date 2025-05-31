using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Request;
/// <summary>
/// 工具选择抽象类
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolChoiceFunction), typeDiscriminator: "function")]
public abstract record ToolChoice
{
	/// <summary>
	/// 用委托创建一个要求调用该函数的工具选择
	/// </summary>
	/// <param name="Function">委托</param>
	/// <returns></returns>
	public static ToolChoiceFunction CreatToolChoiceFunction(Delegate Function) => new ToolChoiceFunction(Function);
}
