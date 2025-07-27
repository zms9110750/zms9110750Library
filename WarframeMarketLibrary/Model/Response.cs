using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketLibrary.Model;
/// <summary>
/// V2 API响应模型
/// </summary>
/// <param name="ApiVersion">API版本</param>
/// <param name="Data">数据</param>
/// <param name="Error">错误</param>
public record Response<T>(
	string ApiVersion,
	T Data,
	string? Error);