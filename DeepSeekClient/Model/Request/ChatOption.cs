using DeepSeekClient.Model.Message;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using zms9110750.DeepSeekClient.Json;
using zms9110750.DeepSeekClient.Model.ModelList;
using zms9110750.DeepSeekClient.Model.Tool;

namespace zms9110750.DeepSeekClient.Model.Request;
/// <summary>
/// 聊天配置
/// </summary>
public class ChatOption
{
	/// <summary>
	/// 消息列表
	/// </summary>
	[JsonConverter(typeof(MessageEnumerableConverter))][field: AllowNull] public IEnumerable<Message> Messages { get => field ?? Enumerable.Empty<Message>(); set; }

	/// <summary>
	/// 聊天模型
	/// </summary>
	/// <remarks>通过<see cref="SetModel(ChatModel)"/>设置</remarks>
	public string Model { get; protected set; }

	/// <summary>
	/// 如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
	/// </summary>
	/// <remarks>介于 -2.0 和 2.0 之间的数字。</remarks>
	public double? FrequencyPenalty { get; set => field = value is >= -2 and <= 2 or null ? value : throw new ArgumentOutOfRangeException(nameof(value), "Frequency penalty must be between -2 and 2."); }

	/// <summary>
	/// 限制一次请求中模型生成 completion 的最大 token 数。输入 token 和输出 token 的总长度受模型的上下文长度的限制。
	/// </summary>
	/// <remarks>介于 1 到 8192 间的整数，如未指定 max_tokens参数，默认使用 4096。</remarks>
	public int? MaxTokens { get; set => field = value is >= 1 and <= 8192 or null ? value : throw new ArgumentOutOfRangeException(nameof(value), "Max tokens must be between 1 and 8192."); }

	/// <summary>
	/// 如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性
	/// </summary>
	/// <remarks>介于 -2.0 和 2.0 之间的数字。</remarks>
	public double? PresencePenalty { get; set => field = value is >= -2 and <= 2 or null ? value : throw new ArgumentOutOfRangeException(nameof(value), "Presence penalty must be between -2 and 2."); }

	/// <summary>
	/// 一个 object，指定模型必须输出的格式。 
	/// 设置为 { "type": "json_object" } 以启用 JSON 模式，该模式保证模型生成的消息是有效的 JSON。
	/// </summary>
	/// <remarks>注意: 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。<br/>
	/// 否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。<br/>
	/// 此外，如果 finish_reason="length"，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。</remarks>
	public ResponseFormatWrap? ResponseFormat { get; set; }

	/// <summary>
	/// 一个 string 或最多包含 16 个 string 的 list，在遇到这些词时，API 将停止生成更多的 token。
	/// </summary>
	public StringValues? Srop { get; set; }

	/// <summary>
	/// 如果设置为 True，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
	/// </summary>
	public bool? Stream { get; set => StreamOptions = (field = value) == true ? StreamOptions.EnableIncludeUsage : null; }

	/// <summary>
	/// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
	/// </summary>
	/// <remarks>该选项随<see cref="Stream"/>自动设置。不可手动设置。</remarks>
	public StreamOptions? StreamOptions { get; protected set; }

	/// <summary>
	/// 采样温度。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。<br/>
	/// 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
	/// </summary>
	/// <remarks>介于 0 和 2 之间</remarks>
	public double? Temperature { get; set => field = value is >= 0 and <= 2 or null ? value : throw new ArgumentOutOfRangeException(nameof(value), "Temperature must be between 0.1 and 2."); }

	/// <summary>
	/// 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。。<br/>
	///所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。 。<br/>
	///我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改。
	/// </summary>
	/// <remarks>介于 0 和 1 之间</remarks>
	public double? TopP { get; set => field = value is >= 0 and <= 1 or null ? value : throw new ArgumentOutOfRangeException(nameof(value), "Top K must be between 0 and 1."); }

	/// <summary>
	/// 工具集
	/// </summary>
	/// <remarks>使用<see cref="ToolKit.Add(Tool.Tool)"/>方法传入委托即可绑定。但初始是null，必须先创建一个</remarks>
	[JsonConverter(typeof(ToolKitConverter))] public ToolKit? Tools { get; set; }

	/// <summary>
	/// 工具选择。因为是类型联合，此项不能手动修改。
	/// </summary>
	/// <remarks>若<see cref="ChatCompletionNamedToolChoice"/>有值，则使用其值<br/>
	/// 否则使用<see cref="ChatCompletionToolChoice"/>的值。</remarks>
	public JsonNode? ToolChoice => ChatCompletionNamedToolChoice != null
				? JsonSerializer.SerializeToNode(ChatCompletionNamedToolChoice, SourceGenerationContext.Default.Options)
				: ChatCompletionToolChoice != null
				? JsonValue.Create(ChatCompletionToolChoice) : (JsonNode?)null;

	/// <summary>
	/// 工具选择。默认为<see cref="ChatCompletionToolChoice.Auto"/>。
	/// </summary>
	[JsonIgnore] public ChatCompletionToolChoice? ChatCompletionToolChoice { get; set; }

	/// <summary>
	/// 工具选择。
	/// </summary>
	[JsonIgnore] public ToolChoice? ChatCompletionNamedToolChoice { get; set; }

	/// <summary>
	/// 是否返回所输出 token 的对数概率。如果为 true，则在 message 的 content 中返回每个输出 token 的对数概率。
	/// </summary>
	/// <remarks>此项不允许手动修改。当<see cref="TopLogprobs"/>不为null时，此项自动设置为true。<br/>
	/// 此外，不允许和<see cref="Model"/>为R1时同时使用，若如此，此项自动设置为false。</remarks>
	public bool? Logprobs => TopLogprobs == null || Model == ChatModel.R1.Id ? null : true;

	/// <summary>
	/// 指定每个输出位置返回输出概率 top N 的 token，且返回这些 token 的对数概率。指定此参数时，logprobs 必须为 true。
	/// </summary>
	/// <remarks>介于 0 和 20 之间，默认为 0。</remarks>
	public int? TopLogprobs { get => Model == ChatModel.R1.Id ? null : field; set => field = value is >= 0 and <= 20 or null ? value : throw new ArgumentOutOfRangeException(nameof(value), "Top logprobs must be between 0 and 20."); }
	
	/// <summary>
	/// 构造函数
	/// </summary>
	/// <remarks>默认使用 V3 模型。</remarks>
	public ChatOption()
	{
		SetModel(ChatModel.V3);
	}
	
	/// <summary>
	/// 设置模型
	/// </summary>
	/// <param name="model">模型列表中的模型</param>

	[MemberNotNull(nameof(Model))]
	public void SetModel(ChatModel model)
	{
		Model = model.Id;
	}

	/// <summary>
	/// 设置预设的采样温度
	/// </summary>
	/// <param name="capabilityType">使用情景</param>
	public void SetTemperature(ModelCapabilityType capabilityType)
	{
		Temperature = (int)capabilityType / 1000.0;
	}
}
