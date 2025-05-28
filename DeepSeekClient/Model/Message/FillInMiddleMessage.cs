using DeepSeekClient.JsonConverter;
using Newtonsoft.Json.Linq;

namespace DeepSeekClient.Model.Message;

[Newtonsoft.Json.JsonConverter(typeof(NewtonsoftConverter<FillInMiddleMessage>)),
	System.Text.Json.Serialization.JsonConverter(typeof(SystemTextJsonConverter<FillInMiddleMessage>))]
public class FillInMiddleMessage(string content) : AssistantMessage(content, null), IStringValueObject<FillInMiddleMessage>
{
	string IStringValueObject<FillInMiddleMessage>.Value => Content;

	public static FillInMiddleMessage Creat(string value)
	{
		return new FillInMiddleMessage(value);
	}
}
