using System.Diagnostics.CodeAnalysis;
using System.Text;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;

namespace zms9110750.DeepSeekClient.Model.Chat.Tool.Function;
/// <summary>
/// 函数工具调用请求
/// </summary>
/// <param name="Index">索引</param>
/// <param name="Id">唯一标识符</param>
/// <param name="Function">函数内容</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolCallFunction), typeDiscriminator: "function")]
public record ToolCallFunction(int Index, string Id, ToolCallFunctionChoice Function) : IToolCallFunction
{
	/// <summary>
	/// 索引
	/// </summary>
	public int Index { get; set; } = Index;
	/// <summary>
	/// 唯一标识符
	/// </summary>
	public string Id { get; set; } = Id;
	/// <summary>
	/// 函数名
	/// </summary>
	[JsonIgnore] public string Name { get => Function.Name; set => Function.Name = value; }
	/// <summary>
	/// 参数。如果内容完整，应该是一个jsonObject
	/// </summary>
	[JsonIgnore] public string Arguments { get => Function.Arguments; set => Function.Arguments = value; }

	IFunctionCallComplete IToolCallFunction.Function => Function;

	IMerge<IToolCall> IDelta<IToolCall>.CreateMerge()
	{
		return new ToolCallFunctionMerge(this);
	}

	IMerge<IToolCallFunction> IDelta<IToolCallFunction>.CreateMerge()
	{
		return new ToolCallFunctionMerge(this);
	}
}

/// <summary>
/// 函数调用请求内容
/// </summary>
/// <param name="Name">函数名</param>
/// <param name="Arguments">参数。如果内容完整，应该是一个jsonObject</param>
public record ToolCallFunctionChoice(string Name, string Arguments) : IFunctionCallComplete
{
	[AllowNull] private JsonObject ArgumentsJson { get => field ??= JsonNode.Parse(Arguments)!.AsObject(); set; }

	/// <summary>
	/// 函数名
	/// </summary>
	public string Name { get; set; } = Name;

	/// <summary>
	/// 参数。如果内容完整，应该是一个jsonObject
	/// </summary>
	public string Arguments { get; set { field = value; ArgumentsJson = null; } } = Arguments;

	JsonObject IFunctionCallComplete.ArgumentsJson => ArgumentsJson;
}
internal class ToolCallFunctionMerge(IToolCallFunction toolCallFunction) : IMerge<IToolCallFunction>, IMerge<IToolCall>
{
	int Index { get; } = toolCallFunction.Index;
	string Id { get; } = toolCallFunction.Id;
	string Name { get; } = toolCallFunction.Function.Name;
	StringBuilder Arguments { get; } = new StringBuilder(toolCallFunction.Function.Arguments);
	public void Merge(IToolCallFunction other)
	{
		if (other.Index != Index)
		{
			throw new ArgumentException("Index not match");
		}
		Arguments.Append(other.Function.Arguments);
	}

	public void Merge(IToolCall other)
	{
		if (other is not IToolCallFunction toolCallFunction)
		{
			throw new ArgumentException("FormatType not match");
		}
		Merge(toolCallFunction);
	}

	public IToolCallFunction ToFinish()
	{
		return new ToolCallFunction(Index, Id, new ToolCallFunctionChoice(Name, Arguments.ToString()));
	}

	IToolCall IMerge<IToolCall>.ToFinish()
	{
		return ToFinish();
	}
}