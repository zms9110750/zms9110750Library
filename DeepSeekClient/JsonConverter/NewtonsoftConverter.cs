// 值对象核心接口
namespace DeepSeekClient.JsonConverter;

public class NewtonsoftConverter<T> : Newtonsoft.Json.JsonConverter where T : ICustomizeJsonConverter<T>
{
	public override bool CanConvert(Type objectType) => T.CanConvertJson;

	public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
	{
		T.WriteNewtonsoftJson(writer, value, serializer);
	}

	public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
	{
		return T.ReadNewtonsoftJson(reader, objectType, existingValue, serializer);
	}
}
