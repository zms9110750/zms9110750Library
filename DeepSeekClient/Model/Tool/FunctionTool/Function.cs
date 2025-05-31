using System.ComponentModel;

namespace zms9110750.DeepSeekClient.Model.Tool.FunctionTool;
/// <summary>
/// 函数工具描述
/// </summary>
/// <param name="Name">函数名</param>
/// <param name="Description">函数介绍。通过<see cref="DescriptionAttribute"/>特性来获取。</param>
/// <param name="Parameters">参数列表。参数描述也通过<see cref="DescriptionAttribute"/>特性来获取。</param>

public record Function(string Name, string? Description, Parameter? Parameters);
