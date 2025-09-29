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
			.CreateSyntaxProvider<ImmutableArray<ClassAnalyzer>>(
				predicate: static (node, _) =>
					node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 } ||
					(node is ClassDeclarationSyntax { AttributeLists.Count: > 0, Modifiers.Count: > 0, Parent: CompilationUnitSyntax or BaseNamespaceDeclarationSyntax } classDecl &&
					 classDecl.Modifiers.Any(m => m.Text == "static")),
				transform: static (ctx, _) =>
				{
					try
					{
						if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol namedTypeSymbol)
						{
							return [];
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
								return [.. classAttributes
									.Select(attr => new ExtendWithInterfaceImplAnalyzer(namedTypeSymbol, attr, globalConfig))
									.Where(analyzer => analyzer.IsValid)];
						}
					}
					catch
					{
					}
					return [];
				}
			)
			.SelectMany((analyzers, _) => analyzers); // 展开所有分析器

		// 2. 注册生成逻辑
		context.RegisterSourceOutput(analyzers, static (context, classAnalyzer) =>
		{
			try
			{
				if (classAnalyzer.IsValid && classAnalyzer.ToString() is string sourceCode)
				{
					context.AddSource($"{classAnalyzer.ExtensionClassNamespace}.{classAnalyzer.ExtensionClassName}/{classAnalyzer.InterfaceType?.ToDisplayString().Replace('<', '{').Replace('>', '}')}.g.cs", sourceCode);
				}
			}
			catch (Exception ex)
			{
				context.AddSource($"Error_{Guid.NewGuid()}.g.cs", $"/* Error: {ex} */");
			}
		});
	}
}
/*class TrieNode
{
	public bool IsEnd { get; private set; }
	public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();

	public bool Add(ReadOnlySpan<char> span)
	{
		if (span.Length == 0)
		{
			var wasEnd = IsEnd;
			IsEnd = true;
			return !wasEnd;
		}
		else
		{
			if (!Children.TryGetValue(span[0], out var child))
			{
				child = new TrieNode();
				Children[span[0]] = child;
			}
			return child.Add(span.Slice(1));
		}
	}

	public bool Contains(ReadOnlySpan<char> span)
	{
		return span.Length == 0 ? IsEnd : Children.TryGetValue(span[0], out var child) && child.Contains(span.Slice(1));
	}
	/// <summary>
	/// 生成一个不在集合中的字符串
	/// </summary>
	/// <remarks>
	/// 从0-9A-Za-z进行遍历。如果不行，从00-0z，然后10-1z，直到z0-zz
	/// </remarks>
	public static string GenerateUniqueString(string prefix, ICollection<string> existingStrings)
	{
		if (existingStrings.Count == 0 || !existingStrings.Contains(prefix))
		{
			return prefix;
		}
		TrieNode? trieNode = default;
		Span<char> buffer = stackalloc char[prefix.Length + 6];//Math.Log( int.MaxValue ,62) = 5.2
		buffer.Fill('0');
		prefix.AsSpan().CopyTo(buffer);
		for (int i = 0; i < 6; i++)
		{
			ReadOnlySpan<char> currentSpan = buffer.Slice(0, prefix.Length + i + 1);
			Span<char> suffixSpan = buffer.Slice(prefix.Length, i + 1);
			int suffixPosition = i;
			do
			{
				bool equals = false;
				if (trieNode == null)
				{
					trieNode = new TrieNode();
					foreach (var item in existingStrings)
					{
						trieNode.Add(item);
						if (currentSpan.SequenceEqual(item))
						{
							equals = true;
						}
					}
				}
				else
				{
					equals = trieNode.Contains(currentSpan);
				}
				if (!equals)
				{
					return new string(currentSpan.ToArray());
				}
				switch (suffixSpan[suffixPosition])
				{
					case '9':
						suffixSpan[suffixPosition] = 'A';
						break;
					case 'Z':
						suffixSpan[suffixPosition] = 'a';
						break;
					case 'z':
						suffixSpan[suffixPosition] = '0';
						suffixPosition--;
						break;
					default:
						suffixSpan[suffixPosition] += (char)1;
						suffixPosition = i;
						break;
				}
			}
			while (suffixPosition >= 0);
		}
		return prefix;
	}
}*/