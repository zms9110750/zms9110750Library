using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomon.NET.Tests.Streams;

// ==================== 编码流 ====================
/// <summary>
/// Reed-Solomon 编码流：将写入的数据编码后分散到多个分片流
/// </summary>
internal sealed class ReedSolomonEncodeStream : Stream
{
    private readonly Stream _dataStream;              // 原始数据输入流
    private readonly IReadOnlyList<Stream> _shardStreams;  // 分片输出流集合
    private readonly IReedSolomon _rs;                // RS 编解码器
    private readonly long _totalLength;               // 总数据长度
    private readonly int _shardSize;                  // 每个分片的大小
    private readonly int _blockSize;                  // 块大小
    private readonly ReedSolomonStreamState _state;   // 状态机
    private long _position;                           // 当前位置

    public ReedSolomonEncodeStream(
        Stream dataStream,
        IReadOnlyList<Stream> shardStreams,
        IReedSolomon rs,
        long totalLength)
    {
        _dataStream = dataStream;
        _shardStreams = shardStreams;
        _rs = rs;
        _totalLength = totalLength;

        // 计算分片大小
        _shardSize = (int)((totalLength + rs.DataShardCount - 1) / rs.DataShardCount);

        // 计算块大小（目标总缓冲区约 4MB）
        _blockSize = Math.Max(1024, (4 * 1024 * 1024) / rs.DataShardCount);
        _blockSize = Math.Min(_blockSize, _shardSize);

        // 初始化状态机
        _state = new ReedSolomonStreamState(rs, totalLength, _blockSize);
        _position = 0;
    }

    public override bool CanRead => false;
    public override bool CanWrite => true;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => _position; set => throw new NotSupportedException(); }

    public override void Write(byte[] buffer, int offset, int count)
    {
        // 同步写入实现
        throw new NotImplementedException();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // 异步写入实现
        throw new NotImplementedException();
    }

    public override void Flush()
    {
        foreach (var stream in _shardStreams)
        {
            stream.Flush();
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();
    public override void SetLength(long value)
        => throw new NotSupportedException();
}


// ==================== 解码流 ====================
/// <summary>
/// Reed-Solomon 解码流：从多个分片流读取数据，解码后输出恢复的原始数据
/// </summary>
internal sealed class ReedSolomonDecodeStream : Stream
{
    private readonly Stream _dataStream;              // 原始数据输出流
    private readonly IReadOnlyList<Stream> _shardStreams;  // 分片输入流集合
    private readonly IReedSolomon _rs;                // RS 编解码器
    private readonly long _totalLength;               // 总数据长度
    private readonly int _shardSize;                  // 每个分片的大小
    private readonly int _blockSize;                  // 块大小
    private readonly ReedSolomonStreamState _state;   // 状态机
    private long _position;                           // 当前位置

    public ReedSolomonDecodeStream(
        Stream dataStream,
        IReadOnlyList<Stream> shardStreams,
        IReedSolomon rs,
        long totalLength)
    {
        _dataStream = dataStream;
        _shardStreams = shardStreams;
        _rs = rs;
        _totalLength = totalLength;

        // 计算分片大小
        _shardSize = (int)((totalLength + rs.DataShardCount - 1) / rs.DataShardCount);

        // 计算块大小
        _blockSize = Math.Max(1024, (4 * 1024 * 1024) / rs.DataShardCount);
        _blockSize = Math.Min(_blockSize, _shardSize);

        // 初始化状态机
        _state = new ReedSolomonStreamState(rs, totalLength, _blockSize);
        _position = 0;
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;
    public override bool CanSeek => false;
    public override long Length => _totalLength;
    public override long Position { get => _position; set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // 同步读取实现
        throw new NotImplementedException();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // 异步读取实现
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();
    public override void SetLength(long value)
        => throw new NotSupportedException();
}