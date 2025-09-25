using zms9110750.InterfaceImplAsExtensionGenerator.Config;

namespace zms9110750.InterfaceImplAsExtensionGenerator.SyntaxProcessors;


/// <summary>
/// �������򼯼����ȫ������
/// </summary>
class GlobalConfigAnalyzer
{
	/// <summary>
	/// ���ɵ���չ�������ƺ�׺
	/// </summary>
	/// <remarks>
	/// �������Զ�������չ��ʱ׷�ӵ�ԭ��������
	/// ����ԭ����Ϊ ITest����׺Ϊ"Extension"�������� TestExtension��
	/// </remarks>
	public string? TypeNameSuffix { get; }

	/// <summary>
	/// �����ռ�׷���ַ���
	/// </summary>
	/// <remarks>
	/// ������չ��ʱ׷�ӵ�ԭ�����ռ����ַ�����
	/// ��Ϊ null ����ַ��������ַ�����ʾʹ��ԭ�����ռ䣩��
	/// </remarks>
	public string? NamespaceSuffix { get; }

	/// <summary>
	/// ʵ��������Ĭ������
	/// </summary>
	/// <remarks>
	/// ��չ�����б�ʾʵ���Ĳ������ơ�
	/// �ӿڡ���Ա���Կɸ��Ǵ�ֵ��δ����ʱʹ�ô˴����á�
	/// </remarks>
	public string InstanceParameterName { get; }

	/// <summary>
	/// Ĭ��Ҫ���ɵĳ�Ա���ͣ���λö�٣�
	/// </summary>
	/// <remarks>
	/// ȫ��Ĭ�����ɵĳ�Ա������ϣ�������+��������
	/// �ӿڡ���Ա���Կɸ��Ǵ�ֵ��δ����ʱʹ�ô˴����á�
	/// </remarks>
	public GenerateMembers DefaultGenerateMembers { get; }

	/// <summary>
	/// �Ƿ�ʹ�þ��﷨����չ������ʽ��������չ
	/// </summary>
	/// <remarks>
	/// Ϊ true ʱʹ�ô�ͳ��չ�����﷨��Ϊ false ʱʹ������չ���﷨��
	/// δ����ʱĬ��Ϊ false������ʹ�����﷨����
	/// </remarks>
	public bool UseLegacySyntax { get; }

	/// <summary>
	/// ��ʼ��ȫ�����÷�����
	/// </summary>
	/// <param name="assemblySymbol">���򼯷���</param>
	/// <remarks>
	/// �ӳ�����������ȡȫ��������Ϣ��
	/// ���û���ҵ��������ԣ���ʹ��Ĭ��ֵ��
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
		}
		else
		{
			// ʹ��Ĭ��ֵ
			TypeNameSuffix = InterfaceImplAsExtensionGlobalAttribute.DefaultTypeNameSuffix;
			InstanceParameterName = InterfaceImplAsExtensionGlobalAttribute.DefaultInstanceParameterName;
			DefaultGenerateMembers = InterfaceImplAsExtensionGlobalAttribute.DefaultGenerateMembersValue;
			UseLegacySyntax = false;
			NamespaceSuffix = null;
		}
	}
}