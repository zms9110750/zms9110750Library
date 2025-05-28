namespace DeepSeekClient.Model.Message;

public class PrefixMessage : AssistantMessage
{
	public PrefixMessage(string reasoningContent, string? name = null) : base("", reasoningContent, name)
	{
		Json["prefix"] = true;
	}
}