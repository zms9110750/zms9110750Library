using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Request;

/// <summary>
/// 函数工具选择
/// </summary>
/// <param name="Function">函数名</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolChoiceFunction), typeDiscriminator: "function")]
public record ToolChoiceFunction(FunctionName Function)
{
	/// <summary>
	/// 通过委托自动绑定
	/// </summary>
	/// <param name="Function"></param>
	public ToolChoiceFunction(Delegate Function) : this(new FunctionName(Function.Method.Name)) { }
}
