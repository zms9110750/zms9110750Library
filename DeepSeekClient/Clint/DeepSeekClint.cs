using DeepSeekClient.Model.Response;
using DeepSeekClient.Model.Tool;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Clint;
using Model = Model.ModelList.Model;
using Meg = Model.Message.Message;
using Format = ResponseFormat;



public class DeepSeekClint : WithJsonObject
{
	public const string FIMCompletions = "https://api.deepseek.com/beta/completions";
	public const string Completions = "https://api.deepseek.com/chat/completions";
	HttpClient HttpClient { get; }
	public Model Model { get; set { Json["model"] = (field = value).Id; } }
	public double? FrequencyPenalty { get; set => Json["frequency_penalty"] = field = ThrowIfNotBetween(value, -2, 2); }
	public int? MaxTokens { get; set => Json["max_tokens"] = field = ThrowIfNotBetween(value, 1, 8192); }
	public double? PresencePenalty { get; set => Json["presence_penalty"] = field = ThrowIfNotBetween(value, -2, 2); }
	public Format ResponseFormat
	{
		get; set => Json["response_format"]!["type"] = (field = value) switch
		{
			Format.Text => "text",
			Format.Json => "json_object",
			_ => throw new NotImplementedException()
		};
	}
	public StringValues Stop { get; set => Json["stop"] = JsonValue.Create(field = value); }
	public bool? StreamIncludeUsage
	{
		get;
		set
		{
			switch (value)
			{
				case var _ when value == field:
					return;
				case null:
					Json.Remove("stream");
					Json.Remove("stream_options");
					break;
				default:
					Json["stream"] = true;
					Json["stream_options"] = new JsonObject() { ["include_usage"] = value };
					break;
			}
			field = value;
		}
	}
	public double? Temperature { get; set => Json["temperature"] = field = ThrowIfNotBetween(value, 0, 2); }
	public double? TopP { get; set => Json["top_p"] = field = ThrowIfNotBetween(value, 0, 1); }
	public int? TopLogprobs
	{
		get;
		set
		{
			switch (value)
			{
				case var _ when value == field:
					return;
				case null:
					Json.Remove("logprobs");
					Json.Remove("top_logprobs");
					break;
				default:
					Json["logprobs"] = true;
					Json["top_logprobs"] = ThrowIfNotBetween(value, 0, 20);
					break;
			}
			field = value;
		}
	}


	/// <summary>
	/// 在输出中，把 prompt 的内容也输出出来
	/// </summary>
	public bool? Echo { get; set => Json["echo"] = field = value; }

	/// <summary>
	/// 制定被补全内容的后缀。
	/// </summary>
	public string? Suffix { get; set => Json["suffix"] = field = value; }

	public DeepSeekClint(string apiKey, HttpClient? http = null)
	{
		HttpClient = http ?? new HttpClient();
		HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
		Model = Model.V3;
		Json["response_format"] = new JsonObject();
		ResponseFormat = Format.Text;
	}
	public async Task<Responses<ChatChoice>> ChatAsync(IEnumerable<Meg> messages, ToolKit? kit = null)
	{
		if (messages?.Any() != true)
		{
			throw new ArgumentException("messages cannot be empty.");
		}
		if (Model == Model.R1)
		{
			TopLogprobs = null;
			kit = null;
		}
		if (kit?.Count == 0)
		{
			kit = null;
		}
		var json = CloneJson();
		json["messages"] = new JsonArray([.. messages.Select(s => s.CloneJson())]);
		json["tools"] = kit?.CloneJson();
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, Completions)
		{
			Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json"),
		};
		using var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
		Console.WriteLine(await response.Content.ReadAsStringAsync());
		return System.Text.Json.JsonSerializer.Deserialize<Responses<ChatChoice>>(await response.Content.ReadAsStringAsync());
	}

	public async IAsyncEnumerable<(Responses<ChatChoice>?, Meg?)> ChatAutoCallToolAsync(IEnumerable<Meg> messages, ToolKit? kit = null)
	{
		var sys = Meg.NewSystemMsg("如果工具调用还不够。可以继续调用。知道认为满意或认为应该先行报告。");
		var list = messages.ToList();
		var resert = await ChatAsync(list, kit);
		list.Add(resert.Choices[0].Message);
		yield return (resert, null);
		while (resert.Choices[0].FinishReason == Finish.ToolCalls && resert.Choices[0].Message.ToolCall is { Length: > 0 } calls && kit?.Count > 0)
		{

			foreach (var item in calls)
			{
				var t = kit.Invoke(item);
				list.Add(t);
				yield return (null, t);
			}
			resert = await ChatAsync(list.Append(sys), kit);
			list.Add(resert.Choices[0].Message);
			yield return (resert, null);
		}
	}


	public async Task<string> FIMAsync(string prompt, string? suffix = null)
	{
		Suffix = suffix;
		if (Model == Model.R1)
		{
			Model = Model.V3;
		}
		if (Echo == true)
		{
			Suffix = null;
			TopLogprobs = null;
		}
		var json = CloneJson();
		json["logprobs"] = json["top_logprobs"]?.DeepClone();
		json["prompt"] = prompt;
		var content = new StringContent(ToString(), Encoding.UTF8, "application/json");
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, FIMCompletions)
		{
			Content = content,
		};
		using var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
		return await response.Content.ReadAsStringAsync();
	}
	static T? ThrowIfNotBetween<T>(T? value, T min, T max, [CallerMemberName] string callName = "") where T : struct, INumber<T>
	{
		return value > max || value < min
			? throw new ArgumentOutOfRangeException(callName, $"{callName} must be between {min} and {max}.")
			: value;
	}

	public override string ToString()
	{
		return Json.ToString();
	}
}

