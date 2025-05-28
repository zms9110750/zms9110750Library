namespace DeepSeekClient.Model.Message;

public class SystemMessage(string content, string? name = null) : Message(content, name)
{ 
	public override string Role => "system";
}
