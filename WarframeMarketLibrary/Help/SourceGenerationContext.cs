
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WarframeMarketLibrary.Model.Item;
namespace WarframeMarketLibrary.Help;

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,  // 属性名转为蛇形命名（如：userName → user_name）
	UseStringEnumConverter = true          // 枚举值转为字符串（但特性无法指定蛇形命名，需在实例覆盖）
)]
[JsonSerializable(typeof(Item))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
	[field: AllowNull]
	public static JsonSerializerOptions V2 => field ??=
		new JsonSerializerOptions(Default.Options) // 必须显式继承
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 完全禁用转义（如/不转义为\/） 
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) }, // 覆盖特性，强制枚举蛇形命名（如FirstValue → first_value） 
			WriteIndented = true,                // 格式化输出（带缩进和换行）
			TypeInfoResolver = JsonTypeInfoResolver.Combine(
				Default.Options.TypeInfoResolver,  // 优先使用源生成
				new DefaultJsonTypeInfoResolver()// 回退到反射 	
			)
		};

	[field: AllowNull]
	public static JsonSerializerOptions V1 => field ??=
		new JsonSerializerOptions(V2) // 必须显式继承
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
		};
}