using zms9110750.InterfaceImplAsExtensionGenerator.Config;

namespace zms9110750.InterfaceImplAsExtensionGenerator.SyntaxProcessors;


/// <summary>
/// 分析程序集级别的全局配置
/// </summary>
class GlobalConfigAnalyzer
{
	/// <summary>
	/// 生成的扩展类型名称后缀
	/// </summary>
	/// <remarks>
	/// 用于在自动生成扩展类时追加到原类型名后。
	/// 例如原类型为 ITest，后缀为"Extension"，则生成 TestExtension。
	/// </remarks>
	public string? TypeNameSuffix { get; }

	/// <summary>
	/// 命名空间追加字符串
	/// </summary>
	/// <remarks>
	/// 生成扩展类时追加到原命名空间后的字符串。
	/// 可为 null 或空字符串（空字符串表示使用原命名空间）。
	/// </remarks>
	public string? NamespaceSuffix { get; }

	/// <summary>
	/// 实例参数的默认名称
	/// </summary>
	/// <remarks>
	/// 扩展方法中表示实例的参数名称。
	/// 接口、成员特性可覆盖此值，未覆盖时使用此处配置。
	/// </remarks>
	public string InstanceParameterName { get; }

	/// <summary>
	/// 默认要生成的成员类型（按位枚举）
	/// </summary>
	/// <remarks>
	/// 全局默认生成的成员类型组合（如属性+方法）。
	/// 接口、成员特性可覆盖此值，未覆盖时使用此处配置。
	/// </remarks>
	public GenerateMembers DefaultGenerateMembers { get; }

	/// <summary>
	/// 是否使用旧语法（扩展方法形式）生成扩展
	/// </summary>
	/// <remarks>
	/// 为 true 时使用传统扩展方法语法；为 false 时使用新扩展块语法。
	/// 未设置时默认为 false（优先使用新语法）。
	/// </remarks>
	public bool UseLegacySyntax { get; }
	public bool UsePublic { get; }

	/// <summary>
	/// 初始化全局配置分析器
	/// </summary>
	/// <param name="assemblySymbol">程序集符号</param>
	/// <remarks>
	/// 从程序集特性中提取全局配置信息。
	/// 如果没有找到配置特性，则使用默认值。
	/// </remarks>
	public GlobalConfigAnalyzer(IAssemblySymbol assemblySymbol)
	{
		var attribute = assemblySymbol.GetAttributes()
			.FirstOrDefault(attr => attr.AttributeClass.EqualName<InterfaceImplAsExtensionGlobalAttribute>());

		if (attribute != null)
		{
			TypeNameSuffix = attribute.GetOrDefault(nameof(InterfaceImplAsExtensionGlobalAttribute.TypeNameSuffix), InterfaceImplAsExtensionGlobalAttribute.DefaultTypeNameSuffix);
			NamespaceSuffix = attribute.GetOrDefault<string>(nameof(InterfaceImplAsExtensionGlobalAttribute.NamespaceSuffix));
			InstanceParameterName = attribute.GetOrDefault(nameof(InterfaceImplAsExtensionGlobalAttribute.InstanceParameterName), InterfaceImplAsExtensionGlobalAttribute.DefaultInstanceParameterName) ?? InterfaceImplAsExtensionGlobalAttribute.DefaultInstanceParameterName;
			DefaultGenerateMembers = (GenerateMembers)attribute.GetOrDefault(nameof(InterfaceImplAsExtensionGlobalAttribute.DefaultGenerateMembers), (int)InterfaceImplAsExtensionGlobalAttribute.DefaultGenerateMembersValue);
			UseLegacySyntax = attribute.GetOrDefault(nameof(InterfaceImplAsExtensionGlobalAttribute.UseLegacySyntax), false);
			UsePublic = attribute.GetOrDefault(nameof(InterfaceImplAsExtensionGlobalAttribute.UsePublic), false);
		}
		else
		{
			// 使用默认值
			TypeNameSuffix = InterfaceImplAsExtensionGlobalAttribute.DefaultTypeNameSuffix;
			InstanceParameterName = InterfaceImplAsExtensionGlobalAttribute.DefaultInstanceParameterName;
			DefaultGenerateMembers = InterfaceImplAsExtensionGlobalAttribute.DefaultGenerateMembersValue;
			UseLegacySyntax = false;
			NamespaceSuffix = null;
		}
	}
}