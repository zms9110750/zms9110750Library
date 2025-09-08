using System.Numerics;
using System.Runtime.CompilerServices;
using zms9110750.DeepSeekClient.Model.Chat.Messages;
using zms9110750.DeepSeekClient.Model.Chat.Tool;

namespace zms9110750.DeepSeekClient.Model.Chat.Request;
/// <summary>
/// 聊天请求体的实现类
/// </summary>
public class ChatRequest : IChatRequest, IFIMRequest, IStreamOptions, IResponseFormatWrap
{
	/// <summary>
	/// 默认提供一个列表以便操作消息列表。
	/// </summary>
	public List<Message> MessagesDefault { get; } = [];

	/// <inheritdoc	/>
	public IEnumerable<IMessage> Messages { get => field ?? MessagesDefault; set; }

	/// <inheritdoc cref="IChatRequest.Tools"/>
	public ToolKit Tools { get; set; } = [];

	/// <summary>
	/// 忽略工具。不参与请求。
	/// </summary>
	public bool IgnoreTools { get; set; }

	/// <inheritdoc	/>
	public int? TopLogprobs { get; set => field = ThrowIfOutOfRange(value, 0, 20); }

	/// <inheritdoc cref="IChatRequestPartialModel.Model"/>
	public ChatModel Model { get; set; } = ChatModel.V3;

	/// <inheritdoc	/>
	public double? FrequencyPenalty { get; set => field = ThrowIfOutOfRange(value, -2, 2); }

	/// <inheritdoc	/>
	public double? PresencePenalty { get; set => field = ThrowIfOutOfRange(value, -2, 2); }

	/// <inheritdoc	/>
	public int? MaxTokens { get; set => field = ThrowIfOutOfRange(value, 1, 8192); }

	/// <inheritdoc	/>
	public IEnumerable<string>? Stop { get; set; }

	/// <inheritdoc	/>
	public bool? Stream { get; set; }

	/// <inheritdoc	/>
	public double? Temperature { get; set => field = ThrowIfOutOfRange(value, 0, 2); }

	/// <inheritdoc	/>
	public double? TopP { get; set => field = ThrowIfOutOfRange(value, 0, 1); }

	/// <inheritdoc	/>
	public ResponseFormat FormatType { get; set; }

	/// <inheritdoc	/>
	public string Prompt { get; set; } = "";

	/// <inheritdoc	/>
	public bool? Echo { get; set; }

	/// <inheritdoc	/>
	public string? Suffix { get; set; }

	/// <summary>
	/// 前缀补全开关。为true且最后一项消息是<see cref="MessageAssistant"/>时会作为补全消息。启用这个功能时，<see cref="Tools"/>会失效。
	/// </summary>
	/// <remarks>如果启用了<see cref="ChatModel.R1"/>且传递了<see cref="MessageAssistant.ReasoningContent"/>，那么<see cref="TopLogprobs"/>会失效。</remarks>
	public bool Prefix { get; set; }

	IEnumerable<IToolRequest>? IChatRequest.Tools => !IgnoreTools && !Prefix && Tools?.Count > 0 ? Tools : null;
	JsonNode? IChatRequest.ToolChoice => ((IChatRequest)this).Tools == null ? null
		: Tools.ToolCall != null ? JsonSerializer.SerializeToNode(Tools.ToolCall, PublicSourceGenerationContext.NetworkOptions)
		: Tools.ToolCallRequire ? JsonSerializer.SerializeToNode(ChatCompletionToolChoice.Required, PublicSourceGenerationContext.NetworkOptions)
		: null;

	int? IFIMRequest.Logprobs => TopLogprobs;
	bool? IChatRequest.Logprobs => TopLogprobs == null ? null : true;
	IResponseFormatWrap? IChatRequest.ResponseFormat => FormatType == default ? null : this;
	IStreamOptions? IChatRequestPartialStream.StreamOptions => Stream == true ? this : null;
	bool? IStreamOptions.IncludeUsage => true;
	string IChatRequestPartialModel.Model => Model.Id;
	IEnumerable<IMessage> IChatRequest.Messages => Prefix ? PrefixMessage() : Messages;

	bool? IFIMRequest.Echo => Echo == true && ((IFIMRequest)this).Suffix == null && ((IFIMRequest)this).Logprobs == null;

	private static T? ThrowIfOutOfRange<T>(T? value, T min, T max, [CallerMemberName] string? paramName = null) where T : struct, INumber<T>
	{
		return value < min || value > max
			? throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.")
			: value;
	}

	private IEnumerable<IMessage> PrefixMessage()
	{
		if (Messages == null)
			yield break;

		using var enumerator = Messages.GetEnumerator();

		if (!enumerator.MoveNext())
		{
			yield break;
		}

		IMessage lastMessage = enumerator.Current;

		while (enumerator.MoveNext())
		{
			yield return lastMessage;
			lastMessage = enumerator.Current;
		}

		// 处理最后一个消息
		if (lastMessage.Role == Role.Assistant)
		{
			lastMessage = lastMessage.WithPrefix();
		}
		yield return lastMessage;
	}
}
