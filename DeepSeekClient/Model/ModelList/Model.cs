using DeepSeekClient.JsonConverter;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.ModelList;

/// <summary>
/// 模型
/// </summary>
/// <param name="Id">模型的标识符</param>
/// <param name="Object">对象的类型，其值仅可能为[model]</param>
/// <param name="OwnedBy">拥有该模型的组织。</param>
public record Model(
	[property: JsonPropertyName("id"), JsonProperty("id")] string Id,
	[property: JsonPropertyName("object"), JsonProperty("object")] string Object,
	[property: JsonPropertyName("owned_by"), JsonProperty("owned_by")] string OwnedBy) : IStringValueObject<Model>
{
	public static Model V3 { get; } = new Model("deepseek-chat", "model", "deepseek");
	public static Model R1 { get; } = new Model("deepseek-reasoner", "model", "deepseek");
	public string Value => Id;

	public static Model Creat(string value)
	{
		return new Model(value, "model", "deepseek");
	}
}

