using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;
using zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

namespace zms9110750.DeepSeekClient.Model.Chat.Tool;

/// <summary>
/// 工具接口
/// </summary>
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(IToolFunction))]
public interface ITool
{
	/// <summary>
	/// 工具类型描述
	/// </summary>
	ToolType Type { get; }
}

/// <summary>
/// 工具描述接口
/// </summary>
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(IToolRequestFunction))]
public interface IToolRequest : ITool
{
	/// <summary>
	/// 执行工具
	/// </summary>
	/// <param name="call">提供内容的工具调用接口</param>
	string Invok(IToolCall call);
}

/// <summary>
/// 工具调用接口
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(ToolCallFunction), typeDiscriminator: "function")]
public interface IToolCall : ITool, IDelta<IToolCall>
{
	/// <summary>
	/// tool 调用的 ID
	/// </summary>
	string Id { get; }
	/// <summary>
	/// 索引
	/// </summary>
	int Index { get; }
}

/// <summary>
/// 工具类型
/// </summary>
public enum ToolType
{
	/// <summary>
	/// 占位符
	/// </summary>
	None,
	/// <summary>
	/// 函数工具
	/// </summary>
	Function
}