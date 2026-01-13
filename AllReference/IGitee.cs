using Refit;

public interface IGitee
{
    /// <summary>
    /// 
    /// </summary> 
    /// <remarks>direction 可选 desc 和 asc</remarks>
    /// <returns></returns>
    [Get("/repos/{owner}/{repo}/releases")]
    public Task<GiteeRelease[]> Releases(string owner, string repo, [Query] int? page = default, [Query] int? per_page = default, [Query] string? direction = default);
}
