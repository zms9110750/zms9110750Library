using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Reflection;

namespace OpenAiReference;

public static class ChatClientBuildExtension
{
	/// <summary>
	/// 从用户机密文件的配置中，注册所有的OpenAI兼容API的聊天客户端。 
	/// </summary>
	/// <param name="services">服务容器</param>   
	/// <remarks>
	/// 配置节应该类似如下格式
	/// <code>{
	///	"OpenAI": {
	///		"DeepSeek": {
	///			"url": "https://api.deepseek.com",
	///			"apiKey": "token",
	///			"model": "deepseek-chat"
	///		}
	///	}
	///}</code>
	///其中，服务提供商名称会作为键进行键控注册。<br/>
	///使用<see cref="FromKeyedServicesAttribute"/>特性访问键控注册。<br/>
	///使用<see cref="ChatClient"/>类型接受注册的客户端。
	/// </remarks>
	public static IServiceCollection AddChatClient(this IServiceCollection services)
	{
		var configuration = new ConfigurationBuilder()
					.AddUserSecrets(Assembly.GetCallingAssembly())
					.AddUserSecrets(Assembly.GetEntryAssembly())
					.Build();
		foreach (var item in configuration.GetSection("OpenAI").GetChildren())
		{
			string? serverName = item.Key;
			string? url = item.GetValue<string>("url");
			string? apiKey = item.GetValue<string>("apiKey");
			string? model = item.GetValue<string>("model");
			if (serverName is not null && url is not null && apiKey is not null && model is not null)
			{
				services.AddKeyedSingleton(serverName, (sp, _) =>
				  new ChatClient(model, new ApiKeyCredential(apiKey), new OpenAIClientOptions() { Endpoint = new Uri(url) }));
			}
		}
		return services;
	}
} 