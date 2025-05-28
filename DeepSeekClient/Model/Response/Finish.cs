
using DeepSeekClient.JsonConverter;

namespace DeepSeekClient.Model.Response;

[System.Text.Json.Serialization.JsonConverter(typeof(SystemTextJsonConverter<Finish>))]
[Newtonsoft.Json.JsonConverter(typeof(NewtonsoftConverter<Finish>))]
public record Finish(string Value) : IStringValueObject<Finish>
{
	/// <summary>
	/// 模型自然停止生成，或遇到 stop 序列中列出的字符串。
	/// </summary>
	public static Finish Stop => new Finish("stop");
	/// <summary>
	/// 输出长度达到了模型上下文长度限制，或达到了 max_tokens 的限制。
	/// </summary>
	public static Finish Length => new Finish("length");
	/// <summary>
	/// 输出内容因触发过滤策略而被过滤。
	/// </summary>
	public static Finish ContentFilter => new Finish("content_filter");
	/// <summary>
	/// 由于后端推理资源受限，请求被打断。
	/// </summary>
	public static Finish InsufficientSystemResource => new Finish("insufficient_system_resource");
	/// <summary>
	/// 调用工具
	/// </summary>
	public static Finish ToolCalls => new Finish("tool_calls"); 

	public static Finish Creat(string value)
	{
		return new Finish(value);
	}
}
