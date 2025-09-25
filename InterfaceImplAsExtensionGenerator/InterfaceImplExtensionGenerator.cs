using zms9110750.InterfaceImplAsExtensionGenerator.Config;

namespace zms9110750.InterfaceImplAsExtensionGenerator;
[Generator]
class InterfaceExtensionGenerator : IIncrementalGenerator
{
	// 使用线程安全的字典缓存全局配置
	private static ConcurrentDictionary<IAssemblySymbol, GlobalConfigAnalyzer> GlobalConfigCache { get; } =
		new ConcurrentDictionary<IAssemblySymbol, GlobalConfigAnalyzer>(SymbolEqualityComparer.Default);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// 1. 筛选出所有具有接口扩展特性，和类扩展特性的接口和类
		var analyzers = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (node, _) =>
					node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 } ||
					(node is ClassDeclarationSyntax { AttributeLists.Count: > 0, Modifiers.Count: > 0 } classDecl &&
					 classDecl.Modifiers.Any(m => m.Text == "static")),
				transform: static (ctx, _) =>
				{
					try
					{
						if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol namedTypeSymbol)
						{
							return ImmutableArray<ClassAnalyzer>.Empty;
						}

						var globalConfig = GlobalConfigCache.GetOrAdd(namedTypeSymbol.ContainingAssembly, assemblySymbol => new GlobalConfigAnalyzer(assemblySymbol));

						switch (ctx.Node)
						{
							case InterfaceDeclarationSyntax when namedTypeSymbol.GetAttributes()
								.FirstOrDefault(a => a.AttributeClass.EqualName<InterfaceImplAsExtensionAttribute>())
								is AttributeData interfaceAttribute
							:
								var analyzer = new InterfaceImplAnalyzer(namedTypeSymbol, interfaceAttribute, globalConfig);
								return analyzer.IsValid ? [analyzer] : [];

							case ClassDeclarationSyntax classDecl when namedTypeSymbol.GetAttributes()
								.Where(a => a.AttributeClass.EqualName<ExtendWithInterfaceImplAttribute>())
								.ToImmutableArray() is { IsEmpty: false } classAttributes
							:

								// 为每个类特性创建一个分析器
								return classAttributes
									.Select(attr => new ExtendWithInterfaceImplAnalyzer(namedTypeSymbol, attr, globalConfig))
									.Where(analyzer => analyzer.IsValid)
									.ToImmutableArray<ClassAnalyzer>();
						}
					}
					catch
					{
					}
					return ImmutableArray<ClassAnalyzer>.Empty;
				}
			)
			.SelectMany((analyzers, _) => analyzers); // 展开所有分析器

		// 2. 注册生成逻辑
		context.RegisterSourceOutput(analyzers, static (context, analyzer) =>
		{
			try
			{
				if (analyzer.IsValid && analyzer.ToString() is string sourceCode)
				{
					var dir = analyzer.ExtensionClassNamespace;
					if (!string.IsNullOrEmpty(dir))
					{
						dir = dir.Replace(".", "/") + "/";
					}
					string suffix = "";
					var type = analyzer.InterfaceType!.TypeArguments;
					if (type.Length > 0)
					{
						suffix = "{" + string.Join(", ", type.Select(t => t.Name)) + "}";
					}

					switch (analyzer)
					{
						case InterfaceImplAnalyzer:
							context.AddSource($"{dir}{analyzer.InterfaceType.Name}{suffix}.g.cs", sourceCode);
							break;
						case ExtendWithInterfaceImplAnalyzer:
							context.AddSource($"{dir}{analyzer.ExtensionClassName}.{analyzer.InterfaceType.Name}{suffix}.g.cs", sourceCode);
							break;
						default:
							context.AddSource($"Unknown_{Guid.NewGuid()}.g.cs", "/*" + sourceCode + "*/");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				context.AddSource($"Error_{Guid.NewGuid()}.g.cs", $"/* Error: {ex} */");
			}
		});
	}
}