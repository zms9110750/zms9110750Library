using Microsoft.Extensions.Caching.Hybrid;
using Polly; 
namespace zms9110750.Utils.Adapters.Polly;

/// <summary>
/// 为 <see cref="ResiliencePipelineBuilder"/> 提供混合缓存策略的扩展方法
/// </summary>
public static class FusionCachePipelineBuilderExtensions
{
    /// <summary>
    /// 向管道中添加混合缓存策略
    /// </summary>
    /// <param name="builder">管道构建器</param>
    /// <param name="options">缓存策略配置选项</param>
    /// <returns>返回当前管道构建器以支持链式调用</returns>
    /// <remarks>将重试策略放在缓存策略的内层，防止重试时获取缓存而不执行用户代码</remarks>
    public static ResiliencePipelineBuilder AddHybridCache(
        this ResiliencePipelineBuilder builder,
        HybridCacheStrategyOptions options)
    {

        return builder.AddStrategy(
            context => new HybridCacheStrategy(options, context.Telemetry),
            options);
    }

    /// <summary>
    /// 向管道中添加混合缓存策略。将重试策略放在缓存策略的内层，防止重试时获取缓存而不执行用户代码。
    /// </summary>
    /// <param name="builder">管道构建器</param>
    /// <param name="cache">混合缓存实例</param>
    /// <param name="resiliencePropertyKey">用于从 <see cref="ResilienceContext.Properties"/> 中读取缓存键的键名</param>
    /// <returns>返回当前管道构建器以支持链式调用</returns>
    /// <remarks>缓存键将优先从上下文属性中读取，如果不存在则使用 <see cref="ResilienceStrategy.ExecuteCore{TResult,TState}"/> 中的 <paramref name="state"/> 参数的字符串表示</remarks>
    public static ResiliencePipelineBuilder AddHybridCache(
        this ResiliencePipelineBuilder builder,
        HybridCache cache,
        string resiliencePropertyKey)
    {
        ArgumentNullException.ThrowIfNull(resiliencePropertyKey);
        HybridCacheStrategyOptions options = new HybridCacheStrategyOptions(
            cache,
            new ResiliencePropertyKey<string>(resiliencePropertyKey));
        return builder.AddStrategy(
            context => new HybridCacheStrategy(options, context.Telemetry),
            options);
    }

    /// <summary>
    /// 向泛型管道中添加混合缓存策略。将重试策略放在缓存策略的内层，防止重试时获取缓存而不执行用户代码。
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    /// <param name="builder">管道构建器</param>
    /// <param name="options">缓存策略配置选项</param>
    /// <returns>返回当前管道构建器以支持链式调用</returns>
    public static ResiliencePipelineBuilder<T> AddHybridCache<T>(
        this ResiliencePipelineBuilder<T> builder,
        HybridCacheStrategyOptions options)
    {
        return builder.AddStrategy(
            context => new HybridCacheStrategy(options, context.Telemetry),
            options);
    }

    /// <summary>
    /// 向泛型管道中添加混合缓存策略。将重试策略放在缓存策略的内层，防止重试时获取缓存而不执行用户代码。
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    /// <param name="builder">管道构建器</param>
    /// <param name="cache">混合缓存实例</param>
    /// <param name="resiliencePropertyKey">用于从 <see cref="ResilienceContext.Properties"/> 中读取缓存键的键名</param>
    /// <returns>返回当前管道构建器以支持链式调用</returns>
    /// <remarks>缓存键将优先从上下文属性中读取，如果不存在则使用 <see cref="ResilienceStrategy.ExecuteCore{TResult,TState}"/> 中的 <paramref name="state"/> 参数的字符串表示</remarks>
    public static ResiliencePipelineBuilder<T> AddHybridCache<T>(
        this ResiliencePipelineBuilder<T> builder,
        HybridCache cache,
        string resiliencePropertyKey)
    {
        ArgumentNullException.ThrowIfNull(resiliencePropertyKey);
        HybridCacheStrategyOptions options = new HybridCacheStrategyOptions(
            cache,
            new ResiliencePropertyKey<string>(resiliencePropertyKey));
        return builder.AddStrategy(
            context => new HybridCacheStrategy(options, context.Telemetry),
            options);
    }
}