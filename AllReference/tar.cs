

using System.Buffers;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Pipelines;
using zms9110750.ReedSolomon.Streams;

/*// 创建 tar 文件
string tarFilePath = @$"C:\Users\16229\source\NugetPack\nuget文档注释备份\{DateTime.Now.ToString("yyyy-MM-dd HH mm")}.tar";

// 或者只添加指定的文件
using FileStream tarStream = File.Create(tarFilePath);
using TarWriter writer = new TarWriter(tarStream);

foreach (var item in Directory.EnumerateDirectories("C:\\Users\\16229\\.nuget\\packages", "zh-Hans", SearchOption.AllDirectories).SelectMany(dir => Directory.EnumerateFiles(dir, "*.xml", SearchOption.TopDirectoryOnly)))
{
    writer.WriteEntry(item, item.Substring("C:\\Users\\16229\\.nuget\\packages\\".Length).Replace('\\', '/'));
    Console.WriteLine(item.Substring("C:\\Users\\16229\\.nuget\\packages".Length));

}*/
Debugger.Break();
int result = 4;
Debug.Assert(result > 0, "结果必须大于0");  // 只在 Debug 下检查
Trace.Assert(result > 0, "结果必须大于0");  // Debug 和 Release 都检查

Pipe pipe = new Pipe();
byte[] bar = new byte[1024];
pipe.Writer.AsStream().Write(bar);

var b = pipe.Reader.TryRead(out ReadResult readResult);
Console.WriteLine(b);
Console.WriteLine(readResult.Buffer.Length);
Console.WriteLine(readResult.Buffer.FirstSpan.Length);
pipe.Reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
Console.WriteLine("======");

b = pipe.Reader.TryRead(out readResult);
Console.WriteLine(b);
Console.WriteLine(readResult.Buffer.Length);
Console.WriteLine(readResult.Buffer.FirstSpan.Length);


Console.WriteLine("======");
pipe.Writer.AsStream().Write([1]);

b = pipe.Reader.TryRead(out readResult);
Console.WriteLine(b);
Console.WriteLine(readResult.Buffer.Length);
Console.WriteLine(readResult.Buffer.FirstSpan.Length);

byte[] bar1 = new byte[1024];
byte[] bar2 = new byte[512];

MemoryStream ms = new MemoryStream(bar1);
MemoryStream ms2 = new MemoryStream(bar2);

StreamRoundRobin roundRobin = new StreamRoundRobin([ms, ms2], 20);
var prb = PipeReader.Create(roundRobin);
var rl = await prb.ReadAtLeastAsync(1);
Console.WriteLine(rl.Buffer.Length);
Console.WriteLine(ms.Position);
Console.WriteLine(ms2.Position);
prb.AdvanceTo(rl.Buffer.End);
rl = await prb.ReadAtLeastAsync(2000);
Console.WriteLine(rl.Buffer.Length); 
