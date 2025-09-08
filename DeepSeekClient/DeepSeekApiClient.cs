using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using zms9110750.DeepSeekClient.Model;
using zms9110750.DeepSeekClient.Model.Chat.Request;
using zms9110750.DeepSeekClient.Model.Chat.Response;
using zms9110750.DeepSeekClient.Model.Chat.Response.Delta;

namespace zms9110750.DeepSeekClient;
/// <summary>
/// DeepSeek API 访问
/// </summary>
/// <remarks>重要API：<list type="bullet">
/// <item><see cref="ChatAsync"/></item>
/// <item><see cref="OptionDefault"/></item>
/// </list> </remarks>
public class DeepSeekApiClient
{
	/// <summary>
	/// 基本对话的API地址
	/// </summary>
	public const string ChatServerUrl = "https://api.deepseek.com/chat/completions";
	/// <summary>
	/// Beta测试版API地址
	/// </summary>
	public const string ChatServerUrlBeta = "https://api.deepseek.com/beta/chat/completions";

	/// <summary>
	/// application/json
	/// </summary>
	protected static MediaTypeHeaderValue MediaHeader { get; } = new MediaTypeHeaderValue("application/json");


	/// <summary>
	/// 网络访问连接器
	/// </summary>
	protected HttpClient Http { get; }

	/// <summary>
	/// 后备默认请求体
	/// </summary>
	public ChatRequest OptionDefault { get; set => field = value ?? throw new ArgumentNullException(nameof(OptionDefault)); } = new ChatRequest();

	/// <summary>
	/// 伴随请求一起发送的设置项
	/// </summary> 
	public IChatRequest OptionChat { get => field ?? OptionDefault; set; }

	/// <summary>
	/// 伴随FIM请求一起发送的设置项
	/// </summary> 
	public IFIMRequest OptionFIM { get => field ?? OptionDefault; set; }

	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="apiKey">DeepSeek API Key</param>
	/// <param name="client">网络访问连接器</param>  
	public DeepSeekApiClient(string apiKey, HttpClient? client = null)
	{
		client ??= new HttpClient();
		Http = client;
		Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
	}

	/// <summary>
	/// 发送一个聊天请求
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	public async Task<IAsyncEnumerable<IChatResponse<IChatChoice>>> ChatAsync(CancellationToken token = default)
	{
		string url = OptionChat.Prefix ? ChatServerUrlBeta : ChatServerUrl;
		bool stream = OptionChat.Stream == true;
		HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
		{
			Content = new StringContent(JsonSerializer.Serialize(OptionChat, PublicSourceGenerationContext.NetworkOptions), MediaHeader)
		}.SetBrowserResponseStreamingEnabled(true);

		var response = await Http.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);

		return !response.IsSuccessStatusCode
			? throw new HttpRequestException($"{await response.Content.ReadAsStringAsync(token)}"
				, response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity ? new ArgumentException(await response.RequestMessage?.Content?.ReadAsStringAsync(token)!) : null
				, response.StatusCode)
			: stream
			? new ChatResponseDelta<ChatDelta>(await response.Content.ReadAsStreamAsync(token), token, response)
			: AsyncEnumerableEx.Return((await response.Content.ReadFromJsonAsync<ChatResponse<ChatChoice>>(PublicSourceGenerationContext.NetworkOptions, token))!);
	}

	/// <summary>
	/// 前后缀补全
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <remarks>这个API在Beta测试，之后可能更改。</remarks>
	[Obsolete("This API is in beta testing and may change in future releases. Use with caution.")]
	public async Task<IAsyncEnumerable<IChatResponse<FIMChoice>>> FIMAsync(CancellationToken token = default)
	{
		string url = "https://api.deepseek.com/beta/completions";

		bool stream = OptionFIM.Stream == true;
		HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
		{
			Content = new StringContent(JsonSerializer.Serialize(OptionFIM, PublicSourceGenerationContext.NetworkOptions), MediaHeader)
		}.SetBrowserResponseStreamingEnabled(true);

		var response = await Http.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);

		return !response.IsSuccessStatusCode
			? throw new HttpRequestException($"{await response.Content.ReadAsStringAsync(token)}"
				, response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity ? new ArgumentException(await response.RequestMessage?.Content?.ReadAsStringAsync(token)!) : null
				, response.StatusCode)
			: stream
			? new ChatResponseDelta<FIMChoice>(await response.Content.ReadAsStreamAsync(token), token, response)
			: AsyncEnumerableEx.Return((await response.Content.ReadFromJsonAsync<ChatResponse<FIMChoice>>(PublicSourceGenerationContext.NetworkOptions, token))!);
	}

	/// <summary>
	/// 获取模型列表
	/// </summary> 
	public Task<ChatModelResponse> GetListModelsAsync(CancellationToken token = default)
	{
		return Http.GetFromJsonAsync<ChatModelResponse>("https://api.deepseek.com/models", PublicSourceGenerationContext.NetworkOptions, token)!;
	}

	/// <summary>
	/// 获取用户余额
	/// </summary> 
	public Task<BalanceResponse> GetUserBalanceAsync(CancellationToken token = default)
	{
		return Http.GetFromJsonAsync<BalanceResponse>("https://api.deepseek.com/user/balance", PublicSourceGenerationContext.NetworkOptions, token)!;
	}
}
