using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace zms9110750.DeepSeekClient.Model.Tool.Functions;

/// <summary>
/// 参数列表
/// </summary>
public class Parameter
{

	/// <summary>
	/// 架构类型，始终为object
	/// </summary>
	public string Type => "object";
	/// <summary>
	/// 参数列表
	/// </summary>
	public IReadOnlyDictionary<string, JsonObject> Properties { get; }
	/// <summary>
	/// 必填参数列表
	/// </summary>
	public IReadOnlyList<string> Required { get; }
	/// <summary>
	/// 根据参数列表生成参数架构
	/// </summary>
	/// <param name="parameters"></param>
	public Parameter(IEnumerable<ParameterInfo> parameters)
	{
		var dic = new Dictionary<string, JsonObject>();
		Properties = dic;
		var required = new List<string>();
		foreach (var parameter in parameters)
		{
			var json = SourceGenerationContext.InternalOptions.GetJsonSchemaAsNode(parameter.ParameterType).AsObject();
			if (parameter.GetCustomAttribute<DescriptionAttribute>()?.Description is string description)
			{
				json["description"] = description;
			}
			dic.Add(parameter.Name!, json);
			if (!parameter.HasDefaultValue)
			{
				required.Add(parameter.Name!);
			}
		}
		Required = required;
	}
}