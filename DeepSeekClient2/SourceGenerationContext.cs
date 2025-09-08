 
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata; 
namespace zms9110750.DeepSeekClient2.Model.Tool;

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,  // 属性名转为蛇形命名（如：userName → user_name）
	UseStringEnumConverter = true,              // 枚举值转为字符串（但特性无法指定蛇形命名，需在实例覆盖）
	AllowOutOfOrderMetadataProperties = true,   // 允许元属性乱序（如JSON字段顺序和类定义不一致）
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  // 自动忽略null值
)]
[JsonSerializable(typeof(int))]


internal partial class SourceGenerationContext : JsonSerializerContext
{
	// 网络传输配置（蛇形命名 + 无转义 + 压缩）
	 
	public static JsonSerializerOptions NetworkOptions => field ??=
		new JsonSerializerOptions(Default.Options) // 必须显式继承
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 完全禁用转义（如/不转义为\/）
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) } // 覆盖特性，强制枚举蛇形命名（如FirstValue → first_value） 
		};

	// 程序内部配置（原名 + 无转义 + 压缩）
	 
	public static JsonSerializerOptions InternalOptions => field ??=
		new JsonSerializerOptions(Default.Options)
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 完全禁用转义
			PropertyNamingPolicy = null,         // 禁用蛇形命名，保持原始属性名（如userName）
		};

	// 调试配置（蛇形命名 + 格式化 + 动态源选择）
	 
	public static JsonSerializerOptions DebugOptions => field ??=
		new JsonSerializerOptions(Default.Options)
		{
			WriteIndented = true,                // 格式化输出（带缩进和换行）
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 完全禁用转义 
			TypeInfoResolver = JsonTypeInfoResolver.Combine(
			Default.Options.TypeInfoResolver,  // 优先使用源生成
			new DefaultJsonTypeInfoResolver()) // 回退到反射 			 
		};
}
/// <summary>
/// 公开的序列化配置
/// </summary>
public static class PublicSourceGenerationContext
{
	/// <summary>
	/// 网络传输配置（蛇形命名 + 无转义 + 压缩）
	/// </summary>
	public static JsonSerializerOptions NetworkOptions { get; } = SourceGenerationContext.NetworkOptions;

	/// <summary>
	/// 程序内部配置（原名 + 无转义 + 压缩）
	/// </summary>
	public static JsonSerializerOptions InternalOptions { get; } = SourceGenerationContext.InternalOptions;

	/// <summary>
	/// 调试配置（蛇形命名 + 格式化 + 动态源选择）
	/// </summary>
	public static JsonSerializerOptions DebugOptions { get; } = SourceGenerationContext.DebugOptions;
}