using zms9110750.InterfaceImplAsExtensionGenerator.Config;

namespace zms9110750.InterfaceImplAsExtensionGenerator.SyntaxProcessors;

/// <summary>
/// 接口成员分析器，用于分析接口成员并生成对应的扩展代码
/// </summary>

class MemberAnalyzer : BaseAnalyzer
{
	/// <summary>
	/// 当前分析的成员符号
	/// </summary>
	public ISymbol MemberSymbol { get; }

	/// <summary>
	/// 成员上的IncludeInterfaceMemberAsExtensionAttribute特性数据（可能为null）
	/// </summary>
	public AttributeData? MemberAttribute { get; }

	/// <summary>
	/// 所属的类分析器实例
	/// </summary>
	public ClassAnalyzer ClassAnalyzer { get; }


	public override bool IsValid
	{
		get
		{
			// 检查成员可访问性，排除internal等
			if (!IsAccessible(MemberSymbol))
			{
				return false;
			}

			if (MemberSymbol is IMethodSymbol { AssociatedSymbol: not null })
			{
				return false;
			}
			if (MemberAttribute?.GetOrDefault<bool?>(nameof(IncludeInterfaceMemberAsExtensionAttribute.ForceGenerate)) is bool b)
			{
				return b;
			}
			var generateMembers = ClassAnalyzer.DefaultGenerateMembers;
			return MemberSymbol.Kind switch
			{
				SymbolKind.Method => generateMembers.HasFlag(GenerateMembers.Method),
				SymbolKind.Property when MemberSymbol is IPropertySymbol { IsIndexer: true } => generateMembers.HasFlag(GenerateMembers.Indexer),
				SymbolKind.Property => generateMembers.HasFlag(GenerateMembers.Property),
				SymbolKind.Event => generateMembers.HasFlag(GenerateMembers.Event),
				_ => false
			};
		}
	}

	/// <summary>
	/// 扩展成员的名称
	/// </summary>
	/// <remarks>
	/// 若特性指定了ReplacementMemberName则使用该名称，否则使用成员原名称
	/// </remarks>
	public string MemberName
	{
		get
		{
			var replacementName = MemberAttribute?.GetOrDefault<string>(nameof(IncludeInterfaceMemberAsExtensionAttribute.ReplacementMemberName));
			return string.IsNullOrEmpty(replacementName) ? MemberSymbol.Name : replacementName!;
		}
	}

	/// <summary>
	/// 实例参数的名称
	/// </summary>
	/// <remarks>
	/// 若使用传统语法且特性指定了InstanceParameterName则使用该名称，否则使用类分析器的默认实例参数名
	/// </remarks>
	public string InstanceParameterName
	{
		get
		{
			if (MemberAttribute != null && ClassAnalyzer.GlobalConfig.UseLegacySyntax)
			{
				string? instanceParamName = MemberAttribute.GetOrDefault<string>(nameof(IncludeInterfaceMemberAsExtensionAttribute.InstanceParameterName));
				if (!string.IsNullOrEmpty(instanceParamName))
				{
					return instanceParamName!;
				}
			}
			return ClassAnalyzer.InstanceParameterName;
		}
	}

	/// <summary>
	/// 初始化成员分析器实例
	/// </summary>
	/// <param name="memberSymbol">要分析的成员符号</param>
	/// <param name="classAnalyzer">所属的类分析器实例</param>
	public MemberAnalyzer(ISymbol memberSymbol, ClassAnalyzer classAnalyzer)
	{
		MemberSymbol = memberSymbol;
		// 从成员符号中查找IncludeInterfaceMemberAsExtensionAttribute特性
		MemberAttribute = memberSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.EqualName<IncludeInterfaceMemberAsExtensionAttribute>());
		ClassAnalyzer = classAnalyzer;
	}


	public override StringBuilder ToString(StringBuilder sb)
	{
		if (!IsValid)
		{
			return sb;
		}
		//处理旧语法
		if (ClassAnalyzer.GlobalConfig.UseLegacySyntax)
		{
			switch (MemberSymbol)
			{
				case IPropertySymbol symbol:
					if (IsAccessible(symbol.GetMethod))
					{
						GenerateExtensionMethod(sb, symbol.GetMethod);
					}
					if (IsAccessible(symbol.SetMethod) && !symbol.SetMethod!.IsInitOnly)
					{
						GenerateExtensionMethod(sb, symbol.SetMethod);
					}
					break;
				case IEventSymbol symbol:
					GenerateExtensionMethod(sb, symbol.AddMethod);
					GenerateExtensionMethod(sb, symbol.RemoveMethod);
					break;
				case IMethodSymbol symbol:
					GenerateExtensionMethod(sb, symbol);
					break;
			}
		}
		else if (MemberSymbol is not IPropertySymbol propertySymbol || !(!IsAccessible(propertySymbol.GetMethod) && propertySymbol.SetMethod?.IsInitOnly == true))
		{
			// 处理新语法
			GenerateDocumentationComment(sb, MemberSymbol);
			switch (MemberSymbol)
			{
				case IPropertySymbol { IsIndexer: true } symbol:
					GenerateNewIndexer(sb, symbol);
					break;
				case IPropertySymbol symbol:
					GenerateNewProperty(sb, symbol);
					break;
				case IEventSymbol symbol:
					GenerateNewEvent(sb, symbol);
					break;
				case IMethodSymbol symbol:
					GenerateExtensionMethod(sb, symbol, false);
					break;
			}
		}
		return sb;
	}

	/// <summary>
	/// 为属性生成新语法的扩展属性代码
	/// </summary> 
	private StringBuilder GenerateNewProperty(StringBuilder sb, IPropertySymbol symbol)
	{
		sb.Append("        ");
		GetAccessibility(sb, symbol.DeclaredAccessibility);
		sb.Append(symbol.Type.ToGlobalDisplayString());
		sb.Append(" ");
		sb.Append(MemberName);
		sb.AppendLine();
		sb.AppendLine("        {");
		if (IsAccessible(symbol.GetMethod))
		{
			sb.Append("            ");
			if (symbol.GetMethod!.DeclaredAccessibility != symbol.DeclaredAccessibility)
			{
				switch (symbol.GetMethod!.DeclaredAccessibility)
				{
					case Accessibility.ProtectedAndInternal:
					case Accessibility.Internal:
						sb.Append("internal ");
						break;
				}
			}
			sb.Append("get => ");
			sb.Append(InstanceParameterName);
			sb.Append(".");
			sb.Append(symbol.Name);
			sb.Append(";");
			sb.AppendLine();
		}
		if (IsAccessible(symbol.SetMethod) && symbol.SetMethod?.IsInitOnly == false)
		{
			sb.Append("            ");
			if (symbol.SetMethod!.DeclaredAccessibility != symbol.DeclaredAccessibility)
			{
				switch (symbol.SetMethod!.DeclaredAccessibility)
				{
					case Accessibility.ProtectedAndInternal:
					case Accessibility.Internal:
						sb.Append("internal ");
						break;
				}
			}
			sb.Append("set => ");
			sb.Append(InstanceParameterName);
			sb.Append(".");
			sb.Append(symbol.Name);
			sb.Append(" = value;");
			sb.AppendLine();
		}
		sb.AppendLine("        }");
		return sb;
	}

	/// <summary>
	/// 为索引器生成新语法的扩展索引器代码
	/// </summary> 
	private StringBuilder GenerateNewIndexer(StringBuilder sb, IPropertySymbol symbol)
	{
		sb.Append("        ");
		GetAccessibility(sb, symbol.DeclaredAccessibility);
		sb.Append(symbol.Type.ToGlobalDisplayString());
		sb.Append(" this[");
		sb.AppendJoin(",", symbol.Parameters.Select(p =>
		{
			sb.Append(p.Type.ToGlobalDisplayString());
			sb.Append(" ");
			return p.Name;
		}));
		sb.AppendLine("]");
		sb.AppendLine("        {");
		if (IsAccessible(symbol.GetMethod))
		{
			if (symbol.GetMethod!.DeclaredAccessibility != symbol.DeclaredAccessibility)
			{
				switch (symbol.GetMethod!.DeclaredAccessibility)
				{
					case Accessibility.ProtectedAndInternal:
					case Accessibility.Internal:
						sb.Append("internal ");
						break;
				}
			}
			sb.Append("get => ");
			sb.Append(InstanceParameterName);
			sb.Append("[");
			sb.AppendJoin(",", symbol.Parameters.Select(p => p.Name));
			sb.Append("];");
			sb.AppendLine();
		}
		if (IsAccessible(symbol.SetMethod) && symbol.SetMethod?.IsInitOnly == false)
		{
			sb.Append("            ");
			if (symbol.SetMethod!.DeclaredAccessibility != symbol.DeclaredAccessibility)
			{
				switch (symbol.SetMethod!.DeclaredAccessibility)
				{
					case Accessibility.ProtectedAndInternal:
					case Accessibility.Internal:
						sb.Append("internal ");
						break;
				}
			}
			sb.Append("set => ");
			sb.Append(InstanceParameterName);
			sb.Append("[");
			sb.AppendJoin(",", symbol.Parameters.Select(p => p.Name));
			sb.Append("];");
			sb.Append(" = value;");
			sb.AppendLine();
		}
		sb.AppendLine("        }");
		return sb;
	}

	/// <summary>
	/// 为事件生成新语法的扩展事件代码
	/// </summary> 
	/// <returns>生成的扩展事件代码，静态事件返回null</returns>
	private StringBuilder GenerateNewEvent(StringBuilder sb, IEventSymbol symbol)
	{
		sb.Append("        ");
		GetAccessibility(sb, symbol.DeclaredAccessibility);
		sb.Append("event ");
		sb.Append(symbol.Type.ToGlobalDisplayString());
		sb.Append(" ");
		sb.Append(MemberName);
		sb.AppendLine();
		sb.AppendLine("        {");
		sb.Append("            add => ");
		sb.Append(InstanceParameterName);
		sb.Append(".");
		sb.Append(symbol.Name);
		sb.Append(" += value;");
		sb.AppendLine();
		sb.Append("            remove => ");
		sb.Append(InstanceParameterName);
		sb.Append(".");
		sb.Append(symbol.Name);
		sb.Append(" -= value;");
		sb.AppendLine();
		sb.AppendLine("        }");
		return sb;
	}

	/// <summary>
	/// 生成文档注释（inheritdoc cref）并追加到StringBuilder
	/// </summary>
	/// <param name="sb">StringBuilder实例</param>
	/// <param name="symbol">要生成文档的符号（方法、属性、索引器、事件）</param>
	public StringBuilder GenerateDocumentationComment(StringBuilder sb, ISymbol symbol)
	{
		if (symbol is not (IMethodSymbol or IPropertySymbol or IEventSymbol))
		{
			return sb;
		}
		sb.Append("        /// <inheritdoc cref=\"");
		sb.Append(symbol.ContainingType.ToGlobalDisplayString().Replace("<", "{").Replace(">", "}"));
		sb.Append(".");
		switch (symbol)
		{
			case IMethodSymbol methodSymbol:
				sb.Append(symbol.Name);
				if (methodSymbol.IsGenericMethod)
				{
					sb.Append('{');
					sb.AppendJoin(",", methodSymbol.TypeParameters.Select(tp => tp.Name));
					sb.Append('}');
				}
				sb.Append('(');
				sb.AppendJoin(", ", methodSymbol.Parameters.Select(p =>
				{
					GetRefKind(sb, p);
					return p.Type.ToGlobalDisplayString().Replace("<", "{").Replace(">", "}");
				}));
				sb.Append(")");
				break;

			case IPropertySymbol { IsIndexer: true } propertySymbol:
				sb.Append("this[");
				sb.AppendJoin(",", propertySymbol.Parameters.Select(p => p.Type.ToGlobalDisplayString().Replace("<", "{").Replace(">", "}")));
				sb.Append("]");
				break;

			default:
				sb.Append(symbol.Name);
				break;
		}
		return sb.AppendLine("\"/>");
	}

	/// <summary>
	/// 生成扩展方法代码并追加到StringBuilder（不包含文档注释）
	/// </summary> 
	public StringBuilder GenerateExtensionMethod(StringBuilder sb, IMethodSymbol? methodSymbol, bool extensionMethod = true)
	{
		if (!IsAccessible(methodSymbol) || methodSymbol is null)
		{
			return sb;
		}
		if (extensionMethod)
		{
			GenerateDocumentationComment(sb, methodSymbol.AssociatedSymbol ?? methodSymbol);
		}

		// 开始方法声明
		sb.Append("        ");
		GetAccessibility(sb, methodSymbol.DeclaredAccessibility);
		if (extensionMethod)
		{
			sb.Append("static ");
		}
		sb.Append(methodSymbol.ReturnType.ToGlobalDisplayString());
		sb.Append(" ");
		sb.Append(methodSymbol.MethodKind switch
		{
			MethodKind.EventAdd => "add_",
			MethodKind.EventRemove => "remove_",
			MethodKind.PropertyGet => "get_",
			MethodKind.PropertySet => "set_",
			_ => null
		});
		sb.Append(MemberName);
		var types = methodSymbol.TypeParameters;
		if (methodSymbol.IsGenericMethod || ClassAnalyzer.InterfaceType!.IsGenericType)
		{
			if (extensionMethod)
			{
				types = ClassAnalyzer.GenericsWithContaining.Concat(types).ToImmutableArray();
			}
			if (types.Any())
			{
				sb.Append("<");
				sb.AppendJoin(", ", types.Select(p => p.Name));
				sb.Append(">");
			}
		}
		sb.Append("(");
		if (extensionMethod)
		{
			// 开始参数列表
			sb.Append("this ");
			sb.Append(methodSymbol.ContainingType.ToGlobalDisplayString());
			sb.Append(" ");
			sb.Append(InstanceParameterName);
			if (!methodSymbol.Parameters.IsEmpty)
			{
				sb.Append(", ");
			}
		}
		for (int i = 0; i < methodSymbol.Parameters.Length; i++)
		{
			var p = methodSymbol.Parameters[i];
			GetRefKind(sb, p);
			GetIsParams(sb, p);
			sb.Append(p.Type.ToGlobalDisplayString());
			sb.Append(" ");
			sb.Append(p.Name);
			GetDefaultValueString(sb, p);
			if (i < methodSymbol.Parameters.Length - 1)
			{
				sb.Append(", ");
			}
		}
		sb.AppendLine(")");


		// 添加泛型约束 
		foreach (var item in types)
		{
			GenerateConstraints(sb, item);
		}

		// 生成方法体
		sb.AppendLine("        {");
		sb.Append("            ");

		// 处理返回值
		if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Void)
		{
			sb.Append("return ");
		}
		// 方法调用
		sb.Append(InstanceParameterName);

		switch (methodSymbol.AssociatedSymbol)
		{
			case IPropertySymbol { IsIndexer: true }:
				sb.Append("[");
				var parameters = methodSymbol.Parameters;
				if (methodSymbol.MethodKind == MethodKind.PropertySet)
				{
					parameters = parameters.Take(parameters.Length - 1).ToImmutableArray();
				}
				sb.AppendJoin(", ", methodSymbol.Parameters.Select(p => p.Name));
				sb.Append("]");
				if (methodSymbol.MethodKind == MethodKind.PropertySet)
				{
					sb.Append(" = ");
					sb.Append("value");
				}
				break;

			case IPropertySymbol symbol:
				sb.Append(".");
				sb.Append(symbol.Name);

				if (methodSymbol.MethodKind == MethodKind.PropertySet)
				{
					sb.Append(" = ");
					sb.Append("value");
				}
				break;

			case IEventSymbol symbol:
				sb.Append(".");
				sb.Append(symbol.Name);
				sb.Append(" ");
				sb.Append(methodSymbol.MethodKind == MethodKind.EventAdd ? "+" : "-");
				sb.Append("= value");
				break;

			default:
				sb.Append(".");
				sb.Append(methodSymbol.Name);
				if (methodSymbol.IsGenericMethod)
				{
					sb.Append("<");
					sb.AppendJoin(", ", methodSymbol.TypeArguments.Select(ta => ta.ToGlobalDisplayString()));
					sb.Append(">");
				}
				sb.Append("(");
				sb.AppendJoin(", ", methodSymbol.Parameters.Select(p =>
				{
					GetRefKind(sb, p);
					return p.Name;
				}));
				sb.Append(")");
				break;
		}
		sb.AppendLine(";");
		return sb.AppendLine("        }");
	}

	/// <summary>
	/// 将Accessibility枚举转换为对应的访问修饰符字符串
	/// </summary> 
	public StringBuilder GetAccessibility(StringBuilder sb, Accessibility accessibility)
	{
		return sb.Append(accessibility switch
		{
			Accessibility.Public => "public ",
			Accessibility.Internal or Accessibility.ProtectedAndInternal => "internal ",
			_ => null
		});
	}

	/// <summary>
	/// 处理参数的修饰符
	/// </summary> 
	public StringBuilder GetRefKind(StringBuilder sb, IParameterSymbol parameter)
	{
		return sb.Append(parameter switch
		{
			{ RefKind: RefKind.Ref } => "ref ",
			{ RefKind: RefKind.Out } => "out ",
			{ RefKind: RefKind.In } => "in ",
			_ => null
		});
	}
	/// <summary>
	/// 处理参数的修饰符
	/// </summary> 
	public StringBuilder GetIsParams(StringBuilder sb, IParameterSymbol parameter)
	{
		return parameter.IsParams ? sb.Append("params ") : sb;
	}

	/// <summary>
	/// 获取参数的默认值字符串表示，避免访问性问题
	/// </summary> 
	public StringBuilder GetDefaultValueString(StringBuilder sb, IParameterSymbol parameter)
	{
		if (!parameter.HasExplicitDefaultValue)
		{
			return sb;
		}
		if (parameter.Type.TypeKind == TypeKind.Enum && parameter.ExplicitDefaultValue != null)
		{
			if (parameter.Type is INamedTypeSymbol enumType)
			{
				sb.Append(" = ");
				var matchingMembers = enumType.GetMembers().OfType<IFieldSymbol>().Where(f => parameter.ExplicitDefaultValue.Equals(f.ConstantValue)).ToImmutableArray();
				if (matchingMembers.Length == 1)
				{
					sb.Append(enumType.ToGlobalDisplayString());
					sb.Append(".");
					sb.Append(matchingMembers.First().Name);
				}
				else
				{
					sb.Append("(");
					sb.Append(enumType.ToGlobalDisplayString());
					sb.Append(")");
					sb.Append(Convert.ToInt64(parameter.ExplicitDefaultValue));
					if (matchingMembers.Length > 1)
					{
						sb.Append(" /* CA1069 */ ");
					}
				}
			}
			return sb;
		}
		sb.Append(" = ");
		sb.Append(parameter.ExplicitDefaultValue switch
		{
			null => "null",
			true => "true",
			false => "false",
			string s => $"@\"{s.Replace("\"", "\"\"")}\"",
			char c => $"\'{c}\'",
			float f => f.ToString(CultureInfo.InvariantCulture) + "f",
			double d => d.ToString(CultureInfo.InvariantCulture),
			decimal dec => dec.ToString(CultureInfo.InvariantCulture) + "m",
			_ => parameter.ExplicitDefaultValue.ToString()
		});
		return sb;
	}
}
