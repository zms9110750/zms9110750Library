#define D1
//创建一个连接器
using System.Runtime.CompilerServices;
using System.Text.Json;

#if D1
#else
using System.Text.Json;
using zms9110750.DeepSeekClient;
using zms9110750.DeepSeekClient.Beta;
using zms9110750.DeepSeekClient.Model.Messages;
using zms9110750.DeepSeekClient.Model.Tool;
using zms9110750.DeepSeekClient.ModelDelta.Response;





var config = new ConfigurationBuilder()
				 .AddUserSecrets<Program>()
				 .AddYamlFile("C:\\Users\\16229\\Desktop\\新建文本文档 (2).yaml")
				 .Build();


var arcaneConfig = config.GetSection("ArcanePackage").Get<Dictionary<string, Dictionary<string, Rarity>>>(); // 自动绑定整个配置
var sb = config.GetSection("ArcanePackage:科维兽:Uncommon").Get<A>(); // 自动绑定整个配置

Console.WriteLine(sb.Items?.Count() ?? -1);

record A(double Quality, string[] Items);




public class Rarity
{
	public double Quality { get; set; }
	public List<string> Items { get; set; }
}





#endif

/*
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>netstandard2.0</TargetFramework>
		<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
		<LangVersion>preview</LangVersion>
		<Nullable>enable</Nullable>

		<!-- 分析器必须配置 -->
		<IsRoslynAnalyzer>true</IsRoslynAnalyzer>
		<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>

		<!-- 允许消费项目引用API -->
		<IncludeBuildOutput>true</IncludeBuildOutput>
		<RootNamespace>zms9110750.$(MSBuildProjectName.Replace(" ", "_"))</RootNamespace>
		<AssemblyName>zms9110750.$(MSBuildProjectName)</AssemblyName>
		<GenerateDocumentationFile>True</GenerateDocumentationFile>
		<GeneratePackageOnBuild>True</GeneratePackageOnBuild>
		<Authors>zms9110750</Authors>
		<PackageProjectUrl>https://github.com/zms9110750/zms9110750Library</PackageProjectUrl>
		<RepositoryUrl>https://github.com/zms9110750/zms9110750Library</RepositoryUrl>
		<PackageReadmeFile>Readme.md</PackageReadmeFile>
		<PackageLicenseExpression>MIT</PackageLicenseExpression> 
		<Version>0.1.0</Version>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" PrivateAssets="all" />
		<PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.11.0" PrivateAssets="all" />
	</ItemGroup>
	 
	<ItemGroup>
	  <None Update="Readme.md">
	    <PackagePath>\</PackagePath>
	    <Pack>True</Pack>
	  </None>
	</ItemGroup>
</Project>

*/




public static class Bug
{
	public static T Debug<T>(this T obj, [CallerMemberName] string? paramName = null, [CallerLineNumber] int? lineNum = null, [CallerFilePath] string? filePath = null)
	{
#if DEBUG
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"{Path.GetFileName(filePath)}  {lineNum}  {paramName}调用 ============ \n");
		Console.ForegroundColor = ConsoleColor.Black;
		Console.WriteLine(obj);
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"\n{paramName}结束===============================\n");
		Console.ForegroundColor = ConsoleColor.Black;
#endif
		return obj;
	}

	public static JsonSerializerOptions WriteIndented(this JsonSerializerOptions options)
	{
		return new JsonSerializerOptions(options) { WriteIndented = true };
	}
}
