
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using zms9110750.DeepSeekClient.Model.Balance;
using zms9110750.DeepSeekClient.Model.ModelList;
using zms9110750.DeepSeekClient.Model.Request;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Tool;
using zms9110750.DeepSeekClient.ModelDelta.Response;

namespace zms9110750.DeepSeekClient;
/// <summary>
/// DeepSeek API 访问
/// </summary>
/// <remarks>重要API：<list type="bullet">
/// <item><see cref="ChatAsync"/></item>
/// <item><see cref="ChatStreamAsync"/></item>
/// <item><see cref="Option"/></item>
/// </list> </remarks>
public class DeepSeekApiClient
{
	/// <summary>
	/// 基本对话的API地址
	/// </summary>
	public const string ChatServerUrl = "https://api.deepseek.com/chat/completions";

	/// <summary>
	/// 网络访问连接器
	/// </summary>
	protected HttpClient Http { get; }

	/// <summary>
	/// 伴随请求一起发送的设置项
	/// </summary>
	public ChatOption Option { get; set => field = value ?? throw new ArgumentNullException(nameof(value)); } = new ChatOption();

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
	/// 基本对话
	/// </summary> 
	/// <remarks>这个方法会把<see cref="Option"/>的<see cref="ChatOption.Stream"/>设置为null</remarks>
	public async Task<ChatResponse<Choice>> ChatAsync(CancellationToken token = default)
	{
		Option.Stream = null;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions);
		using var response = await SendAsync(ChatServerUrl, node!, token);
		return (await response.Content.ReadFromJsonAsync<ChatResponse<Choice>>(SourceGenerationContext.NetworkOptions, token))!;
	}

	/// <summary>
	/// 基本对话流式响应
	/// </summary>
	/// <remarks>这个方法会把<see cref="Option"/>的<see cref="ChatOption.Stream"/>设置为true</remarks>
	public async Task<ChatResponseDelta<ChoiceDelta>> ChatStreamAsync(CancellationToken token = default)
	{
		Option.Stream = true;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions);
		var response = await SendAsync(ChatServerUrl, node!, token);
		return new ChatResponseDelta<ChoiceDelta>(await response.Content.ReadAsStreamAsync(token), token, response);
	}
	/// <summary>
	/// 获取模型列表
	/// </summary> 
	public async Task<ModelResponse> GetListModelsAsync(CancellationToken token = default)
	{
		return (await (await SendAsync("https://api.deepseek.com/models", HttpMethod.Get, token)).Content.ReadFromJsonAsync<ModelResponse>(SourceGenerationContext.NetworkOptions, token))!;
	}
	/// <summary>
	/// 获取用户余额
	/// </summary> 
	public async Task<UserResponse> GetUserBalanceAsync(CancellationToken token = default)
	{
		return (await (await SendAsync("https://api.deepseek.com/user/balance", HttpMethod.Get, token)).Content.ReadFromJsonAsync<UserResponse>(SourceGenerationContext.NetworkOptions, token))!;
	}
	/// <summary>
	/// 发送请求
	/// </summary>  
	/// <remarks>默认<see cref="RetryPolicy"/>在遇到<see cref="HttpStatusCode.TooManyRequests"/>时会自动重试。</remarks>
	/// <exception cref="HttpRequestException"></exception>
	protected async Task<HttpResponseMessage> SendAsync(string url, JsonNode content, CancellationToken token = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
		{
			Content = new StringContent(content!.ToJsonString(SourceGenerationContext.NetworkOptions), Encoding.UTF8, "application/json"),
		};
		var response = await Http.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
		return !response.IsSuccessStatusCode
			? throw new HttpRequestException(await response.Content.ReadAsStringAsync(token) + "\nRequestMessage:" + await response.RequestMessage?.Content?.ReadAsStringAsync(token)!, null, response.StatusCode)
			: response;
	}
	/// <summary>
	/// 发送请求
	/// </summary>  
	/// <remarks>默认<see cref="RetryPolicy"/>在遇到<see cref="HttpStatusCode.TooManyRequests"/>时会自动重试。</remarks>
	/// <exception cref="HttpRequestException"></exception>
	protected async Task<HttpResponseMessage> SendAsync(string url, HttpMethod httpMethod, CancellationToken token = default)
	{
		var response = await Http.SendAsync(new HttpRequestMessage(httpMethod, url), HttpCompletionOption.ResponseHeadersRead, token);
		return !response.IsSuccessStatusCode
		? throw new HttpRequestException(await response.Content.ReadAsStringAsync(token) + "\nRequestMessage:" + await response.RequestMessage?.Content?.ReadAsStringAsync()!, null, response.StatusCode)
		: response;
	}
}
