public interface IFileProvide
{
    public long Length { get; }
    public string Name { get; }
    public string FullPath { get; }
    public Task<bool> Exists();
    public Task CopyToAsync(IFileProvide destination, CancellationToken cancellationToken = default);
    public Task DeleteAsync(CancellationToken cancellationToken = default);
}
