
using System.Net.Http.Json;
using System.Text.Json;
using zms9110750.DeepSeekClient.Model.ModelList;
using zms9110750.DeepSeekClient.Model.Response;
using zms9110750.DeepSeekClient.Model.Tool;
using zms9110750.DeepSeekClient.ModelDelta.Response;

namespace zms9110750.DeepSeekClient.Beta;
/// <summary>
/// 包含测试版API的客户端。
/// </summary>
/// <param name="apiKey">DeepSeek的API</param>
/// <param name="client">网络连接器</param>
public class DeepSeekApiClientBeta(string apiKey, HttpClient? client = null)
	: DeepSeekApiClient(apiKey, client)
{
	/// <summary>
	/// 前缀补全的API地址。
	/// </summary>
	protected const string ChatServerUrlBeta = "https://api.deepseek.com/beta/chat/completions";
	/// <summary>
	/// 中间填充的API地址。
	/// </summary>
	protected const string ChatServerUrlBetaFIM = "https://api.deepseek.com/beta/completions";

	/// <summary>
	/// 对话前缀续写。使用对话前缀续写时，用户需确保 messages 列表里最后一条消息的 role 为 assistant，并设置最后一条消息的 prefix 参数为 True。
	/// </summary>
	/// <remarks><list type="bullet">
	/// <item>Tools存在会报错。这个方法会线程不安全的短暂拿走Tools</item>
	/// <item>Stream不生效</item>
	/// <item>R1不生效</item>
	/// <item>logprobs不能和R1同时存在。这个方法会设置模型为V3</item>
	/// </list></remarks>
	public async Task<ChatResponse<Choice>> ChatBetaAsync(CancellationToken token = default)
	{
		Option.SetModel(ChatModel.V3);
		var temp = Option.Tools;
		Option.Tools = null;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions)!.AsObject();
		Option.Tools = temp;
		using var response = await SendAsync(ChatServerUrlBeta, node, token);
		return (await response.Content.ReadFromJsonAsync<ChatResponse<Choice>>(SourceGenerationContext.NetworkOptions, token))!;
	}

	/// <summary>
	/// 中间填充。用户可以提供前缀和后缀（可选），模型来补全中间的内容。FIM 常用于内容续写、代码补全等场景。
	/// </summary>
	/// <param name="prompt">用于生成完成内容的提示</param>
	/// <param name="suffix">制定被补全内容的后缀</param> 
	/// <param name="token">取消标记</param>
	/// <remarks><list type="bullet">
	/// <item>设个方法会把Stream设置为false</item>
	/// <item>R1报错。这个方法会设置模型为V3</item>
	/// <item>Tools无效</item>
	/// <item>logprobs需要为int。这个方法会把top_logprobs复制给logprobs</item>
	/// </list></remarks>
	public async Task<ChatResponse<ChoiceFIM>> FIMAsync(string prompt, string? suffix = null, CancellationToken token = default)
	{
		Option.Stream = false;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions)!.AsObject();
		if (suffix != null)
		{
			node["suffix"] = suffix;
		}
		node["prompt"] = prompt;
		if ((bool?)node["logprobs"] == true)
		{
			node["logprobs"] = node["top_logprobs"]?.DeepClone();
		}
		using var response = await SendAsync(ChatServerUrlBetaFIM, node, token);
		return (await response.Content.ReadFromJsonAsync<ChatResponse<ChoiceFIM>>(SourceGenerationContext.NetworkOptions, token))!;
	}

	/// <summary>
	/// R1报错。stream有效。Tools无效。echo不能和suffix同时存在。  
	/// </summary>
	/// <param name="prompt">用于生成完成内容的提示</param> 
	/// <param name="echo">在输出中，把 prompt 的内容也输出出来</param>
	/// <param name="token">取消标记</param>
	/// <remarks><list type="bullet">
	/// <item>设个方法会把Stream设置为false</item>
	/// <item>R1报错。这个方法会设置模型为V3</item>
	/// <item>Tools无效</item>
	/// <item>logprobs需要为int。这个方法会把top_logprobs复制给logprobs</item> 
	/// <item>echo不能和suffix同时存在。因此只能选择方法重载一种访问。</item> 
	/// </list></remarks>
	public async Task<ChatResponse<ChoiceFIM>> FIMAsync(string prompt, bool echo, CancellationToken token = default)
	{
		Option.Stream = false;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions)!.AsObject();
		if (echo)
		{
			node["echo"] = true;
		}
		node["prompt"] = prompt;
		if ((bool?)node["logprobs"] == true)
		{
			node["logprobs"] = node["top_logprobs"]?.DeepClone();
		}
		using var response = await SendAsync(ChatServerUrlBetaFIM, node, token);
		return (await response.Content.ReadFromJsonAsync<ChatResponse<ChoiceFIM>>(SourceGenerationContext.NetworkOptions, token))!;
	}
	/// <summary>
	/// R1报错。stream有效。Tools无效。echo不能和suffix同时存在。  
	/// </summary>
	/// <param name="prompt">用于生成完成内容的提示</param>
	/// <param name="suffix">制定被补全内容的后缀</param> 
	/// <param name="token">取消标记</param>
	/// <remarks><list type="bullet">
	/// <item>设个方法会把Stream设置为true</item>
	/// <item>R1报错。这个方法会设置模型为V3</item>
	/// <item>Tools无效</item>
	/// <item>logprobs需要为int。这个方法会把top_logprobs复制给logprobs</item>
	/// </list></remarks>
	public async Task<ChatResponseDelta<ChoiceFIM>> FIMStreamAsync(string prompt, string? suffix = null, CancellationToken token = default)
	{
		Option.Stream = true;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions)!.AsObject();
		if (suffix != null)
		{
			node["suffix"] = suffix;
		}
		node["prompt"] = prompt;
		if ((bool?)node["logprobs"] == true)
		{
			node["logprobs"] = node["top_logprobs"]?.DeepClone();
		}
		var response = await SendAsync(ChatServerUrlBetaFIM, node, token);
		return new ChatResponseDelta<ChoiceFIM>(await response.Content.ReadAsStreamAsync(token), token, response);
	}

	/// <summary>
	/// R1报错。stream有效。Tools无效。echo不能和suffix同时存在。  
	/// </summary>
	/// <param name="prompt">用于生成完成内容的提示</param>
	/// <param name="echo">在输出中，把 prompt 的内容也输出出来</param>
	/// <remarks><list type="bullet">
	/// <item>设个方法会把Stream设置为true</item>
	/// <item>R1报错。这个方法会设置模型为V3</item>
	/// <item>Tools无效</item>
	/// <item>logprobs需要为int。这个方法会把top_logprobs复制给logprobs</item> 
	/// <item>echo不能和suffix同时存在。因此只能选择方法重载一种访问。</item> 
	/// <item>echo不能和logprobs同时存在。</item> 
	/// </list></remarks>
	public async Task<ChatResponseDelta<ChoiceFIM>> FIMStreamAsync(string prompt, bool echo, CancellationToken token = default)
	{
		Option.Stream = true;
		var node = JsonSerializer.SerializeToNode(Option, SourceGenerationContext.NetworkOptions)!.AsObject();
		if (echo && Option.TopLogprobs == null)
		{
			node["echo"] = true;
		}
		node["prompt"] = prompt;
		if ((bool?)node["logprobs"] == true)
		{
			node["logprobs"] = node["top_logprobs"]?.DeepClone();
		}
		var response = await SendAsync(ChatServerUrlBetaFIM, node, token);
		return new ChatResponseDelta<ChoiceFIM>(await response.Content.ReadAsStreamAsync(token), token, response);
	}
}
