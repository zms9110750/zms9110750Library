

using CommandLine;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// 1. 加载用户机密
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var deepSeek = config.GetSection("OpenAI:DeepSeek");
var endpoint = deepSeek["url"];
var apiKey = deepSeek["apiKey"];
var model = deepSeek["model"];

if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(model))
{
    Console.WriteLine("❌ 配置不完整，请检查 secrets.json");
    return;
}

// 2. 创建底层 ChatClient
var chatClient = new ChatClient(
    model,
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

Console.InputEncoding = System.Text.Encoding.UTF8;
// 3. 包装成 AIAgent
var agent = chatClient.AsAIAgent();

// 4. 简单一问一答
Console.WriteLine("=== 单轮对话 ===");
var response = await agent.RunAsync("用一句话介绍你自己");
Console.WriteLine(response.Text);

// 5. 带会话的多轮对话
Console.WriteLine("\n=== 多轮对话 ===");
var session = await agent.CreateSessionAsync();

Console.Write("> ");
var input = Console.ReadLine();
while (!string.IsNullOrWhiteSpace(input))
{
    await foreach (var message in agent.RunStreamingAsync(input, session))
    {
        Console.Write(message.Text);
    }
    Console.WriteLine();
    Console.Write("\n> ");
    input = Console.ReadLine();
}