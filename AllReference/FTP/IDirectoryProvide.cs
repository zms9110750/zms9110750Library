public interface IDirectoryProvide
{
    public IAsyncEnumerable<IDirectoryProvide> EnumChindDirectoryProvide();
    public IAsyncEnumerable<IFileProvide> EnumChindFileProvide();
    public string Name { get; }
    public string FullPath { get; }
    public Task CreateAsync(CancellationToken cancellationToken = default);
    public Task<bool> Exists();
    public Task DeleteAsync(bool recursive = false, CancellationToken cancellationToken = default);
}
