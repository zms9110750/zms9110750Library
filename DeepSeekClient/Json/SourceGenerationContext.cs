using System.Text.Encodings.Web;
using System.Text.Json.Serialization.Metadata;
using zms9110750.DeepSeekClient.Model;
using zms9110750.DeepSeekClient.Model.Chat.Request;
using zms9110750.DeepSeekClient.Model.Chat.Response;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;

namespace zms9110750.DeepSeekClient.Json;

[JsonSerializable(typeof(BalanceResponse))]
[JsonSerializable(typeof(ChatModelResponse))]
[JsonSerializable(typeof(IChatRequest))]
[JsonSerializable(typeof(IFIMRequest))]
[JsonSerializable(typeof(ChatResponse<ChatChoice>))]
[JsonSerializable(typeof(ChatResponse<ChatDelta>))]
[JsonSerializable(typeof(ChatResponse<FIMChoice>))]
internal partial class SourceGenerationContext : JsonSerializerContext;

/// <summary>
/// 公开Json序列化配置
/// </summary>
public static class PublicSourceGenerationContext
{

	private static JsonSerializerOptions Default { get; } =
		new JsonSerializerOptions()
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 完全禁用转义 
			TypeInfoResolver = JsonTypeInfoResolver.Combine(
				SourceGenerationContext.Default.Options.TypeInfoResolver,  // 优先使用源生成
				new DefaultJsonTypeInfoResolver()), // 回退到反射 		
			AllowOutOfOrderMetadataProperties = true,// 允许元属性乱序
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // 忽略null值属性
		};


	/// <summary>
	/// 程序内部配置（原名 + 无转义 + 压缩）
	/// </summary>
	public static JsonSerializerOptions InternalOptions { get; } =
		new JsonSerializerOptions(Default)
		{
			Converters = { new JsonStringEnumConverter() }
		};

	/// <summary>
	/// 网络传输配置（蛇形命名 + 无转义 + 压缩）
	/// </summary>
	public static JsonSerializerOptions NetworkOptions { get; } =
		new JsonSerializerOptions(Default) // 必须显式继承
		{
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },  // 覆盖特性，强制枚举蛇形命名（如FirstValue → first_value） 
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		}; 
}
