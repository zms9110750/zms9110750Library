```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using zms9110750.DeepSeekClient;
using zms9110750.DeepSeekClient.Model.Chat.Tool.Function;

//传入Key
DeepSeekApiClient deepSeek = new DeepSeekApiClient("sk-841a1a2e176");

//为工具箱添加方法
FunctionRequest? fun = deepSeek.OptionDefault.Tools.Add(CompleteExampleModel.Hello);
//为方法限制参数
fun.ParametersWithEnum("a", [40, 60]);
//必须调用方法（强制工具选择的设置都在工具箱里)
deepSeek.OptionDefault.Tools.ToolCallRequire = true;
//一个忽略工具箱的设置在设置选项上
deepSeek.OptionDefault.IgnoreTools = false;

//最大token数量
deepSeek.OptionDefault.MaxTokens = 100;
//启用流式回复
deepSeek.OptionDefault.Stream = true;


//添加消息
deepSeek.OptionDefault.MessageAddUser("测试用例。调用方法");

//请求会得到一个IAsyncEnumerable
var request = await deepSeek.ChatAsync();

//你可以用await foreach来得到流式数据。如果没有开启，仍然可以用await foreach。不过只有最终结果。
await foreach (var item in request)
{
	Console.Write(item.ChoiceFirst.Message.Content);
}

//无论是否启用流，都可以直接await IAsyncEnumerable得到合并以后的结果
var result = await request;
var messageCall = result.ChoiceFirst.Message;
//别忘了添加消息进去。
deepSeek.OptionDefault.MessagesDefault.Add(messageCall);

foreach (var item in messageCall.ToolCalls ?? [])
{
	//工具箱的Invoke方法可以直接调用方法并生成工具调用消息
	deepSeek.OptionDefault.MessagesDefault.Add(deepSeek.OptionDefault.Tools.Invoke(item));
}

deepSeek.OptionDefault.IgnoreTools = true;

deepSeek.OptionDefault.MessageAddUser("测试怎么样？成功吗？输出了什么？");
var result2 = await await deepSeek.ChatAsync();
Console.WriteLine(result2.ChoiceFirst.Message.Content);


//可以分析大部分的System.ComponentModel.DataAnnotations.ValidationAttribute特性下的类
public record CompleteExampleModel
{
	[Required]
	[StringLength(50, MinimumLength = 2)]
	[Description("用户名")]
	public string Username { get; set; } = string.Empty;

	[Range(18, 99)]
	public int Age { get; set; }

	[DataType(DataType.Date)]
	public DateTime Birthday { get; set; }

	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	[Compare("Password")]
	public string ConfirmPassword { get; set; } = string.Empty;

	[MaxLength(200)]
	[MinLength(10)]
	public string Bio { get; set; } = string.Empty;

	[RegularExpression(@"^[A-Za-z0-9_-]{3,16}$")]
	public string Alias { get; set; } = string.Empty;

	[Description("测试用例")]//方法上的特性也可以分析。
	public static int Hello([Range(40, 80)] int a, [Description("测试用例")] CompleteExampleModel model)
	{
		Console.WriteLine(model);
		return -a;
	}
}
```