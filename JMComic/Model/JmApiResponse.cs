

namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示API的通用响应格式
/// </summary>
/// <typeparam name="T">响应数据的类型</typeparam>
/// <param name="Code">响应状态码，200表示成功</param>
/// <param name="Message">响应消息，成功时为null或空字符串，失败时为错误信息</param>
/// <param name="Data">响应数据，成功时包含请求的数据，失败时为null</param>
/// <remarks>
/// API响应是禁漫天堂移动端API的标准返回格式。
/// 所有移动端API调用都返回此格式的JSON响应。
/// 网页端API可能使用不同的响应格式。
/// </remarks>
public record JmApiResponse<T>(
    int Code,
    string? Message,
    T? Data
);