using System.Collections.Frozen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;

namespace zms9110750.DeepSeekClient.Model.Chat.Tool.Function;


/// <summary>
/// 函数工具请求体
/// </summary>
/// <remarks>默认自动分析<see cref="DescriptionAttribute"/>和<see cref="ValidationAttribute"/>下的特性。<br/>
/// 改变<see cref="ValidationSchemaPipeline.Middlewares"/>可以改变默认操作。
/// </remarks>
public partial class FunctionRequest : IToolRequestFunction, IFunctionRequest, IFunctionParameter
{
	const string NameVerifyRegex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
	ParameterInfo[] Parameters { get; }
	object[] Arguments { get; }
	Delegate Function { get; }
	/// <inheritdoc/>
	public string? Description { get; set; }
	/// <inheritdoc/>
	public bool? Strict { get; set; }
	/// <inheritdoc/>
	public string Name
	{
		get;
		set => field = NameVerify().IsMatch(value)
			? value
			: throw new ArgumentOutOfRangeException(nameof(Name), value, "Invalid function name, must match regex: " + NameVerifyRegex);
	}
	/// <inheritdoc/>
	public IReadOnlyDictionary<string, JsonObject> Properties { get; }
	/// <inheritdoc/>
	public IEnumerable<string> Required { get; }
	IFunctionRequest IToolRequestFunction.Function => this;
	IFunctionParameter IFunctionRequest.Parameters => this;

	/// <summary>
	/// 从委托构造一个函数工具
	/// </summary>
	/// <remarks>会处理<see cref="DescriptionAttribute"/>和<see cref="ValidationAttribute"/>特性的派生特性<br/> 
	/// </remarks>
	public FunctionRequest(Delegate function)
	{
		if (!function.HasSingleTarget)
		{
			throw new ArgumentException("Function must have a single target.");
		}
		Function = function;
		Description = function.Method.GetCustomAttribute<DescriptionAttribute>()?.Description;
		Name = function.Method.Name;
		Parameters = function.Method.GetParameters();
		Arguments = new object[Parameters.Length];

		var methodSchema = PublicSourceGenerationContext.InternalOptions.GetMethodSchemaAsNode(function.Method);

		Properties = methodSchema["properties"]!.Deserialize<Dictionary<string, JsonObject>>(PublicSourceGenerationContext.InternalOptions)!.ToFrozenDictionary();
		Required = methodSchema["required"]!.Deserialize<string[]>(PublicSourceGenerationContext.InternalOptions)!.ToFrozenSet();

	}

	/// <inheritdoc/>
	public string Invok(IToolCallFunction call)
	{
		return Invoke(call, PublicSourceGenerationContext.NetworkOptions).ToJsonString(PublicSourceGenerationContext.NetworkOptions);
	}

	/// <summary>
	/// 执行一个工具调用请求
	/// </summary> 
	/// <returns>介绍函数调用结果的JSON对象</returns>
	/// <exception cref="ArgumentException"></exception>
	public JsonObject Invoke(IToolCallFunction call, JsonSerializerOptions? options = null)
	{
		options ??= PublicSourceGenerationContext.InternalOptions;
		JsonObject json = new JsonObject
		{
			["return_type"] = Function.Method.ReturnType.FullName
		};
		try
		{
			Array.Clear(Arguments);
			foreach (var parameter in Parameters)
			{
				Arguments[parameter.Position] = call.Function.ArgumentsJson[parameter.Name!] is JsonNode node
					? node.Deserialize(parameter.ParameterType, options)!
					: parameter.DefaultValue!;
			}
			object? result = Function.DynamicInvoke(Arguments);
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

	/// <summary>
	/// 把一个参数限定为可选范围内的值
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <param name="values"></param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>

	public void ParametersWithEnum<T>(string name, IEnumerable<T>? values)
	{
		if (!Properties.TryGetValue(name, out var json))
		{
			throw new ArgumentOutOfRangeException(nameof(name), name, "Invalid parameter name.");
		}

		// 确保存在 allOf 结构
		if (!json.TryGetPropertyValue("allOf", out var allOfNode) || allOfNode is not JsonArray allOfArray)
		{
			// 如果没有 allOf，则创建 allOf 结构，将原有 schema 作为第一个元素 
			allOfArray = new JsonArray { json.DeepClone() };
			json.Clear();
			json["allOf"] = allOfArray;
		}

		// 添加或更新枚举约束
		if (values == null && allOfArray.Count > 1)
		{
			allOfArray.RemoveAt(1);
		}
		else if (values != null)
		{
			// 添加或更新枚举约束
			var enumConstraint = new JsonObject
			{
				["enum"] = JsonSerializer.SerializeToNode(values, PublicSourceGenerationContext.InternalOptions)
			};

			if (allOfArray.Count > 1)
			{
				allOfArray[1] = enumConstraint;
			}
			else
			{
				allOfArray.Add(enumConstraint);
			}
		}
	}
}


internal static class MethodInfoSchemaExtensions
{
	/// <summary>
	/// 获取方法的JSONSchema架构
	/// </summary> 
	public static JsonObject GetMethodSchemaAsNode(this JsonSerializerOptions? serializerOptions, MethodInfo methodInfo, JsonSchemaExporterOptions? schemaOptions = null)
	{
		schemaOptions ??= ValidationSchemaPipeline.CreatePipelineSchemaExporter();
		serializerOptions ??= PublicSourceGenerationContext.InternalOptions;

		var parameters = methodInfo.GetParameters();
		var properties = new JsonObject();
		var required = new JsonArray();

		foreach (var parameter in parameters)
		{
			properties[parameter.Name!] = ValidationSchemaPipeline
				.ApplyValidationSchema(serializerOptions
						.GetJsonSchemaAsNode(parameter.ParameterType, schemaOptions)
						.AsObject()
					, parameter.GetCustomAttributes<Attribute>());

			if (!parameter.HasDefaultValue && !parameter.IsOptional)
			{
				required.Add(parameter.Name!);
			}
		}

		return new JsonObject
		{
			["type"] = "object",
			["name"] = methodInfo.Name,
			["properties"] = properties,
			["required"] = required
		};
	}
}