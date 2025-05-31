using DeepSeekClient.Model.Balance;
using DeepSeekClient.Model.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
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
