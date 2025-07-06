using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Tool.Functions;
/// <summary>
/// 函数工具调用请求
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Id">唯一标识符</param>
/// <param name="Function">函数内容</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolCallFunction), typeDiscriminator: "function")]
public record ToolCallFunction(int Index, string Id, ToolCallFunctionChoice Function) : ToolCall(Index, Id);


