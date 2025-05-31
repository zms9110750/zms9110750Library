using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using zms9110750.DeepSeekClient.Model.Tool.FunctionCall;

namespace zms9110750.DeepSeekClient.Model.Tool.FunctionTool;

/// <summary>
/// 函数工具
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolFunction), typeDiscriminator: "function")]
public partial class ToolFunction : Tool
{
	/// <summary>
	/// 函数描述
	/// </summary>
	public Function Function { get; }
	Delegate Delegate { get; }
	ParameterInfo[] Parames { get; }
	const string NameVerifyRegex = "^[a-zA-Z_][a-zA-Z0-9_]*$";

	/// <summary>
	/// 分析一个委托并生成一个函数工具
	/// </summary>
	/// <param name="function"></param>
	/// <exception cref="ArgumentException"></exception>
	public ToolFunction(Delegate function)
	{
		ArgumentNullException.ThrowIfNull(function);
		if (!function.HasSingleTarget)
		{
			throw new ArgumentException("Function must have a single target.");
		}
		if (!NameVerify().IsMatch(function.Method.Name))
		{
			throw new ArgumentException($"The function name must match the regex pattern '{NameVerifyRegex}'.");
		}
		Delegate = function;
		Parames = Delegate.Method.GetParameters();
		Function = new Function(Delegate.Method.Name, Delegate.Method.GetCustomAttribute<DescriptionAttribute>()?.Description, new Parameter(Parames));
	}
	/// <summary>
	/// 执行一个工具调用请求
	/// </summary>
	/// <param name="call"></param>
	/// <param name="options"></param>
	/// <returns>介绍函数调用结果的JSON对象</returns>
	/// <exception cref="ArgumentException"></exception>
	public override JsonObject Invoke(ToolCall call, JsonSerializerOptions? options = null)
	{
		if (call is not ToolCallFunction functionCall)
		{
			throw new ArgumentException("Invalid call type.");
		}
		var parameters = JsonNode.Parse(functionCall.Function.Arguments)!.AsObject();
		JsonObject json = new JsonObject
		{
			["return_type"] = Delegate.Method.ReturnType.FullName
		};
		try
		{
			object? result = null;
			if (Parames.Length == 0)
			{
				result = Delegate.DynamicInvoke();
			}
			else
			{
				object[] args = new object[Parames.Length];
				foreach (var parameter in Parames)
				{
					if (parameters[parameter.Name!] is JsonNode node)
					{
						args[parameter.Position] = JsonSerializer.Deserialize(node.ToString(), parameter.ParameterType)!;
					}
					args[parameter.Position] ??= parameter.DefaultValue!;
				}
				result = Delegate.DynamicInvoke(args);
			}
			json["result"] = result == null ? null : JsonSerializer.SerializeToNode(result, options);
		}
		catch (Exception ex)
		{
			json["error"] = new JsonObject
			{
				["type"] = ex.GetType().FullName,
				["message"] = ex.Message
			};
		}
		return json;
	}
	[GeneratedRegex(NameVerifyRegex)]
	private static partial Regex NameVerify();
}
