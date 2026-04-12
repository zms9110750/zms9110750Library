using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.JMComic.Model;

namespace zms9110750.JMComic.Serialization;

/// <summary>
/// JMComic JSON序列化的源生成器上下文
/// </summary>
/// <remarks>
/// 使用源生成器提供高性能的JSON序列化。
/// 所有JMComic实体类的JSON序列化都通过此上下文配置。
/// 源生成器在编译时生成序列化代码，提供更好的性能。
/// </remarks>
[JsonSerializable(typeof(JmAlbum))]
[JsonSerializable(typeof(JmEpisode))]
[JsonSerializable(typeof(JmChapter))]
[JsonSerializable(typeof(JmImage))]
[JsonSerializable(typeof(JmSearchResult))]
[JsonSerializable(typeof(JmFavoriteFolder))]
[JsonSerializable(typeof(JmPagedResult<>))]
[JsonSerializable(typeof(JmApiResponse<>))]
[JsonSerializable(typeof(JmUser))]
[JsonSerializable(typeof(JmComment))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class JmJsonSerializerContext : JsonSerializerContext
{
    /// <summary>
    /// 获取默认的JSON序列化选项
    /// </summary>
    /// <returns>配置好的JSON序列化选项</returns>
    /// <remarks>
    /// 默认选项配置：
    /// 1. 属性名使用camelCase命名
    /// 2. 忽略空值
    /// 3. 允许尾随逗号
    /// 4. 使用不区分大小写的属性名匹配
    /// </remarks>
    public static JsonSerializerOptions DefaultOptions { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters =
        {
            // 可以在这里添加自定义转换器
        }
    };
}