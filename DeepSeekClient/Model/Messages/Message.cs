using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Beta;
namespace zms9110750.DeepSeekClient.Model.Messages;
/// <summary>
/// 消息基类
/// </summary>
/// <param name="Content">消息内容</param>
/// <param name="Name">可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。现在没用</param>
/// <remarks>没有提供role字段以分辨。必须通过模式匹配来判断类型。
/// <list type="bullet">
/// <item><see cref="MessageUser"/></item>
/// <item><see cref="MessageAssistant"/></item>
/// <item><see cref="MessageSystem"/></item>
/// <item><see cref="MessageTool"/></item>
/// </list>
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(MessageUser), typeDiscriminator: "user")]
[JsonDerivedType(typeof(MessageAssistant), typeDiscriminator: "assistant")]
[JsonDerivedType(typeof(MessageSystem), typeDiscriminator: "system")]
[JsonDerivedType(typeof(MessageTool), typeDiscriminator: "tool")]
public abstract record Message(string? Content, string? Name = null)
{
	/// <summary>
	/// 创建普通用户消息
	/// </summary>
	/// <param name="content">消息内容</param>
	/// <param name="name">可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。现在没用</param>
	/// <returns></returns>
	public static MessageUser NewUserMsg(string content, string? name = null)
	{
		return new MessageUser(content, name);
	}
	/// <summary>
	/// 创建一个假的AI/语言模型消息
	/// </summary>
	/// <param name="content">消息内容</param>
	/// <param name="name">可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。现在没用</param>
	/// <returns></returns>
	public static MessageAssistant NewAssistantMsg(string content, string? name = null)
	{
		return new MessageAssistant(content, "", name);
	}
	/// <summary>
	/// 创建系统/平台消息
	/// </summary>
	/// <param name="content"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public static MessageSystem NewSystemMsg(string content, string? name = null)
	{
		return new MessageSystem(content, name);
	}
	/// <summary>
	/// 创建前缀补全消息
	/// </summary>
	/// <param name="prefix">前缀</param>
	/// <returns></returns>
	/// <remarks>用于<see cref="DeepSeekApiClientBeta.ChatBetaAsync(CancellationToken)"/></remarks>
	public static MessageAssistant NewPrefixMsg(string prefix)
	{
		return new MessageAssistant(prefix,  Prefix: true);
	}
}
