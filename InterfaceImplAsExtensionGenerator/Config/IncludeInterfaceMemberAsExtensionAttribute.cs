namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;
/// <summary>
/// 接口成员级别的扩展生成规则，控制单个成员是否生成扩展
/// </summary>
/// <remarks>
/// 作用于接口的具体成员（方法、属性等），未设置的属性继承接口级或全局配置。
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
public class IncludeInterfaceMemberAsExtensionAttribute : Attribute
{
	/// <summary>
	/// 生成的扩展成员的替代名称
	/// </summary>
	/// <remarks>
	/// 用于替换原成员名称的扩展成员名。不可为 null 或空字符串。
	/// 未设置时使用原成员名称。
	/// </remarks>
	public string? ReplacementMemberName { get; set; }

	/// <summary>
	/// 实例参数的名称（仅在旧语法中生效）
	/// </summary>
	/// <remarks>
	/// 扩展方法中表示实例的参数名称，仅在 UseLegacySyntax 为 true 时有效。不可为 null 或空字符串。
	/// 未设置时继承接口级或程序集级 InstanceParameterName。
	/// </remarks>
	public string? InstanceParameterName { get; set; }

	/// <summary>
	/// 强制控制成员是否生成（null 表示继承上级配置）
	/// </summary>
	/// <remarks>
	/// true 表示强制生成该成员，false 表示强制不生成，null 表示遵循接口级/程序集级的默认设置。
	/// </remarks>
	public bool? ForceGenerate { get; set; }
}
