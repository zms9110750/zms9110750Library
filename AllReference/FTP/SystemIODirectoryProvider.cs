public class SystemIODirectoryProvider : IDirectoryProvide
{
    private readonly DirectoryInfo _directoryInfo;

    public SystemIODirectoryProvider(DirectoryInfo directoryInfo)
    {
        _directoryInfo = directoryInfo;
    }

    public SystemIODirectoryProvider(string directoryPath)
    {
        _directoryInfo = new DirectoryInfo(directoryPath);
    }

    public string Name => _directoryInfo.Name;
    public string FullPath => _directoryInfo.FullName;

    public Task CreateAsync(CancellationToken cancellationToken = default)
    {
        _directoryInfo.Create();
        return Task.CompletedTask;
    }

    public Task<bool> Exists() => Task.FromResult(_directoryInfo.Exists);

    public async IAsyncEnumerable<IDirectoryProvide> EnumChindDirectoryProvide()
    {
        if (!_directoryInfo.Exists)
            yield break;
        foreach (var dir in _directoryInfo.GetDirectories())
        {
            yield return new SystemIODirectoryProvider(dir);
        }
    }

    public async IAsyncEnumerable<IFileProvide> EnumChindFileProvide()
    {
        if (!_directoryInfo.Exists)
            yield break;
        foreach (var file in _directoryInfo.GetFiles())
        {
            yield return new SystemIOFileProvider(file);
        }
    }

    public Task DeleteAsync(bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (_directoryInfo.Exists)
        {
            _directoryInfo.Delete(recursive);
        }
        return Task.CompletedTask;
    }
}
