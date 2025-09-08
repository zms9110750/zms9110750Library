namespace zms9110750.DeepSeekClient.Model.Chat.Response;

/// <summary>
/// 聊天响应接口
/// </summary>
public interface IChatResponse<out TChoice>
{
	/// <inheritdoc cref="ChatResponse{TChoice}.Id"/>
	string Id { get; }
	/// <inheritdoc cref="ChatResponse{TChoice}.Object"/>
	string Object { get; }
	/// <inheritdoc cref="ChatResponse{TChoice}.Created"/>
	long Created { get; }
	/// <inheritdoc cref="ChatResponse{TChoice}.Model"/>
	string Model { get; }
	/// <inheritdoc cref="ChatResponse{TChoice}.SystemFingerint"/>
	string SystemFingerint { get; }
	/// <summary>
	/// 获取该响应的首项。
	/// </summary>
	/// <remarks>理应只有唯一一项</remarks>
	TChoice ChoiceFirst => Choices.First();
	/// <inheritdoc cref="ChatResponse{TChoice}.Choices"/>
	IEnumerable<TChoice> Choices { get; }
	/// <inheritdoc cref="ChatResponse{TChoice}.Usage"/>
	Usage Usage { get; }
}


/// <summary>
/// 聊天响应
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Object">对象的类型, 其值为 chat.completion。</param>
/// <param name="Created">创建聊天完成时的 Unix 时间戳（以秒为单位）。</param>
/// <param name="Model">生成该 completion 的模型名。</param>
/// <param name="SystemFingerint">模型运行时的后端配置的指纹。</param>
/// <param name="Choices">模型生成的补全内容的选择列表。但是永远只有一个元素。</param>
/// <param name="Usage">该对话补全请求的用量信息。</param>
public record ChatResponse<TChoice>(
	 string Id,
	 string Object,
	 long Created,
	 string Model,
	 string SystemFingerint,
	 TChoice[] Choices,
	 Usage Usage) : IChatResponse<TChoice>
{
	IEnumerable<TChoice> IChatResponse<TChoice>.Choices => Choices;
}

/// <summary>
/// 表示API调用的token使用情况统计
/// </summary>
/// <param name="CompletionTokens">完成部分消耗的token数量</param>
/// <param name="PromptTokens">提示部分消耗的token数量</param>
/// <param name="PromptCacheHitTokens">提示缓存命中的token数量</param>
/// <param name="PromptCacheMissTokens">提示缓存未命中的token数量</param>
/// <param name="TotalTokens">总消耗token数量</param>
/// <param name="CompletionTokensDetails">完成部分token的详细使用情况</param>
/// <param name="PromptTokensDetails">提示部分token的详细使用情况</param>
public record Usage(
	int CompletionTokens,
	int PromptTokens,
	int PromptCacheHitTokens,
	int PromptCacheMissTokens,
	int TotalTokens,
	UsageCompletionTokensDetail CompletionTokensDetails,
	UsagePromptTokensDetail PromptTokensDetails);
/// <summary>
/// 完成部分token的详细使用情况
/// </summary>
/// <param name="ReasoningTokens">用于思维链推理的token数量</param>
public record UsageCompletionTokensDetail(int ReasoningTokens);

/// <summary>
/// 提示部分token的详细使用情况
/// </summary>
/// <param name="CachedTokens">从缓存中读取的token数量</param>
public record UsagePromptTokensDetail(int CachedTokens);