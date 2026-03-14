using FluentFTP;

public class SystemIOFileProvider : IFileProvide
{
    private readonly FileInfo _fileInfo;

    public SystemIOFileProvider(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }

    public SystemIOFileProvider(string filePath)
    {
        _fileInfo = new FileInfo(filePath);
    }

    public long Length => _fileInfo.Exists ? _fileInfo.Length : 0;
    public string Name => _fileInfo.Name;
    public string FullPath => _fileInfo.FullName;

    public Task<bool> Exists() => Task.FromResult(_fileInfo.Exists);

    public async Task CopyToAsync(IFileProvide destination, CancellationToken cancellationToken = default)
    {
        if (!_fileInfo.Exists)
            throw new FileNotFoundException($"文件不存在: {_fileInfo.FullName}");

        if (destination is SystemIOFileProvider sysDest)
        {
            // 本地到本地复制
            _fileInfo.CopyTo(sysDest.FullPath, overwrite: true);
        }
        else if (destination is FtpFileProvider ftpDest)
        {
            // 本地到FTP
            var ftpClient = ftpDest.GetFtpClient();
            using var fileStream = _fileInfo.OpenRead();
            await ftpClient.UploadStream(fileStream, ftpDest.FullPath, FtpRemoteExists.Overwrite, token: cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"不支持的目标类型: {destination.GetType()}");
        }
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (_fileInfo.Exists)
        {
            _fileInfo.Delete();
        }
        return Task.CompletedTask;
    }
}
