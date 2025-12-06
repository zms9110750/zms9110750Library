using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Users;

namespace WarframeMarketQuery.Model;

/// <summary>
/// V2 API响应模型
/// </summary>
/// <param name="ApiVersion">API版本</param>
/// <param name="Data">数据</param>
/// <param name="Error">错误</param>
public record Response<T>(
	string ApiVersion,
	T Data,
	string? Error);


/// <summary>
/// V2 API响应模型
/// </summary>
/// <param name="ApiVersion">API版本</param> 
/// <param name="Error">错误</param>
public abstract record Response(
	string ApiVersion,
	string? Error)
{
	public static JsonSerializerOptions V2options { get; } =
		new JsonSerializerOptions
		{
			// 使用源生成器的类型信息解析器
			TypeInfoResolver = JsonTypeInfoResolver.Combine(
				SourceGenerationContext.Default.Options.TypeInfoResolver,  // 优先使用源生成
				new DefaultJsonTypeInfoResolver()// 回退到反射 	
			), 
			// 自定义配置
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // 蛇形命名
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
			WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
	public static JsonSerializerOptions V1options { get; } = new JsonSerializerOptions(V2options)
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
	};
}


[JsonSerializable(typeof(Response<ItemSet>))]
[JsonSerializable(typeof(Response<User>))]
[JsonSerializable(typeof(Response<Orders.OrderTop>))]
[JsonSerializable(typeof(Response<Statistics.Statistic>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}