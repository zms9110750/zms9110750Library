namespace zms9110750.InterfaceImplAsExtensionGenerator.SyntaxProcessors;

static class SourceGeneratorExtensions
{
	/// <summary>
	/// 获取类型的全局完全限定名
	/// </summary>
	public static string ToGlobalDisplayString(this ISymbol typeSymbol)
	{
		return typeSymbol == null
			? string.Empty
			: typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}

	/// <summary>
	/// 判断类型是否与指定的类型名称相同
	/// </summary>
	public static bool EqualName<T>(this ISymbol? typeSymbol)
	{
		return typeSymbol != null && typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) == typeof(T).FullName;
	}


	/// <summary>
	/// 尝试获取命名参数的值
	/// </summary>
	public static bool TryGet<T>(this AttributeData attributeData, string parameterName, out T? value)
	{
		value = default;
		if (attributeData == null)
		{
			return false;
		} 

		foreach (var namedArg in attributeData.NamedArguments)
		{
			if (namedArg.Key == parameterName && namedArg.Value.Value is T val)
			{
				value = val;
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 尝试获取构造函数参数的值
	/// </summary>
	public static bool TryGet<T>(this AttributeData attributeData, int index, out T? value)
	{
		value = default;
		if (attributeData?.ConstructorArguments.IsDefaultOrEmpty != false)
		{
			return false;
		}

		if (index < attributeData.ConstructorArguments.Length)
		{
			var arg = attributeData.ConstructorArguments[index];
			if (arg.Value is T val)
			{
				value = val;
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 获取命名参数的值，如果不存在则返回默认值
	/// </summary>
	public static T? GetOrDefault<T>(this AttributeData attributeData, string parameterName, T? defaultValue = default)
	{
		return attributeData.TryGet(parameterName, out T? value) ? value : defaultValue;
	}

	/// <summary>
	/// 获取构造函数参数的值，如果不存在则返回默认值
	/// </summary>
	public static T? GetOrDefault<T>(this AttributeData attributeData, int index, T? defaultValue = default)
	{
		return attributeData.TryGet(index, out T? value) ? value : defaultValue;
	}

	public static StringBuilder AppendJoin<T>(this StringBuilder sb, string? separator, IEnumerable<T?> values)
	{
		using var enumerator = values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			sb.Append(enumerator.Current);
		}
		while (enumerator.MoveNext())
		{
			sb.Append(separator);
			sb.Append(enumerator.Current);
		}
		return sb;
	}


	/// <summary>
	/// 生成泛型参数约束代码
	/// </summary>
	public static StringBuilder GenerateConstraints(this ITypeParameterSymbol parameter, StringBuilder? sb = null)
	{
		sb ??= new StringBuilder();
		var constraintStrings = new List<string>();
		if (parameter.HasReferenceTypeConstraint)
		{
			constraintStrings.Add("class");
		}
		if (parameter.HasValueTypeConstraint)
		{
			constraintStrings.Add("struct");
		}
		if (parameter.HasUnmanagedTypeConstraint)
		{
			constraintStrings.Add("unmanaged");
		}
		if (parameter.HasNotNullConstraint)
		{
			constraintStrings.Add("notnull");
		}
		// 类型约束
		foreach (var constraintType in parameter.ConstraintTypes)
		{
			constraintStrings.Add(constraintType.ToGlobalDisplayString());
		}
		if (parameter.HasConstructorConstraint)
		{
			constraintStrings.Add("new()");
		}
		if (constraintStrings.Any())
		{
			sb.AppendLine();
			sb.Append("             where ");
			sb.Append(parameter.Name);
			sb.Append(" : ");
			sb.AppendJoin(", ", constraintStrings);
		}
		return sb;
	}

	public static TValue? GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? dictionary, TKey key, TValue? defaultValue = default)
	{
		return dictionary != null && dictionary.TryGetValue(key, out TValue? value) ? value : defaultValue;
	}
}