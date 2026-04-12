using FluentFTP;

public class FtpFileProvider : IFileProvide
{
    private readonly AsyncFtpClient _ftpClient;
    private readonly string _filePath;
    private FtpListItem? _cachedItem;

    public FtpFileProvider(AsyncFtpClient ftpClient, string filePath)
    {
        _ftpClient = ftpClient;
        _filePath = filePath;
    }

    public long Length
    {
        get
        {
            EnsureCachedItem();
            return _cachedItem?.Size ?? 0;
        }
    }

    public string Name => Path.GetFileName(_filePath);
    public string FullPath => _filePath;

    internal AsyncFtpClient GetFtpClient()
    {
        return _ftpClient;
    }

    public async Task<bool> Exists()
    {
        return await _ftpClient.FileExists(_filePath);
    }

    public async Task CopyToAsync(IFileProvide destination, CancellationToken cancellationToken = default)
    {
        if (!await Exists())
            throw new FileNotFoundException($"FTP文件不存在: {_filePath}");

        if (destination is SystemIOFileProvider sysDest)
        {
            // FTP到本地
            await _ftpClient.DownloadFile(sysDest.FullPath, _filePath, FtpLocalExists.Overwrite,
                FtpVerify.None, token: cancellationToken);
        }
        else if (destination is FtpFileProvider ftpDest)
        {
            // FTP到FTP
            using var memoryStream = new MemoryStream();
            await _ftpClient.DownloadStream(memoryStream, _filePath, token: cancellationToken);
            memoryStream.Position = 0;
            var destClient = ftpDest.GetFtpClient();
            await destClient.UploadStream(memoryStream, ftpDest.FullPath, FtpRemoteExists.Overwrite,
                token: cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"不支持的目标类型: {destination.GetType()}");
        }
    }

    public async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (await Exists())
        {
            await _ftpClient.DeleteFile(_filePath, cancellationToken);
        }
    }

    private async void EnsureCachedItem()
    {
        if (_cachedItem == null && await Exists())
        {
            var items = await _ftpClient.GetListing(Path.GetDirectoryName(_filePath));
            _cachedItem = items.FirstOrDefault(item => item.FullName == _filePath && item.Type == FtpObjectType.File);
        }
    }
}
