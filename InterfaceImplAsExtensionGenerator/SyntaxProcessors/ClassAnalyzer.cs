using zms9110750.InterfaceImplAsExtensionGenerator.Config;

namespace zms9110750.InterfaceImplAsExtensionGenerator.SyntaxProcessors;

abstract class ClassAnalyzer(GlobalConfigAnalyzer globalConfig) : BaseAnalyzer
{
	/// <summary>
	/// 全局配置分析器
	/// </summary>
	public GlobalConfigAnalyzer GlobalConfig { get; } = globalConfig;

	/// <summary>
	/// 要扩展的接口类型
	/// </summary>
	public abstract INamedTypeSymbol? InterfaceType { get; }

	/// <summary>
	/// 特性定义的实例参数的名称
	/// </summary>
	protected abstract string? AttributeInstanceParameterName { get; }

	/// <summary>
	/// 特性定义的默认生成的成员类型
	/// </summary>
	protected abstract GenerateMembers AttributeDefaultGenerateMembers { get; }
	/// <summary>
	/// 特性定义的扩展类的名称
	/// </summary>
	protected abstract string? AttributeExtensionClassName { get; }
	/// <summary>
	/// 特性定义的扩展类所在的命名空间
	/// </summary>
	protected abstract string? AttributeExtensionClassNamespace { get; }

	/// <summary>
	/// 实例参数的名称
	/// </summary>
	public string InstanceParameterName => AttributeInstanceParameterName ?? GlobalConfig.InstanceParameterName!;

	/// <summary>
	/// 默认生成的成员类型
	/// </summary>
	public GenerateMembers DefaultGenerateMembers => AttributeDefaultGenerateMembers == default ? GlobalConfig.DefaultGenerateMembers : AttributeDefaultGenerateMembers;
	/// <summary>
	/// 扩展类的名称
	/// </summary>
	public string ExtensionClassName => AttributeExtensionClassName ?? InterfaceType!.Name + GlobalConfig.TypeNameSuffix;
	/// <summary>
	/// 扩展类所在的命名空间
	/// </summary>
	public string ExtensionClassNamespace
	{
		get
		{
			var space = AttributeExtensionClassNamespace;
			if (space != null)
			{
				return space == "<global namespace>" ? "" : space;
			}
			space = InterfaceType!.ContainingNamespace.ToDisplayString();
			if (space == "<global namespace>")
			{
				return "" + GlobalConfig.NamespaceSuffix;
			}
			else if (!string.IsNullOrEmpty(GlobalConfig.NamespaceSuffix))
			{
				return space + "." + GlobalConfig.NamespaceSuffix;
			}
			return space;
		}
	}

	protected ClassAnalyzer(ISymbol symbol) : this(new GlobalConfigAnalyzer(symbol.ContainingAssembly))
	{
	}
	public override StringBuilder ToString(StringBuilder code)
	{
		code ??= new StringBuilder();
		// 处理命名空间
		if (!string.IsNullOrEmpty(ExtensionClassNamespace) && ExtensionClassNamespace != "<global namespace>")
		{
			code.Append("namespace ").Append(ExtensionClassNamespace);
			if (GlobalConfig.UseLegacySyntax)
			{
				code.AppendLine();
				code.AppendLine("{");
			}
			else
			{
				code.AppendLine(";");
				code.AppendLine();
			}
		}
		code.Append("static partial class ");
		code.AppendLine(ExtensionClassName);
		code.AppendLine("{");

		// 处理新语法：生成扩展块
		if (!GlobalConfig.UseLegacySyntax)
		{
			var typeParameters = InterfaceType!.TypeArguments.OfType<ITypeParameterSymbol>().ToImmutableArray();

			code.Append("    extension");
			if (typeParameters.Any())
			{
				code.Append("<");
				code.AppendJoin(", ", typeParameters.Select(arg => arg.Name));
				code.Append(">");
			}
			code.Append("(");
			code.Append(InterfaceType.ToGlobalDisplayString());
			code.Append(" ");
			code.Append(InstanceParameterName);
			code.AppendLine(")");
			foreach (var item in typeParameters)
			{
				GenerateConstraints(code, item);
			}
			code.AppendLine("    {");
		}

		// 遍历所有成员并生成代码
		foreach (var member in InterfaceType!.GetMembers().Where(IsAccessible))
		{
			new MemberAnalyzer(member, this).ToString(code);
		}

		// 处理新语法：关闭扩展块
		if (!GlobalConfig.UseLegacySyntax)
		{
			code.AppendLine("    }");
		}

		code.AppendLine("}");

		// 关闭命名空间（旧语法）
		if (GlobalConfig.UseLegacySyntax && !string.IsNullOrEmpty(ExtensionClassNamespace) && ExtensionClassNamespace != "<global namespace>")
		{
			code.AppendLine("}");
		}
		return code;
	}
}

class InterfaceImplAnalyzer : ClassAnalyzer
{
	public InterfaceImplAnalyzer(INamedTypeSymbol typeSymbol, AttributeData? classAttribute = null, GlobalConfigAnalyzer? globalConfig = null)
		: base(globalConfig ?? new GlobalConfigAnalyzer(typeSymbol.ContainingAssembly))
	{
		InterfaceType = typeSymbol;
		classAttribute ??= InterfaceType.GetAttributes().FirstOrDefault(a => a.AttributeClass!.EqualName<InterfaceImplAsExtensionAttribute>());
		if (InterfaceType == null || classAttribute == null)
		{
			return;
		}
		IsValid = true;
		AttributeInstanceParameterName = classAttribute.GetOrDefault<string>(nameof(InterfaceImplAsExtensionAttribute.InstanceParameterName));
		AttributeDefaultGenerateMembers = classAttribute.GetOrDefault<GenerateMembers>(nameof(InterfaceImplAsExtensionAttribute.DefaultGenerateMembers));
		AttributeExtensionClassName = classAttribute.GetOrDefault<string>(nameof(InterfaceImplAsExtensionAttribute.ExtensionClassName));
		AttributeExtensionClassNamespace = classAttribute.GetOrDefault<string>(nameof(InterfaceImplAsExtensionAttribute.ExtensionClassNamespace));
	}
	public override INamedTypeSymbol? InterfaceType { get; }
	public override bool IsValid { get; }
	protected override string? AttributeInstanceParameterName { get; }
	protected override GenerateMembers AttributeDefaultGenerateMembers { get; }
	protected override string? AttributeExtensionClassName { get; }
	protected override string? AttributeExtensionClassNamespace { get; }
}
class ExtendWithInterfaceImplAnalyzer : ClassAnalyzer
{
	public ExtendWithInterfaceImplAnalyzer(INamedTypeSymbol typeSymbol, AttributeData classAttribute, GlobalConfigAnalyzer? globalConfig = null)
		: base(globalConfig ?? new GlobalConfigAnalyzer(typeSymbol.ContainingAssembly))
	{
		if (!classAttribute.AttributeClass.EqualName<ExtendWithInterfaceImplAttribute>())
		{
			return;
		}
		AttributeInstanceParameterName = classAttribute.GetOrDefault<string>(nameof(ExtendWithInterfaceImplAttribute.InstanceParameterName));
		AttributeDefaultGenerateMembers = classAttribute.GetOrDefault<GenerateMembers>(nameof(ExtendWithInterfaceImplAttribute.DefaultGenerateMembers));
		AttributeExtensionClassName = typeSymbol.Name;
		AttributeExtensionClassNamespace = typeSymbol.ContainingNamespace.ToDisplayString();

		if (classAttribute is { ConstructorArguments: { IsEmpty: false } constructor }
		&& constructor[0].Value is INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceType)
		{
			InterfaceType = interfaceType.IsUnboundGenericType ? interfaceType.ConstructedFrom : interfaceType;
			IsValid = true;
		}
	}
	public override INamedTypeSymbol? InterfaceType { get; }
	public override bool IsValid { get; }
	protected override string? AttributeInstanceParameterName { get; }
	protected override GenerateMembers AttributeDefaultGenerateMembers { get; }
	protected override string? AttributeExtensionClassName { get; }
	protected override string? AttributeExtensionClassNamespace { get; }
}