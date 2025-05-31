using DeepSeekClient.Model.Balance;
using DeepSeekClient.Model.ModelList;
using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.RateLimiting;
using zms9110750.DeepSeekClient.Model.Request;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Response.Delta;
using zms9110750.DeepSeekClient.Model.Tool;

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
	protected const string ChatServerUrl = "https://api.deepseek.com/chat/completions";

	/// <summary>
	/// 限流器
	/// </summary>
	protected TokenBucketRateLimiter Limiter { get; } = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
	{
		TokenLimit = 50,
		TokensPerPeriod = 8,
		ReplenishmentPeriod = TimeSpan.FromSeconds(1),
		QueueLimit = 100
	});

	/// <summary>
	/// 重试器
	/// </summary>
	/// <remarks>仅针对<see cref="HttpStatusCode.TooManyRequests"/>重试5次</remarks>
	protected AsyncRetryPolicy<HttpResponseMessage> RetryPolicy { get; } = Policy
		.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
		.WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: (retryAttempt) => TimeSpan.FromSeconds(retryAttempt));

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
	/// <param name="apiKey"></param>
	/// <param name="client"></param>
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
	public async Task<ChatResponse> ChatAsync(CancellationToken token = default)
	{
		Option.Stream = null;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.Default.ChatOption);
		using var response = await SendAsync(ChatServerUrl, node!, token);
		return (await response.Content.ReadFromJsonAsync(SourceGenerationContext.Default.ChatResponse, token))!;
	}

	/// <summary>
	/// 基本对话流式响应
	/// </summary>
	/// <remarks>这个方法会把<see cref="Option"/>的<see cref="ChatOption.Stream"/>设置为true</remarks>
	public async Task<ChatResponseDelta> ChatStreamAsync(CancellationToken token = default)
	{
		await Limiter.AcquireAsync(1, token);
		Option.Stream = true;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.Default.ChatOption);
		var response = await SendAsync(ChatServerUrl, node!, token);
		return new ChatResponseDelta(await response.Content.ReadAsStreamAsync(token), token, response);
	}
	/// <summary>
	/// 获取模型列表
	/// </summary> 
	public async Task<ModelResponse> GetListModelsAsync(string modelName, CancellationToken token = default)
	{
		return (await (await SendAsync("https://api.deepseek.com/models", HttpMethod.Get, token)).Content.ReadFromJsonAsync(SourceGenerationContext.Default.ModelResponse, token))!;
	}
	/// <summary>
	/// 获取用户余额
	/// </summary> 
	public async Task<UserResponse> GetUserBalanceAsync(string modelName, CancellationToken token = default)
	{
		return( await (await SendAsync("https://api.deepseek.com/user/balance", HttpMethod.Get, token)).Content.ReadFromJsonAsync(SourceGenerationContext.Default.UserResponse, token))!;
	}
	/// <summary>
	/// 发送请求
	/// </summary>  
	/// <remarks>在遇到<see cref="HttpStatusCode.TooManyRequests"/>时会自动重试。</remarks>
	/// <exception cref="HttpRequestException"></exception>
	protected async Task<HttpResponseMessage> SendAsync(string url, JsonNode content, CancellationToken token = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, ChatServerUrl)
		{
			Content = new StringContent(content!.ToJsonString(SourceGenerationContext.UnsafeRelaxed), Encoding.UTF8, "application/json"),
		};
		return await RetryPolicy.ExecuteAsync(async (token) =>
				{
					await Limiter.AcquireAsync(1, token);
					HttpResponseMessage? response = await Http.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
					return response is { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.TooManyRequests }
						? throw new HttpRequestException(await response.Content.ReadAsStringAsync(token), null, response.StatusCode)
						: response;
				}, token);
	}
	/// <summary>
	/// 发送请求
	/// </summary>  
	/// <remarks>在遇到<see cref="HttpStatusCode.TooManyRequests"/>时会自动重试。</remarks>
	/// <exception cref="HttpRequestException"></exception>
	protected async Task<HttpResponseMessage> SendAsync(string url, HttpMethod httpMethod, CancellationToken token = default)
	{
		return await RetryPolicy.ExecuteAsync(async (token) =>
		{
			await Limiter.AcquireAsync(1, token);
			HttpResponseMessage? response = await Http.SendAsync(new HttpRequestMessage(httpMethod, url), HttpCompletionOption.ResponseHeadersRead, token);
			return response is { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.TooManyRequests }
				? throw new HttpRequestException(await response.Content.ReadAsStringAsync(token), null, response.StatusCode)
				: response;
		}, token);
	}
}
