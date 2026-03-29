using System.Formats.Tar;
using zms9110750.ReedSolomon.ReedSolomons;

byte[] data = File.ReadAllBytes("小鹿.yaml");

using var ms = new MemoryStream();
using var writer = new TarWriter(ms, TarEntryFormat.Pax, leaveOpen: true);

// 创建条目（PAX 支持中文）
var entry = new PaxTarEntry(TarEntryType.RegularFile, "小鹿.yaml")
{
    Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite,
    DataStream = new MemoryStream(data)
};

writer.WriteEntry(entry);

// 关闭 writer 会自动写结束标记
writer.Dispose();

File.WriteAllBytes("小鹿.yaml.tar", ms.ToArray());


  