using FluentFTP;

public class FtpDirectoryProvider : IDirectoryProvide
{
    private readonly AsyncFtpClient _ftpClient;
    private readonly string _directoryPath;
    private List<FtpListItem>? _cachedItems;

    public FtpDirectoryProvider(AsyncFtpClient ftpClient, string directoryPath)
    {
        _ftpClient = ftpClient;
        _directoryPath = directoryPath.TrimEnd('/') + "/";
    }

    public string Name
    {
        get
        {
            var trimmedPath = _directoryPath.TrimEnd('/');
            return Path.GetFileName(trimmedPath) ?? trimmedPath;
        }
    }

    public string FullPath => _directoryPath;
    internal AsyncFtpClient GetFtpClient() => _ftpClient;

    public async Task CreateAsync(CancellationToken cancellationToken = default)
    {
        if (!await Exists())
        {
            await _ftpClient.CreateDirectory(_directoryPath, true, cancellationToken);
        }
    }

    public async Task<bool> Exists()
    {
        return await _ftpClient.DirectoryExists(_directoryPath);
    }

    public async IAsyncEnumerable<IDirectoryProvide> EnumChindDirectoryProvide()
    {
        await EnsureItemsCached();
        if (_cachedItems == null)
            yield break;

        foreach (var item in _cachedItems)
        {
            if (item.Type == FtpObjectType.Directory)
            {
                yield return new FtpDirectoryProvider(_ftpClient, item.FullName);
            }
        }
    }

    public async IAsyncEnumerable<IFileProvide> EnumChindFileProvide()
    {
        await EnsureItemsCached();
        if (_cachedItems == null)
            yield break;

        foreach (var item in _cachedItems)
        {
            if (item.Type == FtpObjectType.File)
            {
                yield return new FtpFileProvider(_ftpClient, item.FullName);
            }
        }
    }

    public async Task DeleteAsync(bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (await Exists())
        {
            await _ftpClient.DeleteDirectory(_directoryPath, recursive ? FtpListOption.Recursive : FtpListOption.Auto, cancellationToken);
        }
    }

    private async Task EnsureItemsCached()
    {
        if (_cachedItems == null)
        {
            if (await _ftpClient.DirectoryExists(_directoryPath))
            {
                _cachedItems = (await _ftpClient.GetListing(_directoryPath)).ToList();
            }
            else
            {
                _cachedItems = new List<FtpListItem>();
            }
        }
    }
}
