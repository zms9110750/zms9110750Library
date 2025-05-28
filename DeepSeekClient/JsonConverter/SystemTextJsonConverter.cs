// 值对象核心接口
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.JsonConverter;

public class SystemTextJsonConverter<T> : JsonConverter<T> where T : ICustomizeJsonConverter<T>
{
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return T.ReadTextJson(ref reader, typeToConvert, options);
	}
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		T.WriteTextJson(writer, value, options);
	}
}
