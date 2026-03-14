public static class DirectoryCopyExtensions
{
    public static async Task CopyToAsync(
        this IDirectoryProvide source,
        IDirectoryProvide destination,
        bool skipIdenticalFiles = true,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await destination.CreateAsync(cancellationToken);

        // 复制文件
        await foreach (var sourceFile in source.EnumChindFileProvide().WithCancellation(cancellationToken))
        {
            var targetFile = GetTargetFile(destination, sourceFile.Name);

            bool shouldCopy = true;
            if (skipIdenticalFiles && await targetFile.Exists())
            {
                var sourceSize = sourceFile.Length;
                var targetSize = targetFile.Length;

                if (sourceSize == targetSize)
                {
                    progress?.Report($"跳过（大小相同）: {sourceFile.Name}");
                    shouldCopy = false;
                }
            }

            if (shouldCopy)
            {
                progress?.Report($"复制: {sourceFile.Name}");
                await sourceFile.CopyToAsync(targetFile, cancellationToken);
            }
        }

        // 递归复制子目录
        await foreach (var sourceSubDir in source.EnumChindDirectoryProvide().WithCancellation(cancellationToken))
        {
            var targetSubDir = GetTargetDirectory(destination, sourceSubDir.Name);
            await sourceSubDir.CopyToAsync(targetSubDir, skipIdenticalFiles, progress, cancellationToken);
        }
    }

    private static IFileProvide GetTargetFile(IDirectoryProvide dir, string fileName)
    {
        if (dir is SystemIODirectoryProvider sysDir)
        {
            return new SystemIOFileProvider(Path.Combine(sysDir.FullPath, fileName));
        }
        else if (dir is FtpDirectoryProvider ftpDir)
        {
            var client = ftpDir.GetFtpClient();
            return new FtpFileProvider(client, $"{ftpDir.FullPath.TrimEnd('/')}/{fileName}");
        }
        throw new NotSupportedException($"不支持的类型: {dir.GetType()}");
    }

    private static IDirectoryProvide GetTargetDirectory(IDirectoryProvide dir, string dirName)
    {
        if (dir is SystemIODirectoryProvider sysDir)
        {
            return new SystemIODirectoryProvider(Path.Combine(sysDir.FullPath, dirName));
        }
        else if (dir is FtpDirectoryProvider ftpDir)
        {
            var client = ftpDir.GetFtpClient();
            return new FtpDirectoryProvider(client, $"{ftpDir.FullPath.TrimEnd('/')}/{dirName}");
        }
        throw new NotSupportedException($"不支持的类型: {dir.GetType()}");
    }
}