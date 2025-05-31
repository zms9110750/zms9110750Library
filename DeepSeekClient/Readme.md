```csharp
//创建一个连接器
DeepSeekApiClient deepSeek = new DeepSeekApiClient("sk-15c0c15987d34");

//工具箱可以添加任何委托作为函数调用
deepSeek.Option.Tools = new ToolKit();
deepSeek.Option.Tools.Add((Func<int, int, int>)Random.Shared.Next);

//设置消息列表
List<Message> messages = new List<Message>();
deepSeek.Option.Messages = messages;
messages.Add(Message.NewUserMsg("测试API，试试调用一个方法。生成一个上亿的数字。"));

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

```