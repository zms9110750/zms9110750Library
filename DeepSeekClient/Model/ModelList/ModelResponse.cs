using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.ModelList;
/// <summary>
/// 模型列表
/// </summary>
/// <param name="Object">其值仅可能为[list]</param>
/// <param name="Data">模型列表</param>
public record ModelResponse(
	[property: JsonPropertyName("object"), JsonProperty("object")] string Object,
	[property: JsonPropertyName("data"), JsonProperty("data")] List<Model> Data);

