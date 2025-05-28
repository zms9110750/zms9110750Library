namespace DeepSeekClient.Model.Message;

public class UserMessage(string content, string? name = null) : Message(content, name)
{
	public override string Role => "user";
}
