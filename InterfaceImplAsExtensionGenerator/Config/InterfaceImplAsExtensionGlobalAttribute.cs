namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;
/// <summary>
/// 程序集级别的扩展生成规则，设置全局默认行为
/// </summary>
/// <remarks>
/// 作为根级规则，未被接口/成员特性覆盖的配置将使用此处的值。
/// TypeNameSuffix 和 InstanceParameterName 为必填项，不可为 null 或空字符串。
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public class InterfaceImplAsExtensionGlobalAttribute : Attribute
{
	/// <summary>
	/// 生成的扩展类型名称后缀
	/// </summary>
	/// <remarks>
	/// 用于在自动生成扩展类时追加到原类型名后。例如原类型为 ITest，后缀为"Extension"，则生成 TestExtension。
	/// 不可为 null 或空字符串，根级别默认值为"Extension"。
	/// </remarks>
	public string? TypeNameSuffix { get; set; } = DefaultTypeNameSuffix;

	/// <summary>
	/// 命名空间追加字符串
	/// </summary>
	/// <remarks>
	/// 生成扩展类时追加到原命名空间后的字符串。可为 null 或空字符串（空字符串表示使用原命名空间）。
	/// 未设置时使用原命名空间，不追加任何内容。
	/// </remarks>
	public string? NamespaceSuffix { get; set; }

	/// <summary>
	/// 实例参数的默认名称
	/// </summary>
	/// <remarks>
	/// 扩展方法中表示实例的参数名称。不可为 null 或空字符串，根级别默认值为"instance"。
	/// 接口、成员特性可覆盖此值，未覆盖时使用此处配置。
	/// </remarks>
	public string? InstanceParameterName { get; set; } = DefaultInstanceParameterName;

	/// <summary>
	/// 默认要生成的成员类型（按位枚举）
	/// </summary>
	/// <remarks>
	/// 全局默认生成的成员类型组合（如属性+方法）。未设置时默认为属性和方法的组合。
	/// 接口、成员特性可覆盖此值，未覆盖时使用此处配置。
	/// </remarks>
	public GenerateMembers DefaultGenerateMembers { get; set; } = DefaultGenerateMembersValue;

	/// <summary>
	/// 是否使用旧语法（扩展方法形式）生成扩展
	/// </summary>
	/// <remarks>
	/// 为 true 时使用传统扩展方法语法；为 false 时使用新扩展块语法。
	/// 未设置时默认为 false（优先使用新语法）。
	/// </remarks>
	public bool UseLegacySyntax { get; set; }

	internal const string DefaultTypeNameSuffix = "Extension";
	internal const string DefaultInstanceParameterName = "instance";
	internal const GenerateMembers DefaultGenerateMembersValue = GenerateMembers.Property | GenerateMembers.Method;
}
