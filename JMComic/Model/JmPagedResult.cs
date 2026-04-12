
namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示分页查询的结果
/// </summary>
/// <typeparam name="T">分页结果中数据项的类型</typeparam>
/// <param name="Items">当前页的数据项数组</param>
/// <param name="PageIndex">当前页码，从1开始</param>
/// <param name="PageSize">每页的数据项数量</param>
/// <param name="TotalCount">总数据项数量</param>
/// <param name="TotalPages">总页数</param>
/// <remarks>
/// 分页结果用于包装API返回的分页数据，包含分页元信息和当前页的数据。
/// 大多数列表查询API都返回分页结果，如搜索、分类浏览、收藏夹等。
/// </remarks>
public record JmPagedResult<T>(
    T[] Items,
    int PageIndex,
    int PageSize,
    int TotalCount,
    int TotalPages
);