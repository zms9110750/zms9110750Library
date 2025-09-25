namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;


/// <summary>
/// 按位枚举，表示可生成的扩展成员类型
/// </summary>
/// <remarks>
/// 支持组合使用（如 Property | Method 表示同时生成属性和方法）。
/// 其中 Indexer 和 Event 目前暂不支持，但预留枚举值以应对未来扩展。
/// </remarks>
[Flags]
public enum GenerateMembers
{
	/// <summary>
	/// 继承上级配置或使用默认值
	/// </summary>
	InheritOrDefault = 0,

	/// <summary>
	/// 生成属性的扩展
	/// </summary>
	Property = 1 << 0,

	/// <summary>
	/// 生成方法的扩展
	/// </summary>
	Method = 1 << 1,

	/// <summary>
	/// 生成索引器的扩展
	/// </summary>
	Indexer = 1 << 2,

	/// <summary>
	/// 生成事件的扩展
	/// </summary>
	Event = 1 << 3
}