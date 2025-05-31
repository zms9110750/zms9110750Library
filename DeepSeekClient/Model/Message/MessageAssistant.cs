using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Tool;

namespace DeepSeekClient.Model.Message;
/// <summary>
/// AI/语言模型消息
/// </summary>
/// <param name="Content">消息内容</param>
/// <param name="ReasoningContent">思维链内容</param>
/// <param name="Name">可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。现在没用</param>
/// <param name="ToolCalls">工具调用请求。使用<see cref="ToolKit.Invoke(ToolCall, System.Text.Json.JsonSerializerOptions?)"/>生成一个工具消息</param>
/// <param name="Prefix">前缀内容开关</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(MessageAssistant), typeDiscriminator: "assistant")]
public record MessageAssistant(string? Content, string? ReasoningContent = null, string? Name = null, List<ToolCall>? ToolCalls = null, bool? Prefix = null) : Message(Content, Name);