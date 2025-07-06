using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Tool;
using zms9110750.DeepSeekClient.ModelDelta.Tool;
namespace zms9110750.DeepSeekClient.Model.Messages;
/// <summary>
/// 工具调用消息
/// </summary>
/// <param name="Content">消息内容</param>
/// <param name="ToolCallId">工具调用ID。必须和<see cref="ToolCall.Id"/>对应，AI才知道这个消息是回应什么调用</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(MessageTool), typeDiscriminator: "tool")]
public record MessageTool(string Content, string ToolCallId) : Message(Content);