using Polly;
using Polly.Telemetry;

namespace zms9110750.Utils.Adapters.Polly;

/// <summary>
/// 混合缓存策略
/// </summary>
/// <remarks>
/// 此策略在执行用户代码前先查询混合缓存。如果缓存命中，则直接返回缓存值；
/// 如果缓存未命中，则执行用户代码，并将结果存入缓存。
/// </remarks>
public sealed class HybridCacheStrategy(HybridCacheStrategyOptions options, ResilienceStrategyTelemetry telemetry) : ResilienceStrategy
{
    private readonly HybridCacheStrategyOptions _options = options;
    private readonly ResilienceStrategyTelemetry _telemetry = telemetry;

    /// <inheritdoc />
    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // 确定缓存键：优先从上下文属性获取，否则使用 state 的字符串表示
        string key = _options.Key.Key != default &&
                     context.Properties.TryGetValue(_options.Key, out var cacheKeyObj) &&
                     cacheKeyObj is string cacheKey
            ? cacheKey
            : state!.ToString()!;

        // 从缓存获取或创建值
        var cached = await _options.Cache.GetOrCreateAsync(
            key,
            (callback, context, state, _telemetry),
            static async (state, can) =>
            {
                // 缓存未命中，记录遥测
                state._telemetry.Report(
                    new ResilienceEvent(ResilienceEventSeverity.Information, "CacheMiss"),
                    state.context,
                    state.state);

                // 执行用户代码
                Outcome<TResult> result = await state.callback(state.context, state.state)
                    .ConfigureAwait(state.context.ContinueOnCapturedContext);

                // 如果执行成功，返回结果；如果失败，抛出异常（缓存不存储失败结果）
                result.ThrowIfException();
                return result.Result;
            })
            .ConfigureAwait(context.ContinueOnCapturedContext);

        // 返回成功结果
        return Outcome.FromResult(cached);
    }
}
