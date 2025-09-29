namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;

/// <summary>
/// 类级别的扩展生成规则，在现有扩展类中追加接口的扩展成员
/// </summary>
/// <remarks>
/// 主要用于整合非当前程序集接口的扩展成员。未设置的属性继承接口级或全局配置。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExtendWithInterfaceImplAttribute(Type appendInterfaceType) : Attribute
{
	/// <summary>
	/// 要追加成员的接口类型（必填项）
	/// </summary>
	/// <remarks>
	/// 生成器将为该接口的成员生成扩展，并添加到当前标记的扩展类中。
	/// 构造函数强制要求传入，不可为null。
	/// </remarks>
	public Type AppendInterfaceType { get; set; } = appendInterfaceType;

	/// <summary>
	/// 实例参数的名称
	/// </summary>
	/// <remarks>
	/// 扩展中表示接口实例的参数名称。不可为null或空字符串。
	/// 未设置时继承程序集级或接口级的InstanceParameterName。
	/// </remarks>
	public string? InstanceParameterName { get; set; }

	/// <summary>
	/// 为追加的接口默认生成的成员类型（按位枚举）
	/// </summary>
	/// <remarks>
	/// 未设置时继承程序集级或接口级的DefaultGenerateMembers。
	/// </remarks>
	public GenerateMembers DefaultGenerateMembers { get; set; }
}