using DeepSeekClient.Model.Tool;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DeepSeekClient.NewModel.Tool;

public partial class FunctionTool : ToolBase
{
	const string NameVerifyRegex = @"^[a-zA-Z0-9_-]{1,64}$";
	public override string Type => "function";
	public string Description { get; init => Json["description"] = field = value; }
	public ParameterSchema Parameters { get; init => Json["parameters"] = JsonObject.Parse(value.ToString()!); }
	Delegate Delegate { get; }
	IReadOnlyList<ParameterInfo> ParametersInfo { get; }
	Type ReturnType { get; }

	public FunctionTool(Delegate function) : base(function?.Method?.Name!)
	{
		switch (function)
		{
			case null:
				throw new ArgumentNullException(nameof(function));
			case { HasSingleTarget: false }:
				throw new ArgumentException("Function must have a single target.");
		}
		if (!NameVerify().IsMatch(Name))
		{
			throw new ArgumentException($"The function name must match the regex pattern '{NameVerifyRegex}'.");
		}
		Description = function.Method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
		Parameters = new ParameterSchema(function.Method.GetParameters());
		Delegate = function;
		ParametersInfo = function.Method.GetParameters();
		ReturnType = function.Method.ReturnType;
	}
	public override JsonObject Invoke(ToolEntry toolEntry)
	{
		if (toolEntry is not ToolEntryFunction { Arguments: var arguments })
		{
			throw new ArgumentException("Invalid tool entry type.");
		}
		var result = new JsonObject();
		try
		{
			object? returnValue;
			if (ParametersInfo.Count == 0)
			{
				returnValue = Delegate.DynamicInvoke();
			}
			else
			{
				var argumentsNode = (JsonNode.Parse(arguments)?.AsObject()) ?? throw new ArgumentException("Invalid JSON arguments");
				var args = new object?[ParametersInfo.Count];
				foreach (var (i, param) in ParametersInfo.Index())
				{
					args[i] = argumentsNode.TryGetPropertyValue(param.Name!, out var paramValue)
						? paramValue.Deserialize(param.ParameterType)
						: param.HasDefaultValue
						? param.DefaultValue
						: throw new ArgumentException($"Missing required argument: {param.Name}");
				}
				returnValue = Delegate.DynamicInvoke(args);
			}
			result["result"] = (ReturnType == typeof(void)) ? null : JsonValue.Create(returnValue);
			result["error"] = null;
		}
		catch (Exception ex)
		{
			// 捕获异常，返回错误信息
			result["result"] = null;
			result["error"] = ex.Message;
		}
		return result;
	}


	[GeneratedRegex(NameVerifyRegex)]
	private static partial Regex NameVerify();
}
