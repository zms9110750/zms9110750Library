using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

internal class MessageAssistantToolCallArrayConverter : JsonConverter<ToolCall[]>
{
	public override ToolCall[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using JsonDocument doc = JsonDocument.ParseValue(ref reader);
		JsonElement root = doc.RootElement;
		ToolCall[] result = new ToolCall[root.GetArrayLength()];
		int index = 0;
		foreach (var element in root.EnumerateArray())
		{
			JsonObject? obj = null;
			if (!element.TryGetProperty("type", out _))
			{
				foreach (JsonProperty property in element.EnumerateObject())
				{
					if (property.Value.ValueKind == JsonValueKind.Object)
					{
						obj = JsonObject.Create(element)!;
						obj["type"] = property.Name;
						break;
					}
				}
			}
			result[index++] = obj == null ? element.Deserialize<ToolCall>(options)! : obj.Deserialize<ToolCall>(options)!;
		}
		return result;
	}

	public override void Write(Utf8JsonWriter writer, ToolCall[] value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, options);
	}
}