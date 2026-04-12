using Microsoft.Extensions.Caching.Hybrid;
using Polly;

namespace zms9110750.Utils.Adapters.Polly;
/// <summary>
/// 混合缓存策略的配置选项
/// </summary>
public sealed class HybridCacheStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// 获取混合缓存实例
    /// </summary>
    public HybridCache Cache { get; init; }

    /// <summary>
    /// 获取用于从 <see cref="ResilienceContext.Properties"/> 中读取缓存键的属性键
    /// </summary>
    /// <remarks>
    /// 如果设置了此属性，策略会优先从上下文的属性字典中读取缓存键；
    /// 否则会使用 <see cref="ResilienceStrategy.ExecuteCore{TResult,TState}"/> 中的 <paramref name="state"/> 参数的字符串表示作为缓存键。
    /// </remarks>
    public ResiliencePropertyKey<string> Key { get; init; }

    /// <summary>
    /// 初始化 <see cref="HybridCacheStrategyOptions"/> 的新实例
    /// </summary>
    /// <param name="cache">混合缓存实例</param>
    /// <param name="key">可选，用于从上下文属性中读取缓存键的键名</param>
    public HybridCacheStrategyOptions(HybridCache cache, ResiliencePropertyKey<string> key = default)
    {
        Name = "HybridCache";
        Cache = cache;
        Key = key;
    }
}
