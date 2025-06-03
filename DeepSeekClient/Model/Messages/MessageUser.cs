using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Messages;
/// <summary>
/// 用户消息
/// </summary>
/// <param name="Content">消息内容</param>
/// <param name="Name">可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。现在没用</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(MessageUser), typeDiscriminator: "user")]
public record MessageUser(string Content, string? Name = null) : Message(Content, Name);
