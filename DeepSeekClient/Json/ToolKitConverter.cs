using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.Json;

internal class ToolKitConverter : JsonConverter<ToolKit>
{
	public override ToolKit? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{ 
		return JsonSerializer.Deserialize<ToolKit>(ref reader, options);
	}

	public override void Write(Utf8JsonWriter writer, ToolKit value, JsonSerializerOptions options)
	{
		// 检查集合是否为空
		if (value == null || value.Count == 0)
		{
			writer.WriteNullValue();
		}
		else
		{
			// 使用源生成上下文序列化
			JsonSerializer.Serialize(writer, value, SourceGenerationContext.Default.ToolKit);
		}
	}
}