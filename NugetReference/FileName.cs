

using DeepSeekClient.Model.Balance;
using DeepSeekClient.Model.Message;
using DeepSeekClient.Model.ModelList;
using System.Text.Json;
using zms9110750.DeepSeekClient.Model.Tool;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using zms9110750.DeepSeekClient.Model.Tool.FunctionCall;
using zms9110750.DeepSeekClient.Model.Response;
using System.Diagnostics.CodeAnalysis;
using zms9110750.DeepSeekClient.Model.Response.Delta;
using zms9110750.DeepSeekClient.Model.Request;
using zms9110750.DeepSeekClient.Beta;
using zms9110750.DeepSeekClient.Model.ModelList;
using zms9110750.DeepSeekClient;

//创建一个连接器
DeepSeekApiClient deepSeek = new DeepSeekApiClient("sk-15c0c15987d34a0e95f888a424d25e99");

//工具箱可以添加任何委托作为函数调用
deepSeek.Option.Tools = new ToolKit();
deepSeek.Option.Tools.Add((Func<int, int, int>)Random.Shared.Next);

//设置消息列表
List<Message> messages = new List<Message>();
deepSeek.Option.Messages = messages;
messages.Add(Message.NewUserMsg("测试API，试试调用这个方法5次。生成上亿的数字。"));

//发出请求
var response = await deepSeek.ChatAsync();
//记得把回应添加进消息列表
messages.Add(response.Choices[0].Message);

//调用工具箱
if (response.Choices[0].Message.ToolCalls != null)
{
	foreach (var toolCall in response.Choices[0].Message.ToolCalls!)
	{
		//工具箱调用消息里的API调用请求。
		messages.Add(deepSeek.Option.Tools.Invoke(toolCall));
	}
}

messages.Add(Message.NewUserMsg("可以告诉我得到了多少吗"));


//设置模型
deepSeek.Option.SetModel(ChatModel.R1);


//使用流式请求
var response2 = await deepSeek.ChatStreamAsync();
bool b = false;

//流式请求可以通过异步迭代器获取途中的消息。
await foreach (var item in response2)
{
	//增量消息通过Delta属性获取。
	Console.Write(item.Delta?.ReasoningContent);
	if (!b && string.IsNullOrEmpty(item.Delta?.ReasoningContent))
	{
		b = true;
		Console.WriteLine();
		Console.WriteLine();
	}
	Console.Write(item.Delta?.Content);
}

//也可以直接await获取拼接完的消息
var response3 = await response2;
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

//ReasoningContent属性获取思维链内容
Console.WriteLine(response3.Choices[0].Message.ReasoningContent);
//Content属性获取普通消息内容
Console.WriteLine(response3.Choices[0].Message.Content);
messages.Add(response3.Choices[0].Message);





class P
{
	public static string Add(int a)
	{
		return "你好" + a;
	}
}



[JsonSourceGenerationOptions(WriteIndented = true,
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	UseStringEnumConverter = true,
	NumberHandling = JsonNumberHandling.AllowReadingFromString,
	AllowOutOfOrderMetadataProperties = true,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	)]
[JsonSerializable(typeof(UserBalance))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(ModelResponse))]
[JsonSerializable(typeof(ToolCall))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(ToolKit))]
[JsonSerializable(typeof(ChatResponse))]
[JsonSerializable(typeof(ChatResponseDelta[]))]
[JsonSerializable(typeof(Choice))]
[JsonSerializable(typeof(ChatOption))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
	[field: AllowNull]
	public static JsonSerializerOptions UnsafeRelaxed => field ??= new JsonSerializerOptions(Default.Options)
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};
}
