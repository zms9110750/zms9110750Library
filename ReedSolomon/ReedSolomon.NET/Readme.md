# RS冗余编码校验库

基于 https://github.com/egbakou/reedsolomon 通过ai创作的 Reed-Solomon 编码库
经过了性能优化和现代化改造。
破坏了原库的API。添加了流式接口，支持异步操作。
 
## 快速开始

### 编码：生成冗余分片

```csharp
using zms9110750.ReedSolomon.ReedSolomons;

// 创建编解码器：16 个数据分片，4 个冗余分片
var rs = new ReedSolomon8Bit(16, 4);

// 准备数据分片（每个分片大小相同）
var dataShards = new List<byte[]>();
for (int i = 0; i < 16; i++)
{
    var shard = new byte[1024];
    // 填充数据...
    dataShards.Add(shard);
}

// 准备冗余分片（空的，会被写入）
var parityShards = new List<byte[]>();
for (int i = 0; i < 4; i++)
{
    parityShards.Add(new byte[1024]);
}

// 编码
rs.EncodeParity(dataShards, parityShards);
```

### 解码：恢复丢失的分片

```csharp
using zms9110750.ReedSolomon.ReedSolomons;

var rs = new ReedSolomon8Bit(16, 4);

// 所有分片（数据 + 冗余）
var allShards = dataShards.Concat(parityShards).ToList();

// 标记哪些分片存在（丢失分片 0 和 5）
var shardPresent = Enumerable.Repeat(true, 20).ToArray();
shardPresent[0] = false;
shardPresent[5] = false;

// 清空丢失的分片
allShards[0] = new byte[1024];
allShards[5] = new byte[1024];

// 解码恢复
rs.DecodeMissing(allShards, shardPresent);
```

### 校验冗余分片是否正确

```csharp
using zms9110750.ReedSolomon.ReedSolomons;

var rs = new ReedSolomon8Bit(16, 4);

// 编码
rs.EncodeParity(dataShards, parityShards);

// 校验
bool isCorrect = rs.IsParityCorrect(dataShards, parityShards);
Console.WriteLine($"校验结果: {isCorrect}");
```

## 流式处理（大文件）

### 编码：文件 → 分片流

```csharp
using zms9110750.ReedSolomon.ReedSolomons;
using System.IO;

var rs = new ReedSolomon8Bit(16, 4);
string inputFile = "largefile.mp4";
string outputDir = @"C:\shards";

// 创建输出流
var outputStreams = new List<Stream>();
for (int i = 0; i < rs.TotalShardCount; i++)
{
    outputStreams.Add(File.Create(Path.Combine(outputDir, $"shard_{i}.bin")));
}

// 编码
using (var input = File.OpenRead(inputFile))
{
    await rs.EncodeParityAsync(input, input.Length, outputStreams);
}

// 关闭流
foreach (var s in outputStreams) s.Dispose();
```

### 解码：分片流 → 文件

```csharp
using zms9110750.ReedSolomon.ReedSolomons;
using System.IO;

var rs = new ReedSolomon8Bit(16, 4);
string inputDir = @"C:\shards";
string outputFile = @"C:\recovered.mp4";
long originalLength = new FileInfo(@"C:\largefile.mp4").Length; // 需要知道原始大小

// 创建输入流（null 表示缺失）
var inputStreams = new List<Stream?>();
for (int i = 0; i < 20; i++)
{
    string path = Path.Combine(inputDir, $"shard_{i}.bin");
    if (File.Exists(path))
    {
        inputStreams.Add(File.OpenRead(path));
    }
    else
    {
        inputStreams.Add(null); // 缺失的分片
    }
}

// 解码
using (var output = File.Create(outputFile))
{
    await rs.DecodeMissingAsync(inputStreams, output, originalLength);
}

// 关闭存在的流
foreach (var s in inputStreams.Where(x => x != null))
{
    s.Dispose();
}

## 自定义伽罗瓦域

```csharp
using zms9110750.ReedSolomon.Galois;
using zms9110750.ReedSolomon.ReedSolomons;

// 使用不同的本原多项式（默认 P29）
var gf = new GF8bit(GF8Poly.P43);
var rs = new ReedSolomon8Bit(16, 4, gf);
```

## 分块处理（手动控制）

```csharp
using zms9110750.ReedSolomon.ReedSolomons;

var rs = new ReedSolomon8Bit(16, 4);
int shardSize = 1024 * 1024 * 128; // 128MB
int blockSize = 1024 * 1024; // 1MB
int blocks = (shardSize + blockSize - 1) / blockSize;

for (int block = 0; block < blocks; block++)
{
    int offset = block * blockSize;
    int byteCount = Math.Min(blockSize, shardSize - offset);
    
    rs.EncodeParity(dataShards, parityShards, offset, byteCount);
}
```

## API 参考

| 类 | 说明 |
|---|---|
| `IReedSolomon<T>` | 泛型接口，支持 byte/ushort/uint |
| `ReedSolomon8Bit` | byte 版本实现 |
| `GF8bit` | GF(2⁸) 伽罗瓦域 |
| `GF8Poly` | 本原多项式枚举（P29、P43、P45 等 16 种） |
 