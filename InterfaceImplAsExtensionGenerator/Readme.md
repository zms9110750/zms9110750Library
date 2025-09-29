# 简介

这个包可以为接口API生成扩展方法。

让显式实现的方法也可以调用。


## 主要API
引入命名空间`zms9110750.InterfaceImplAsExtensionGenerator.Config`

### 程序集托底配置
```csharp
[assembly: InterfaceImplAsExtensionGlobal(UseLegacySyntax = false)]
```

成员|效果|默认|备注
-|-|-|-
`TypeNameSuffix`|生成的扩展类型名称后缀|`Extension`|
`NamespaceSuffix`|命名空间追加字符串||追加的字符串会作为子命名空间
`InstanceParameterName`|实例参数的默认名称|`instance`|
`DefaultGenerateMembers`|默认要生成的成员类型| `Property | Method`|目前新语法不支持索引器和事件。在支持的时候可以自行启用
`UseLegacySyntax`|是否使用旧语法|`false`|启用老语法，会全部以扩展方法形式生成。方法名和新语法一样。


### 接口扩展特性
```csharp
[InterfaceImplAsExtension(DefaultGenerateMembers = GenerateMembers.Method)]
interface IHello
{
}
```
成员|效果|备注
-|-|-
`ExtensionClassName`|扩展类的名称|指定名称而不是后缀
`ExtensionClassNamespace`|扩展类所在的命名空间|指定名称而不是后缀。可以为`null`
`InstanceParameterName`|实例参数的名称|
`DefaultGenerateMembers`|为当前接口默认生成的成员类型|

### 成员微调特性
```csharp
	[IncludeInterfaceMemberAsExtension(ReplacementMemberName = "GetSet")]
	string GetSet { get; set; }
```
成员|效果|备注
-|-|-
`ReplacementMemberName`|生成的扩展成员的替代名称|
`InstanceParameterName`|实例参数的名称|仅在旧语法的扩展方法中有效
`ForceGenerate`|强制控制成员是否生成|`null`为根据配置，`true`和`false`为强制生成/不生成

### 静态类附加接口扩展特性
```csaharp
[ExtendWithInterfaceImpl(typeof(IList<int>))]
[ExtendWithInterfaceImpl(typeof(ISet<int>))]
public static partial class ClassExtensions
{
}
```
成员|效果|备注
-|-|-
`AppendInterfaceType`|要追加成员的接口类型|在构造器中必填
`InstanceParameterName`|实例参数的名称|
`DefaultGenerateMembers`|为追加的接口默认生成的成员类型|

### 生成类型枚举
```csharp
GenerateMembers.Property| GenerateMembers.Method
```

- `Property`:生成属性的扩展
- `Method`:生成方法的扩展
- `Indexer`:生成索引器的扩展
- `Event`:生成事件的扩展



 
## 示例

```csharp
interface IHello
{
	string GetSet { get; set; }
	string GetInit { get; init; }
	string PropertyGet { protected get; set; }
	int this[string index] { get; }
	void Hello();
	string Hello(string t);
	public void Hello<T1, T2, T3>(out T1 t)
		where T1 : class, IList<int>, new()
		where T2 : struct, ISet<int>
		where T3 : class, IList<int>, new();

	void Hello(ref string t);

	void Collection(ref int a, int b = 10, string s = "hello\t" + @"  {你好}""{哈哈}  ", bool d = true | true ^ true, float f = 34, Color r = Color.Red, params int[] p);
}
```
生成：
```csharp
static partial class IHelloExtension
{
    extension(global::IHello instance)
    {
        /// <inheritdoc cref="global::IHello.GetSet"/>
        public string GetSet
        {
            get => instance.GetSet;
            set => instance.GetSet = value;
        }
        /// <inheritdoc cref="global::IHello.GetInit"/>
        public string GetInit
        {
            get => instance.GetInit;
        }
        /// <inheritdoc cref="global::IHello.PropertyGet"/>
        public string PropertyGet
        {
            set => instance.PropertyGet = value;
        }
        /// <inheritdoc cref="global::IHello.Hello()"/>
        public void Hello()
        {
            instance.Hello();
        }
        /// <inheritdoc cref="global::IHello.Hello(string)"/>
        public string Hello(string t)
        {
            return instance.Hello(t);
        }
        /// <inheritdoc cref="global::IHello.Hello{T1,T2,T3}(out T1)"/>
        public void Hello<T1, T2, T3>(out T1 t)
             where T1 : class, global::System.Collections.Generic.IList<int>, new()
             where T2 : struct, global::System.Collections.Generic.ISet<int>
             where T3 : class, global::System.Collections.Generic.IList<int>, new()
        {
            instance.Hello<T1, T2, T3>(out t);
        }
        /// <inheritdoc cref="global::IHello.Hello(ref string)"/>
        public void Hello(ref string t)
        {
            instance.Hello(ref t);
        }
        /// <inheritdoc cref="global::IHello.Collection(ref int, int, string, bool, float, global::Color, int[])"/>
        public void Collection(ref int a, int b = 10, string s = @"hello	  {你好}""{哈哈}  ", bool d = true, float f = 34f, global::Color r = (global::Color)1 /* CA1069 */ , params int[] p)
        {
            instance.Collection(ref a, b, s, d, f, r, p);
        }
    }
}
```












