using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace zms9110750.TreeCollection.Ordered;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
public class TreeListNodeConverterFactory : JsonConverterFactory

{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsGenericType &&
			   typeToConvert.GetGenericTypeDefinition() == typeof(TreeNode<>);
	}

	public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
	{
		Type valueType = type.GetGenericArguments()[0];
		Type converterType = typeof(TreeListNodeJsonConverter<>).MakeGenericType(valueType);
		return (JsonConverter)Activator.CreateInstance(converterType)!;
	}
}

public class TreeListNodeJsonConverter<T> : JsonConverter<TreeNode<T>>
{
	public override TreeNode<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// 先将JSON读取为JsonObject
		JsonObject jsonObject = JsonNode.Parse(ref reader)?.AsObject() ?? throw new JsonException("Invalid JSON object");

		// 从JsonObject中提取数据
		T? value = (jsonObject["Value"] ?? throw new JsonException("Missing 'Value' property")).GetValue<T>();
		var node = new TreeNode<T>(value);

		if (jsonObject["Children"] is JsonArray childrenArray)
		{
			node.Add(childrenArray.OfType<JsonObject>().Select(childNode => childNode.Deserialize<TreeNode<T>>(options)!));
		}
		return node;
	}

	public override void Write(Utf8JsonWriter writer, TreeNode<T> value, JsonSerializerOptions options)
	{
		// 创建JsonObject表示节点
		var jsonObject = new JsonObject
		{
			["Value"] = JsonValue.Create(value.Value),
		};
		if (value.Count > 0)
		{
			jsonObject["Children"] = new JsonArray(value.Select(child =>
					JsonSerializer.SerializeToNode(child, options)).ToArray());
		}

		// 写入JsonObject
		jsonObject.WriteTo(writer, options);
	}
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释