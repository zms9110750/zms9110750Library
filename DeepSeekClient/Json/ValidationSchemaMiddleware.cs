using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Schema;

namespace zms9110750.DeepSeekClient.Json;

/// <summary>
/// 验证处理管道
/// </summary>
public static class ValidationSchemaPipeline
{
	/// <summary>
	/// 默认验证中间件集合
	/// </summary>
	public static readonly IEnumerable<Func<Attribute, JsonObject, JsonObject>> DefaultMiddlewares = new List<Func<Attribute, JsonObject, JsonObject>>
{
	HandleDescription,
	HandleRequired,
	HandleRange,
	HandleMaxLength,
	HandleMinLength,
	HandleRegularExpression,
	HandleEmailAddress,
	HandleUrl,
	HandlePhone,
	HandleCreditCard,
	HandleCompare,
	HandleDataType,
	HandleStringLength
};

	/// <summary>
	/// 自定义验证中间件集合的默认行为
	/// </summary>
	public static IEnumerable<Func<Attribute, JsonObject, JsonObject>> Middlewares { get => field ?? DefaultMiddlewares; set; }


	/// <summary>
	/// 应用验证处理管道到JsonSchema
	/// </summary>  
	public static JsonObject ApplyValidationSchema(JsonObject schemaObject, IEnumerable<Attribute> attributes, IEnumerable<Func<Attribute, JsonObject, JsonObject>>? middleware = null)
	{
		middleware ??= Middlewares;
		// 遍历所有特性，应用中间件处理
		return attributes.SelectMany(_ => middleware, (attr, mw) => (attr, mw))
			 .Aggregate(schemaObject,
				  (current, tuple) => tuple.mw(tuple.attr, current)
			 );
	}


	/// <summary>
	/// 创建验证处理管道
	/// </summary>
	/// <param name="middleware">自定义中间件集合（默认使用DefaultMiddlewares）</param>
	/// <returns>组合后的处理委托</returns>
	public static JsonSchemaExporterOptions CreatePipelineSchemaExporter(IEnumerable<Func<Attribute, JsonObject, JsonObject>>? middleware = null)
	{
		middleware ??= DefaultMiddlewares;
		return new JsonSchemaExporterOptions
		{
			TransformSchemaNode = (context, schema) =>
			{
				// 获取所有特性
				var attributeProvider = context.PropertyInfo?.AttributeProvider ?? context.TypeInfo.Type;
				var attributes = attributeProvider.GetCustomAttributes(inherit: true).OfType<Attribute>();

				JsonObject schemaObject = schema switch
				{
					JsonObject => schema.AsObject(),
					JsonValue value when value.GetValueKind() == JsonValueKind.False => new JsonObject { ["not"] = true },
					_ => new JsonObject()
				};

				return ApplyValidationSchema(schemaObject, attributes, middleware);
			}
		};
	}

	/// <summary>
	/// 处理<see cref="DescriptionAttribute"/>特性
	/// </summary>
	public static JsonObject HandleDescription(Attribute attribute, JsonObject schema)
	{
		if (attribute is DescriptionAttribute descriptionAttr)
		{
			schema.Insert(0, "description", descriptionAttr.Description);
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="RequiredAttribute"/>特性
	/// </summary>
	public static JsonObject HandleRequired(Attribute attribute, JsonObject schema)
	{
		if (attribute is RequiredAttribute)
		{
			if (schema.TryGetPropertyValue("type", out var typeNode) &&
				typeNode?.ToString() == "string")
			{
				schema["minLength"] = 1;
			}
			else
			{
				schema["required"] = true;
			}
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="RangeAttribute"/>特性
	/// </summary>
	public static JsonObject HandleRange(Attribute attribute, JsonObject schema)
	{
		if (attribute is RangeAttribute rangeAttr)
		{
			if (rangeAttr.Minimum != null)
				schema["minimum"] = Convert.ToDouble(rangeAttr.Minimum);

			if (rangeAttr.Maximum != null)
				schema["maximum"] = Convert.ToDouble(rangeAttr.Maximum);
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="MaxLengthAttribute"/>特性
	/// </summary>
	public static JsonObject HandleMaxLength(Attribute attribute, JsonObject schema)
	{
		if (attribute is MaxLengthAttribute maxLengthAttr)
		{
			schema["maxLength"] = maxLengthAttr.Length;
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="MinLengthAttribute"/>特性
	/// </summary>
	public static JsonObject HandleMinLength(Attribute attribute, JsonObject schema)
	{
		if (attribute is MinLengthAttribute minLengthAttr)
		{
			schema["minLength"] = minLengthAttr.Length;
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="RegularExpressionAttribute"/>特性
	/// </summary>
	public static JsonObject HandleRegularExpression(Attribute attribute, JsonObject schema)
	{
		if (attribute is RegularExpressionAttribute regexAttr)
		{
			schema["pattern"] = regexAttr.Pattern;
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="EmailAddressAttribute"/>特性
	/// </summary>
	public static JsonObject HandleEmailAddress(Attribute attribute, JsonObject schema)
	{
		if (attribute is EmailAddressAttribute)
		{
			schema["format"] = "email";
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="UrlAttribute"/>特性
	/// </summary>
	public static JsonObject HandleUrl(Attribute attribute, JsonObject schema)
	{
		if (attribute is UrlAttribute)
		{
			schema["format"] = "uri";
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="PhoneAttribute"/>特性
	/// </summary>
	public static JsonObject HandlePhone(Attribute attribute, JsonObject schema)
	{
		if (attribute is PhoneAttribute)
		{
			schema["format"] = "phone";
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="CreditCardAttribute"/>特性
	/// </summary>
	public static JsonObject HandleCreditCard(Attribute attribute, JsonObject schema)
	{
		if (attribute is CreditCardAttribute)
		{
			schema["format"] = "credit-card";
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="CompareAttribute"/>特性
	/// </summary>
	public static JsonObject HandleCompare(Attribute attribute, JsonObject schema)
	{
		if (attribute is CompareAttribute compareAttr)
		{
			schema["compareTo"] = compareAttr.OtherProperty;
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="DataTypeAttribute"/>特性
	/// </summary>
	public static JsonObject HandleDataType(Attribute attribute, JsonObject schema)
	{
		if (attribute is DataTypeAttribute dataTypeAttr)
		{
			switch (dataTypeAttr.DataType)
			{
				case DataType.Date:
					schema["format"] = "date";
					break;
				case DataType.DateTime:
					schema["format"] = "date-time";
					break;
				case DataType.Time:
					schema["format"] = "time";
					break;
				case DataType.Currency:
					schema["format"] = "currency";
					break;
				case DataType.Password:
					schema["format"] = "password";
					break;
			}
		}
		return schema;
	}

	/// <summary>
	/// 处理<see cref="StringLengthAttribute"/>特性
	/// </summary>
	public static JsonObject HandleStringLength(Attribute attribute, JsonObject schema)
	{
		if (attribute is StringLengthAttribute stringLengthAttr)
		{
			if (stringLengthAttr.MaximumLength > 0)
				schema["maxLength"] = stringLengthAttr.MaximumLength;

			if (stringLengthAttr.MinimumLength > 0)
				schema["minLength"] = stringLengthAttr.MinimumLength;
		}
		return schema;
	}
}