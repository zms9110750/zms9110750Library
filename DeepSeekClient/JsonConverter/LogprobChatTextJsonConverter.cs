using DeepSeekClient.Model.Response;
using System.Text.Json;

namespace DeepSeekClient.JsonConverter;

public class LogprobChatTextJsonConverter : System.Text.Json.Serialization.JsonConverter<Logprob[]>
{
	public override Logprob[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using JsonDocument doc = JsonDocument.ParseValue(ref reader);
		JsonElement root = doc.RootElement;

		// 获取content数组
		if (!root.TryGetProperty("content", out JsonElement contentElement) ||
			contentElement.ValueKind != JsonValueKind.Array)
		{
			return Array.Empty<Logprob>();
		}

		// 转换为Logprob数组
		var result = new Logprob[contentElement.GetArrayLength()];
		int i = 0;

		foreach (var item in contentElement.EnumerateArray())
		{
			// 处理top_logprobs
			var topLogprobs = Array.Empty<LogprobContent>();
			if (item.TryGetProperty("top_logprobs", out JsonElement topElement) &&
				topElement.ValueKind == JsonValueKind.Array)
			{
				topLogprobs = new LogprobContent[topElement.GetArrayLength()];
				int j = 0;

				foreach (var topItem in topElement.EnumerateArray())
				{
					topLogprobs[j++] = new LogprobContent(
						topItem.GetProperty("token").GetString() ?? string.Empty,
						topItem.GetProperty("logprob").GetDouble()
					);
				}
			}

			result[i++] = new Logprob(
				item.GetProperty("token").GetString() ?? string.Empty,
				item.GetProperty("logprob").GetDouble(),
				topLogprobs
			);
		}

		return result;
	}

	public override void Write(Utf8JsonWriter writer, Logprob[] value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("content");

		writer.WriteStartArray();

		foreach (var logprob in value)
		{
			writer.WriteStartObject();

			writer.WriteString("token", logprob.Token);
			writer.WriteNumber("logprob", logprob.Logprob);

			writer.WritePropertyName("top_logprobs");
			writer.WriteStartArray();

			foreach (var topLogprob in logprob.TopLogprobs)
			{
				writer.WriteStartObject();
				writer.WriteString("token", topLogprob.Token);
				writer.WriteNumber("logprob", topLogprob.Logprob);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}