using DeepSeekClient.Model.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeepSeekClient.JsonConverter;

public class LogprobChatNewtonsoftConverter : JsonConverter<Logprob[]>
{
	public override Logprob[] ReadJson(JsonReader reader, Type objectType, Logprob[] existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		// 1. 加载JSON对象
		JObject jo = JObject.Load(reader);

		// 2. 获取content数组
		if (jo["content"] is not JArray contentArray)
			return [];

		// 3. 转换为Logprob数组
		return [.. contentArray.Select(item => new Logprob(
			 item["token"]?.Value<string>() ?? string.Empty,
			 item["logprob"]?.Value<double>() ?? 0.0,
			 item["top_logprobs"]?
				.Select(topItem => new LogprobContent(
				  topItem["token"]?.Value<string>() ?? string.Empty,
					  topItem["logprob"]?.Value<double>() ?? 0.0
				))
				.ToArray() ?? []
		))];
	}

	public override void WriteJson(JsonWriter writer, Logprob[] value, JsonSerializer serializer)
	{
		// 开始写入对象 
		writer.WriteStartObject();
		writer.WritePropertyName("content");

		// 开始写入content数组
		writer.WriteStartArray();

		foreach (var logprob in value)
		{
			writer.WriteStartObject();

			// 写入基础字段
			writer.WritePropertyName("token");
			writer.WriteValue(logprob.Token);

			writer.WritePropertyName("logprob");
			writer.WriteValue(logprob.Logprob);

			// 写入top_logprobs数组
			writer.WritePropertyName("top_logprobs");
			writer.WriteStartArray();

			foreach (var topLogprob in logprob.TopLogprobs)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("token");
				writer.WriteValue(topLogprob.Token);
				writer.WritePropertyName("logprob");
				writer.WriteValue(topLogprob.Logprob);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		writer.WriteEndArray();
		writer.WriteEndObject();
	}
} 
