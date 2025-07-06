 
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization; 
using zms9110750.DeepSeekClient.Model.Tool.FunctionTool;

namespace zms9110750.DeepSeekClient.Model.Tool;
/// <summary>
/// 工具抽象类
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolFunction), typeDiscriminator: "function")] 
public abstract class Tool
{
	/// <summary>
	/// 执行工具请求。
	/// </summary>
	/// <param name="call"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public abstract JsonObject Invoke(ToolCall call, JsonSerializerOptions? options = null); 
}
