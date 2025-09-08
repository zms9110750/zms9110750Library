using zms9110750.DeepSeekClient.Model.Chat.Tool;

namespace zms9110750.DeepSeekClient.Json;

internal class MessageAssistantToolCallArrayConverter : JsonConverter<List<IToolCall>>
{
	public override List<IToolCall>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using JsonDocument doc = JsonDocument.ParseValue(ref reader);
		JsonElement root = doc.RootElement;
		List<IToolCall> result = new List<IToolCall>(root.GetArrayLength());
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
			result.Add(obj == null ? element.Deserialize<IToolCall>(options)! : obj.Deserialize<IToolCall>(options)!);
		}
		return result;
	}

	public override void Write(Utf8JsonWriter writer, List<IToolCall> value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, options);
	}
}