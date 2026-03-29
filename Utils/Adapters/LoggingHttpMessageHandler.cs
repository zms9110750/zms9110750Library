using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace zms9110750.Utils.Adapters;
/// <summary>
/// 日志记录 HTTP 请求和响应的 DelegatingHandler
/// </summary>
public class LoggingHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    private readonly LoggingOptions _options;

    public LoggingHttpMessageHandler(ILoggerFactory loggerFactory, LoggingOptions? options = null)
    {
        _logger = loggerFactory.CreateLogger<LoggingHttpMessageHandler>();
        _options = options ?? new LoggingOptions();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // 请求日志（各自按级别打）
            await LogRequestAsync(request);

            var response = await base.SendAsync(request, cancellationToken);

            // 响应日志（各自按级别打）
            await LogResponseAsync(response, startTime);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP 请求异常: {Message}", ex.Message);
            throw;
        }
    }

    private async Task LogRequestAsync(HttpRequestMessage request)
    {
        // 请求行
        if (_options.RequestLineLevel != LogLevel.None && _logger.IsEnabled(_options.RequestLineLevel))
        {
            _logger.Log(_options.RequestLineLevel,
                "--> {Method} {Path} HTTP/{Version}",
                request.Method,
                request.RequestUri?.PathAndQuery,
                request.Version);
        }

        // 请求头
        if (_options.RequestHeadersLevel != LogLevel.None && _logger.IsEnabled(_options.RequestHeadersLevel))
        {
            foreach (var header in request.Headers)
            {
                var value = _options.SensitiveHeaders.Contains(header.Key)
                    ? "******"
                    : string.Join(", ", header.Value);
                _logger.Log(_options.RequestHeadersLevel, "{Key}: {Value}", header.Key, value);
            }
        }

        // 请求体
        if (_options.RequestBodyLevel != LogLevel.None &&
            _logger.IsEnabled(_options.RequestBodyLevel) &&
            request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                _logger.Log(_options.RequestBodyLevel, "Request Body:\n{Body}",
                    Truncate(content, _options.MaxBodyLength));
            }
        }
    }

    private async Task LogResponseAsync(HttpResponseMessage response, DateTime startTime)
    {
        var duration = DateTime.UtcNow - startTime;

        // 响应行
        if (_options.ResponseLineLevel != LogLevel.None && _logger.IsEnabled(_options.ResponseLineLevel))
        {
            var line = $"<-- {(int)response.StatusCode} {response.StatusCode}";
            if (_options.DurationLevel != LogLevel.None && _logger.IsEnabled(_options.DurationLevel))
                line += $" - {duration.TotalMilliseconds:F0}ms";

            _logger.Log(_options.ResponseLineLevel, "{Line}", line);
        }

        // 响应头（单独或和行一起？这里单独）
        if (_options.ResponseHeadersLevel != LogLevel.None && _logger.IsEnabled(_options.ResponseHeadersLevel))
        {
            foreach (var header in response.Headers)
                _logger.Log(_options.ResponseHeadersLevel, "{Key}: {Value}", header.Key, string.Join(", ", header.Value));
            foreach (var header in response.Content.Headers)
                _logger.Log(_options.ResponseHeadersLevel, "{Key}: {Value}", header.Key, string.Join(", ", header.Value));
        }

        // 响应体
        if (_options.ResponseBodyLevel != LogLevel.None &&
            _logger.IsEnabled(_options.ResponseBodyLevel) &&
            response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                _logger.Log(_options.ResponseBodyLevel, "Response Body:\n{Body}",
                    Truncate(content, _options.MaxBodyLength));
            }
        }
    }

    private static string Truncate(string input, int maxLength)
    {
        if (input.Length <= maxLength)
            return input;
        return input[..maxLength] + "... (truncated)";
    }
}
public class LoggingOptions
{
    /// <summary>
    /// 请求行日志级别（默认 Debug）
    /// </summary>
    public LogLevel RequestLineLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 请求头日志级别（默认 Debug）
    /// </summary>
    public LogLevel RequestHeadersLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 请求体日志级别（默认 Debug）
    /// </summary>
    public LogLevel RequestBodyLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 响应行日志级别（默认 Debug）
    /// </summary>
    public LogLevel ResponseLineLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 响应头日志级别（默认 Debug）
    /// </summary>
    public LogLevel ResponseHeadersLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 响应体日志级别（默认 Debug）
    /// </summary>
    public LogLevel ResponseBodyLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 耗时日志级别（默认 Debug）
    /// </summary>
    public LogLevel DurationLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 敏感头列表（始终记录但隐藏值）
    /// </summary>
    public HashSet<string> SensitiveHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "Set-Cookie", "Proxy-Authorization"
    };

    /// <summary>
    /// 请求/响应体最大显示长度
    /// </summary>
    public int MaxBodyLength { get; set; } = 2000;

    /// <summary>
    /// 批量设置所有日志级别
    /// </summary>
    public void SetAllLevels(LogLevel level)
    {
        RequestLineLevel = level;
        RequestHeadersLevel = level;
        RequestBodyLevel = level;
        ResponseLineLevel = level;
        ResponseHeadersLevel = level;
        ResponseBodyLevel = level;
        DurationLevel = level;
    }

}