using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.Json;

internal class MessageEnumerableConverter : JsonConverter<IEnumerable<Message>>
{
	public override IEnumerable<Message>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, IEnumerable<Message> value, JsonSerializerOptions options)
	{
		// 1. 创建JsonArray容器
		var jsonArray = new JsonArray();

		// 2. 遍历消息集合
		foreach (var message in value)
		{
			if (message == null)
			{
				continue;
			}
			// 3. 为每个消息创建JsonObject
			var messageObj = JsonSerializer.SerializeToNode(message, SourceGenerationContext.NetworkOptions)!.AsObject();
			messageObj.Remove("reasoning_content");
			jsonArray.Add(messageObj);
		}

		// 6. 写入最终JSON
		jsonArray.WriteTo(writer);
	}
}