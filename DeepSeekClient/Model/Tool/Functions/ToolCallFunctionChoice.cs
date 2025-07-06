using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Tool.Functions;

/// <summary>
/// 函数调用请求内容
/// </summary>
/// <param name="Name">函数名</param>
/// <param name="Arguments">参数。如果内容完整，应该是一个jsonObject</param>
public record ToolCallFunctionChoice(string Name, string Arguments);