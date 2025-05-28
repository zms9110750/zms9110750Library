using DeepSeekClient.Clint;
using Newtonsoft.Json.Schema.Generation;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;

namespace DeepSeekClient.Model.Tool;

public sealed class ParameterSchema:WithJsonObject
{
	static JSchemaGenerator JSchemaGenerator { get; } = new JSchemaGenerator();
	static ParameterSchema()
	{
		JSchemaGenerator.GenerationProviders.Add(new StringEnumGenerationProvider());
	} 
	public string Type { get; init => Json["type"] = field = value; }
	public IReadOnlyDictionary<string, JsonObject> Properties { get; init => Json["properties"] = JsonValue.Create(field = value); }
	public IReadOnlyList<string> Required { get; init => Json["required"] = JsonValue.Create(field = value); }
	public ParameterSchema(ParameterInfo[] parameters)
	{

		Type = "object";
		var properties = new Dictionary<string, JsonObject>();
		var required = new List<string>();
		foreach (var param in parameters)
		{
			var json = JsonNode.Parse(JSchemaGenerator.Generate(param.ParameterType).ToString()).AsObject();
			json["description"] = param.GetCustomAttribute<DescriptionAttribute>()?.Description;
			properties[param.Name!] = json;
			if (!param.HasDefaultValue)
			{
				required.Add(param.Name!);
			}
		}
		Properties = properties; 
		Required = required; 
	} 
}
