// 值对象核心接口
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.JsonConverter;

public interface ICustomizeJsonConverter<T>
{
	public static abstract bool CanConvertJson { get; }
	public static abstract void WriteNewtonsoftJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer);
	public static abstract object ReadNewtonsoftJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer);
	public static abstract T ReadTextJson(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options );
	public static abstract void WriteTextJson(Utf8JsonWriter writer, T value, JsonSerializerOptions options );
}
