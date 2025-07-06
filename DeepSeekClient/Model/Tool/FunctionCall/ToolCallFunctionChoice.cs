namespace zms9110750.DeepSeekClient.Model.Tool.FunctionCall;

/// <summary>
/// 函数调用请求内容
/// </summary>
/// <param name="Name">函数名</param>
/// <param name="Arguments">参数</param>
public record ToolCallFunctionChoice(string Name, string Arguments);


