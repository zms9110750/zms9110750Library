using DeepSeekClient.Model.Balance;
using DeepSeekClient.Model.Message;
using DeepSeekClient.Model.ModelList;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Beta;
using zms9110750.DeepSeekClient.Model.Request;
using zms9110750.DeepSeekClient.Model.Response;

namespace zms9110750.DeepSeekClient.Model.Tool;

[JsonSourceGenerationOptions(WriteIndented = true,
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	UseStringEnumConverter = true,
	NumberHandling = JsonNumberHandling.AllowReadingFromString,
	AllowOutOfOrderMetadataProperties = true,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	)]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(ModelResponse))]
[JsonSerializable(typeof(ToolCall))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(ChatResponse))]
[JsonSerializable(typeof(ChatResponseFIM))]
[JsonSerializable(typeof(ChatOption))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
	[field: AllowNull]
	public static JsonSerializerOptions UnsafeRelaxed => field ??= new JsonSerializerOptions(Default!.Options)
	{
		WriteIndented = false,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};
}
