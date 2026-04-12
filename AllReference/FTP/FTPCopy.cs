#if true2
//ftp://192.168.0.107:3721/Alarms/
CopyFromFtp("192.168.0.107", 3721, "Alarms/", "F:\\Alarms");
/*
IProgress<string> progress=new Progress<string>(Console.WriteLine); 
// 创建FTP客户端，不指定用户名密码
 var client = new AsyncFtpClient("192.168.0.107", 3721);
 var client2 = new AsyncFtpClient("192.168.0.109", 3721);

client.Config.DataConnectionType = FtpDataConnectionType.AutoActive;
// 连接FTP服务器
await client2.Connect();

// 创建接口实例
var remoteDirProvider = new FtpDirectoryProvider(client, "AlarmsBack/");
var localDirProvider = new SystemIODirectoryProvider("F:\\Alarms");

await localDirProvider.CopyToAsync(remoteDirProvider,true, progress);*/


#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
static void CopyFromFtp(string ftpHost, int ftpPort, string remotePath, string localPath)
{
    // 创建FTP客户端，不指定用户名密码
    using (var client = new FtpClient(ftpHost, ftpPort))
    {
        // 连接FTP服务器
        client.Connect();

        // 开始递归复制
        CopyDirectory(client, remotePath, localPath);

        // 断开连接
        client.Disconnect();
    }
}
#pragma warning restore CS8321 // 已声明本地函数，但从未使用过

// 递归复制目录的方法
static void CopyDirectory(FtpClient client, string remoteDir, string localDir)
{
    // 获取远程目录下的所有项目（文件和子目录）
    var items = client.GetListing(remoteDir);

    foreach (var item in items)
    {
        // 本地对应路径
        var localPath = Path.Combine(localDir, item.Name);

        if (item.Type == FtpObjectType.Directory)
        {
            if (((string[])[""]).Contains(item.Name))
            {
                continue;
            }
            // 如果是目录：创建本地目录，然后递归复制
            Directory.CreateDirectory(localPath);
            CopyDirectory(client, item.FullName, localPath);
        }
        else if (item.Type == FtpObjectType.File)
        {
            // 检查本地文件是否存在
            if (File.Exists(localPath))
            {
                // 获取本地文件大小
                var localFileInfo = new FileInfo(localPath);

                // 如果文件大小相同，跳过下载
                if (localFileInfo.Length == item.Size)
                {
                    continue; // 跳过这个文件
                }
            }

            // 下载文件（只有不存在或大小不同时才下载）
            client.DownloadFile(localPath, item.FullName);
            Console.WriteLine("完成:" + localPath);
        }
    }
}
#endif
