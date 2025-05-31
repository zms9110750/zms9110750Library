using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.RateLimiting;

public sealed class RateLimiterTester : IDisposable
{
	// 配置参数
	private const int ScaleFactor = 100;
	private const int InitialBurstCapacity = 360 * ScaleFactor;
	private const int InitialTokensPerRequest = 200;
	private const int TokensPerPeriod = 6 * ScaleFactor;
	private const int QueueLimit = 600;

	// 运行时状态
	private int _tokensPerRequest = InitialTokensPerRequest;
	private readonly TimeSpan _period;
	private readonly Func<CancellationToken, Task> _apiRequestFunc;
	private readonly TokenBucketRateLimiter _limiter;
	private Channel<Task> _channel;
	private readonly Stopwatch _stopwatch = new Stopwatch();

	// 统计计数器
	private int _totalAttempts;
	private int _successfulRequests;
	private int _rateLimitedRequests;
	private int _otherErrors;

	public RateLimiterTester(Func<CancellationToken, Task> apiRequestFunc, TimeSpan? period = null)
	{
		_apiRequestFunc = apiRequestFunc ?? throw new ArgumentNullException(nameof(apiRequestFunc));
		_period = period ?? TimeSpan.FromSeconds(60);

		_limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
		{
			TokenLimit = InitialBurstCapacity,
			TokensPerPeriod = TokensPerPeriod,
			ReplenishmentPeriod = TimeSpan.FromSeconds(0.1),
			QueueLimit = QueueLimit
		});
	}

	public async Task<TestResult> RunTestAsync(CancellationToken ct = default)
	{
		_channel = Channel.CreateUnbounded<Task>();
		_stopwatch.Start();

		// 启动监控任务
		var monitorTask = Task.Run(() => MonitorStatusAsync(ct), ct);

		var producerTask = Task.Run(() => ProduceRequestsAsync(ct), ct);
		var consumerTask = Task.Run(() => ConsumeResultsAsync(ct), ct);

		await Task.WhenAll(producerTask, consumerTask);
		_stopwatch.Stop();

		await monitorTask;

		return GenerateReport();
	}

	private async Task ProduceRequestsAsync(CancellationToken ct)
	{
		var startTime = DateTime.UtcNow;

		while (!ct.IsCancellationRequested && DateTime.UtcNow - startTime <= _period)
		{
			Interlocked.Increment(ref _totalAttempts);

			using var lease = await _limiter.AcquireAsync(_tokensPerRequest, ct);

			if (lease.IsAcquired)
			{
				var requestTask = _apiRequestFunc(ct);
				await _channel.Writer.WriteAsync(requestTask, ct);
			}
			else
			{
				await Task.Delay(10, ct);
			}
		}

		_channel.Writer.Complete();
	}

	private async Task ConsumeResultsAsync(CancellationToken ct)
	{
		await foreach (var task in _channel.Reader.ReadAllAsync(ct))
		{
			try
			{
				await task;
				Interlocked.Increment(ref _successfulRequests);

				// 成功时减少令牌消耗（最低10）
				Interlocked.Exchange(ref _tokensPerRequest,
					Math.Max(60, _tokensPerRequest - 2));
			}
			catch (Exception ex) when (IsRateLimitExceeded(ex))
			{
				Interlocked.Increment(ref _rateLimitedRequests);

				// 429错误时增加令牌消耗
				Interlocked.Exchange(ref _tokensPerRequest,
					Math.Min(200, _tokensPerRequest + 10));
			}
			catch
			{
				Interlocked.Increment(ref _otherErrors);
			}
		}
	}

	private async Task MonitorStatusAsync(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested && !_stopwatch.IsRunning)
		{
			await Task.Delay(100, ct);
		}

		while (!ct.IsCancellationRequested && _stopwatch.IsRunning)
		{
			var elapsed = _stopwatch.Elapsed;
			if (elapsed >= _period)
				break;

			var stats = _limiter.GetStatistics();
			Console.WriteLine(
				$"[状态] 耗时: {elapsed.TotalSeconds:F1}s/{_period.TotalSeconds}s | " +
				$"消耗: {_tokensPerRequest}令牌/请求 | " +
				$"可用令牌: {stats.CurrentAvailablePermits} | " +
				$"排队: {stats.CurrentQueuedCount} | " +
				$"成功: {_successfulRequests} | " +
				$"429限流: {_rateLimitedRequests}");

			await Task.Delay(1000, ct);
		}
	}

	private TestResult GenerateReport()
	{
		return new TestResult
		{
			TotalAttempts = _totalAttempts,
			SuccessfulRequests = _successfulRequests,
			RateLimitedRequests = _rateLimitedRequests,
			OtherErrors = _otherErrors,
			TestDuration = _stopwatch.Elapsed,
			FinalTokensPerRequest = _tokensPerRequest / ScaleFactor,
			FinalAllowedRate = 3600 / (_tokensPerRequest / ScaleFactor)
		};
	}

	private static bool IsRateLimitExceeded(Exception ex)
	{
		return ex is HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests };
	}

	public void Dispose()
	{
		_limiter?.Dispose();
		_channel.Writer.TryComplete();
	}
}

public class TestResult
{
	public int TotalAttempts { get; set; }
	public int SuccessfulRequests { get; set; }
	public int RateLimitedRequests { get; set; }
	public int OtherErrors { get; set; }
	public TimeSpan TestDuration { get; set; }
	public int FinalTokensPerRequest { get; set; }
	public int FinalAllowedRate { get; set; }

	public double RequestsPerSecond => TotalAttempts / TestDuration.TotalSeconds;

	public override string ToString() => $"""
        ======== 限流测试最终报告 ========
        测试时长: {TestDuration.TotalSeconds:F2}秒
        总尝试请求: {TotalAttempts}
        实际吞吐量: {RequestsPerSecond:F1} 请求/秒
        ----------------------------
        成功请求: {SuccessfulRequests} ({(SuccessfulRequests * 100.0 / TotalAttempts):F1}%)
        429限流请求: {RateLimitedRequests}
        其他错误: {OtherErrors}
        ----------------------------
        最终状态:
        标准化令牌消耗: {FinalTokensPerRequest}/请求
        允许速率: {FinalAllowedRate} 请求/分钟
        =============================
        """;
}