using DeepSeekClient.Model.Response;
using System.Text.Json;

public class LogprobFIMJsonConverter : System.Text.Json.Serialization.JsonConverter<Logprob[]>
{
	public override Logprob[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using JsonDocument doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		return root.GetProperty("tokens").EnumerateArray().Zip(
				root.GetProperty("token_logprobs").EnumerateArray(),
				root.GetProperty("top_logprobs").EnumerateArray())
			.Select(x => new Logprob(x.First.GetString()!, x.Second.GetDouble(),
				x.Third.EnumerateObject().Select(y => new LogprobContent(y.Name, y.Value.GetDouble())).ToArray()))
			.ToArray();
	}

	public override void Write(Utf8JsonWriter writer, Logprob[] value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		// Write tokens array
		writer.WritePropertyName("tokens");
		writer.WriteStartArray();
		foreach (var item in value)
		{
			writer.WriteStringValue(item.Token);
		}
		writer.WriteEndArray();

		// Write token_logprobs array
		writer.WritePropertyName("token_logprobs");
		writer.WriteStartArray();
		foreach (var item in value)
		{
			writer.WriteNumberValue(item.Logprob);
		}
		writer.WriteEndArray();

		// Write top_logprobs array
		writer.WritePropertyName("top_logprobs");
		writer.WriteStartArray();
		foreach (var item in value)
		{
			writer.WriteStartObject();
			foreach (var top in item.TopLogprobs)
			{
				writer.WritePropertyName(top.Token);
				writer.WriteNumberValue(top.Logprob);
			}
			writer.WriteEndObject();
		}
		writer.WriteEndArray();

		writer.WriteEndObject();
	}
}
