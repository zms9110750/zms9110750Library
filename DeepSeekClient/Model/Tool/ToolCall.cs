using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Tool.Functions; 

namespace zms9110750.DeepSeekClient.Model.Tool;
/// <summary>
/// 工具调用请求
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Id">唯一标识符</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolCallFunction), typeDiscriminator: "function")]
public abstract record ToolCall(int Index, string Id);


