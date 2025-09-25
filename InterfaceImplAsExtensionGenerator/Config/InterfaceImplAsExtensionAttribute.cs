namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;

/// <summary>
/// 接口级别的扩展生成规则，为特定接口定制生成行为
/// </summary>
/// <remarks>
/// 仅作用于当前接口，未设置的属性继承程序集级全局配置。
/// </remarks>
[AttributeUsage(AttributeTargets.Interface)]
public class InterfaceImplAsExtensionAttribute : Attribute
{
	/// <summary>
	/// 扩展类的名称
	/// </summary>
	/// <remarks>
	/// 为当前接口生成的扩展类的具体名称。不可为 null 或空字符串。
	/// 未设置时，使用接口名 + 程序集级 TypeNameSuffix 生成。
	/// </remarks>
	public string? ExtensionClassName { get; set; }

	/// <summary>
	/// 扩展类所在的命名空间
	/// </summary>
	/// <remarks>
	/// 生成的扩展类所属的命名空间。可为 null 或空字符串（空字符串表示全局命名空间）。
	/// 未设置时，使用接口原命名空间 + 程序集级 NamespaceSuffix 生成。
	/// </remarks>
	public string? ExtensionClassNamespace { get; set; }

	/// <summary>
	/// 实例参数的名称
	/// </summary>
	/// <remarks>
	/// 扩展中表示接口实例的参数名称。不可为 null 或空字符串。
	/// 未设置时继承程序集级 InstanceParameterName。
	/// </remarks>
	public string? InstanceParameterName { get; set; }

	/// <summary>
	/// 为当前接口默认生成的成员类型（按位枚举）
	/// </summary>
	/// <remarks>
	/// 为当前接口生成的成员类型组合。未设置时继承程序集级 DefaultGenerateMembers。
	/// </remarks>
	public GenerateMembers DefaultGenerateMembers { get; set; }
}
