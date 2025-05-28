using DeepSeekClient.Model.Message;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace DeepSeekClient.Model.Tool;

public class ToolKit : IDictionary<string, ToolBase>
{
	Dictionary<string, ToolBase> Tools { get; } = [];

	public ICollection<string> Keys => ((IDictionary<string, ToolBase>)Tools).Keys;

	public ICollection<ToolBase> Values => ((IDictionary<string, ToolBase>)Tools).Values;

	public int Count => ((ICollection<KeyValuePair<string, ToolBase>>)Tools).Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<string, ToolBase>>)Tools).IsReadOnly;

	public ToolBase this[string key] { get => ((IDictionary<string, ToolBase>)Tools)[key]; set => ((IDictionary<string, ToolBase>)Tools)[key] = value; }
	public ToolChoice? Choice { get; }
	public void Add(ToolBase tool)
	{
		Tools[tool.Key] = tool;
	}
	public void Remove(string key)
	{
		Tools.Remove(key);
	}
	public void Remove(ToolBase tool)
	{
		Tools.Remove(tool.Key);
	}
	public ToolMessage Invoke(ToolCall call)
	{
		// TODO: Implement ToolChoice
		return new ToolMessage(Tools[call.Key].Invoke(default).ToString(), call.Id);
	}
	public override string ToString()
	{
		return CloneJson().ToString();
	}

	public JsonArray CloneJson()
	{
		JsonArray tools = new JsonArray();
		foreach (var tool in Tools.Values)
		{
			JsonObject json = new JsonObject();
			json["type"] = tool.Type;
			json[tool.Type] = JsonNode.Parse(tool.ToString());
			tools.Add(json);
		}
		return tools;
	}

	public void Add(string key, ToolBase value)
	{
		((IDictionary<string, ToolBase>)Tools).Add(key, value);
	}

	public bool ContainsKey(string key)
	{
		return ((IDictionary<string, ToolBase>)Tools).ContainsKey(key);
	}

	bool IDictionary<string, ToolBase>.Remove(string key)
	{
		return ((IDictionary<string, ToolBase>)Tools).Remove(key);
	}

	public bool TryGetValue(string key, [MaybeNullWhen(false)] out ToolBase value)
	{
		return ((IDictionary<string, ToolBase>)Tools).TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<string, ToolBase> item)
	{
		((ICollection<KeyValuePair<string, ToolBase>>)Tools).Add(item);
	}

	public void Clear()
	{
		((ICollection<KeyValuePair<string, ToolBase>>)Tools).Clear();
	}

	public bool Contains(KeyValuePair<string, ToolBase> item)
	{
		return ((ICollection<KeyValuePair<string, ToolBase>>)Tools).Contains(item);
	}

	public void CopyTo(KeyValuePair<string, ToolBase>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<string, ToolBase>>)Tools).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<string, ToolBase> item)
	{
		return ((ICollection<KeyValuePair<string, ToolBase>>)Tools).Remove(item);
	}

	public IEnumerator<KeyValuePair<string, ToolBase>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<string, ToolBase>>)Tools).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)Tools).GetEnumerator();
	}
}
