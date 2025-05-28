using DeepSeekClient.Model.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 

public class LogprobNewtonsoftConverter : Newtonsoft.Json.JsonConverter<Logprob[]>
{
	public override Logprob[] ReadJson(JsonReader reader, Type objectType, Logprob[] existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
	{
		// 加载整个JSON对象
		JObject jo = JObject.Load(reader); 

		// 组合成Logprob数组
		return jo["tokens"].Zip(jo["token_logprobs"], jo["top_logprobs"])
			.Select(x => new Logprob(
				x.First.Value<string>(),
				x.Second.Value<double>(),
				((JObject)x.Third).Properties()
				   .Select(p => new LogprobContent(p.Name, p.Value.Value<double>()))
				   .ToArray()
			)).ToArray();
	}

	public override void WriteJson(JsonWriter writer, Logprob[] value, Newtonsoft.Json.JsonSerializer serializer)
	{
		// 开始写入对象
		writer.WriteStartObject();

		// 写入tokens数组
		writer.WritePropertyName("tokens");
		writer.WriteStartArray();
		foreach (var item in value)
		{
			writer.WriteValue(item.Token);
		}
		writer.WriteEndArray();

		// 写入token_logprobs数组
		writer.WritePropertyName("token_logprobs");
		writer.WriteStartArray();
		foreach (var item in value)
		{
			writer.WriteValue(item.Logprob);
		}
		writer.WriteEndArray();

		// 写入top_logprobs数组
		writer.WritePropertyName("top_logprobs");
		writer.WriteStartArray();
		foreach (var item in value)
		{
			writer.WriteStartObject();
			foreach (var top in item.TopLogprobs)
			{
				writer.WritePropertyName(top.Token);
				writer.WriteValue(top.Logprob);
			}
			writer.WriteEndObject();
		}
		writer.WriteEndArray();

		// 结束对象
		writer.WriteEndObject();
	}
}
