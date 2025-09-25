namespace zms9110750.InterfaceImplAsExtensionGenerator.SyntaxProcessors;
abstract class BaseAnalyzer
{
	/// <summary>
	/// 是否有效
	/// </summary>
	public abstract bool IsValid { get; }
	public abstract StringBuilder ToString(StringBuilder sb);
	public override string ToString()
	{
		if (!IsValid)
		{
			return "";
		}
		return ToString(new StringBuilder()).ToString();
	}

	// 检查成员是否可访问（包括静态和访问权限检查）
	public static bool IsAccessible(ISymbol? member)
	{
		return member is
		{
			IsImplicitlyDeclared: false,
			IsStatic: false,
			Kind: SymbolKind.Method or SymbolKind.Property or SymbolKind.Event,
			DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
		};
	}

	/// <summary>
	/// 生成泛型参数约束代码
	/// </summary>
	public static StringBuilder GenerateConstraints(StringBuilder sb, ITypeParameterSymbol parameter)
	{
		var constraintStrings = new List<string>();
		if (parameter.HasReferenceTypeConstraint)
		{
			constraintStrings.Add("class");
		}
		if (parameter.HasValueTypeConstraint)
		{
			constraintStrings.Add("struct");
		}
		if (parameter.HasUnmanagedTypeConstraint)
		{
			constraintStrings.Add("unmanaged");
		}
		if (parameter.HasNotNullConstraint)
		{
			constraintStrings.Add("notnull");
		}
		// 类型约束
		foreach (var constraintType in parameter.ConstraintTypes)
		{
			constraintStrings.Add(constraintType.ToGlobalDisplayString());
		}
		if (parameter.HasConstructorConstraint)
		{
			constraintStrings.Add("new()");
		}
		if (constraintStrings.Any())
		{
			sb.Append("             where ");
			sb.Append(parameter.Name);
			sb.Append(" : ");
			sb.AppendJoin(", ", constraintStrings);
			sb.AppendLine();
		}
		return sb;
	}
}
