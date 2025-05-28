// 值对象核心接口
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.JsonConverter;
public interface IStringValueObject<T> : ICustomizeJsonConverter<T> where T : IStringValueObject<T>
{
	static readonly JsonConverter<T> Converter = (JsonConverter<T>)JsonSerializerOptions.Default.GetConverter(typeof(T));

	string Value { get; }
	abstract static T Creat(string value);
	static bool ICustomizeJsonConverter<T>.CanConvertJson => true;
	static void ICustomizeJsonConverter<T>.WriteNewtonsoftJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
	{
		writer.WriteValue(((T)value).Value); // Newtonsoft默认不转义中文
	}
	static object ICustomizeJsonConverter<T>.ReadNewtonsoftJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
	{
		switch (reader.TokenType)
		{
			case Newtonsoft.Json.JsonToken.String:
				return T.Creat(reader.Value?.ToString() ?? string.Empty);
			default:
				throw new Newtonsoft.Json.JsonSerializationException($"预期字符串类型");
		}
	}
	static T ICustomizeJsonConverter<T>.ReadTextJson(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return T.Creat(reader.GetString());
			default:
				throw new JsonException($"预期字符串类型");
		}
	}
	static void ICustomizeJsonConverter<T>.WriteTextJson(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(JsonEncodedText.Encode(value.Value)); // 依赖工厂配置的编码器，无需额外操作 
	}
}