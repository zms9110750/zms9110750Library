using Newtonsoft.Json.Schema.Generation;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;

namespace zms9110750.DeepSeekClient.Model.Tool.FunctionTool;

/// <summary>
/// 参数列表
/// </summary>
public class Parameter
{
	static JSchemaGenerator Generator { get; } = new JSchemaGenerator();
	static Parameter()
	{
		Generator.GenerationProviders.Add(new StringEnumGenerationProvider());
	}
	/// <summary>
	/// 架构类型，始终为object
	/// </summary>
	public string Type => "object"; 
	internal Dictionary<string, JsonObject> Properties { get; } = new();
	internal IReadOnlyList<string> Required { get; }
	internal Parameter(IEnumerable<ParameterInfo> parameters)
	{
		var required = new List<string>();
		foreach (var parameter in parameters)
		{
			var schema = Generator.Generate(parameter.ParameterType);
			var json = JsonNode.Parse(schema.ToString())!.AsObject();
			if (parameter.GetCustomAttribute<DescriptionAttribute>()?.Description is string description)
			{
				json["description"] = description;
			}
			Properties.Add(parameter.Name!, json);
			if (!parameter.HasDefaultValue)
			{
				required.Add(parameter.Name!);
			}
		}
		Required = required;
	}
}