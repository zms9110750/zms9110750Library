using System.Collections.Immutable;

namespace zms9110750.ReedSolomon.Streams;

/// <summary>
/// 流轮换器：循环使用多个底层流，每写入/读取指定字节数自动切换到下一个流
/// </summary>
public class StreamRoundRobin : Stream
{
    private readonly ImmutableList<Stream> _streams;
    private long _position;
    /// <summary>
    /// 底层流的数量
    /// </summary>
    public int StreamsCount => _streams.Count;
    /// <summary>
    /// 每个流操作的字节数，达到后自动切换
    /// </summary>
    public int SegmentSize { get; }

    /// <inheritdoc/>
    public override bool CanRead { get; }

    /// <inheritdoc/>
    public override bool CanWrite { get; }

    /// <inheritdoc/>
    public override bool CanSeek { get; }

    /// <summary>
    /// 当前正在使用的流
    /// </summary>
    private Stream CurrentStream => _streams[(int)((_position / SegmentSize) % _streams.Count)];

    /// <summary>
    /// 初始化流轮换器
    /// </summary>
    /// <param name="streams">底层流集合（循环使用，不会关闭）</param>
    /// <param name="segmentSize">每个流操作的字节数，达到后自动切换</param>
    public StreamRoundRobin(IEnumerable<Stream> streams, int segmentSize)
    {
        if (streams == null)
        {
            throw new ArgumentNullException(nameof(streams));
        }
        if (segmentSize <= 0)
        {
            throw new ArgumentException("segmentSize 必须大于 0", nameof(segmentSize));
        }

        // 过滤掉 null 的流，转为不可变列表
        _streams = streams.OfType<Stream>().ToImmutableList();

        if (_streams.Count == 0)
        {
            throw new ArgumentException("至少需要一个有效的流", nameof(streams));
        }

        SegmentSize = segmentSize;

        CanRead = true;
        CanWrite = true;
        CanSeek = true;
        foreach (var stream in _streams)
        {
            CanRead &= stream.CanRead;
            CanWrite &= stream.CanWrite;
            CanSeek &= stream.CanSeek;
        }

        // 如果既不能读也不能写，报错
        if (!CanRead && !CanWrite)
        {
            throw new InvalidOperationException("基础流即不全部可读也不全部可写。");
        }
    }

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();
    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set
        {
            if (!CanSeek)
            {
                throw new NotSupportedException("流不可跳转");
            }
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (value == _position)
            {
                return;
            }

            Span<long> stepOld = stackalloc long[_streams.Count];
            Span<long> stepNew = stackalloc long[_streams.Count];
            FillBySegments(stepOld, SegmentSize, _position);
            FillBySegments(stepNew, SegmentSize, value);


            for (int i = 0; i < _streams.Count; i++)
            {
                _streams[i].Seek(stepNew[i] - stepOld[i], SeekOrigin.Current);
            }
            _position = value;

            static void FillBySegments(Span<long> span, int segmentSize, long position)
            {
                span.Fill(Math.DivRem(position, span.Length * segmentSize, out long remaining) * segmentSize);

                for (int i = 0; i < span.Length; i++)
                {
                    long add = Math.Min(segmentSize, remaining);
                    span[i] += add;
                    remaining -= add;
                }
            }
        }
    }
    /// <inheritdoc/>
    public override void Flush()
    {
        foreach (var stream in _streams)
        {
            stream.Flush();
        }
    }

    /// <inheritdoc/>
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        foreach (var stream in _streams)
        {
            await stream.FlushAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        // 检查当前流是否支持读取操作
        if (!CanRead)
        {
            throw new NotSupportedException("流不可读");
        }

        // 记录总共已经读取的字节数
        int totalBytesRead = 0;
        // 当前写入缓冲区的偏移位置
        int currentOffset = offset;
        // 还需要读取的字节数
        int bytesRemaining = count;

        // 循环读取，直到满足请求的字节数或流结束
        while (bytesRemaining > 0)
        {
            // 本次在当前流中计划读取的字节数（不能超过剩余需求，也不能超过当前流剩余容量）
            int toRead = Math.Min(bytesRemaining, SegmentSize - (int)(_position % SegmentSize));

            // 记录在当前流中已经读取的字节数
            int bytesReadInThisStream = 0;
            // 循环读取当前流，直到读满 toRead 字节或流结束
            while (bytesReadInThisStream < toRead)
            {
                // 从当前流中读取数据，目标位置是缓冲区当前偏移加上已读取的部分
                int read = CurrentStream.Read(
                    buffer,
                    currentOffset + bytesReadInThisStream,
                    toRead - bytesReadInThisStream
                );

                // 如果读取到 0 字节，表示当前流已结束
                if (read == 0)
                {
                    // 更新全局位置（加上在当前流中已读取的字节数）
                    _position += bytesReadInThisStream;
                    // 返回总共已读取的字节数（可能少于请求的 count）
                    return totalBytesRead + bytesReadInThisStream;
                }

                // 累加在当前流中读取的字节数
                bytesReadInThisStream += read;
            }

            // 累加总读取字节数
            totalBytesRead += bytesReadInThisStream;
            // 减少剩余需要读取的字节数
            bytesRemaining -= bytesReadInThisStream;
            // 更新缓冲区偏移位置
            currentOffset += bytesReadInThisStream;
            // 更新全局位置
            _position += bytesReadInThisStream;
        }

        // 成功读取所有请求的字节数，返回 count
        return totalBytesRead;
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // 检查当前流是否支持读取操作
        if (!CanRead)
        {
            throw new NotSupportedException("流不可读");
        }

        // 记录总共已经读取的字节数
        int totalBytesRead = 0;
        // 当前写入缓冲区的偏移位置
        int currentOffset = offset;
        // 还需要读取的字节数
        int bytesRemaining = count;

        // 循环读取，直到满足请求的字节数或流结束
        while (bytesRemaining > 0)
        {
            // 本次在当前流中计划读取的字节数（不能超过剩余需求，也不能超过当前流剩余容量）
            int toRead = Math.Min(bytesRemaining, SegmentSize - (int)(_position % SegmentSize));

            // 记录在当前流中已经读取的字节数
            int bytesReadInThisStream = 0;
            // 循环读取当前流，直到读满 toRead 字节或流结束
            while (bytesReadInThisStream < toRead)
            {
                // 从当前流中读取数据，目标位置是缓冲区当前偏移加上已读取的部分
                int read = await CurrentStream.ReadAsync(
                    buffer,
                    currentOffset + bytesReadInThisStream,
                    toRead - bytesReadInThisStream
                );

                // 如果读取到 0 字节，表示当前流已结束
                if (read == 0)
                {
                    // 更新全局位置（加上在当前流中已读取的字节数）
                    _position += bytesReadInThisStream;
                    // 返回总共已读取的字节数（可能少于请求的 count）
                    return totalBytesRead + bytesReadInThisStream;
                }

                // 累加在当前流中读取的字节数
                bytesReadInThisStream += read;
            }

            // 累加总读取字节数
            totalBytesRead += bytesReadInThisStream;
            // 减少剩余需要读取的字节数
            bytesRemaining -= bytesReadInThisStream;
            // 更新缓冲区偏移位置
            currentOffset += bytesReadInThisStream;
            // 更新全局位置
            _position += bytesReadInThisStream;
        }

        // 成功读取所有请求的字节数，返回 count
        return totalBytesRead;
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (!CanWrite)
        {
            throw new NotSupportedException("流不可写");
        }

        int bytesRemaining = count;
        int currentOffset = offset;

        while (bytesRemaining > 0)
        { 
            // 本次写入的字节数
            int toWrite = Math.Min(bytesRemaining, SegmentSize - (int)(_position % SegmentSize));

            // 写入当前流
            CurrentStream.Write(buffer, currentOffset, toWrite);

            // 更新状态
            currentOffset += toWrite;
            bytesRemaining -= toWrite;
            _position += toWrite;
        }
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (!CanWrite)
            throw new NotSupportedException("流不可写");

        int bytesRemaining = count;
        int currentOffset = offset;

        while (bytesRemaining > 0)
        { 
            // 本次写入的字节数
            int toWrite = Math.Min(bytesRemaining, SegmentSize - (int)(_position % SegmentSize));

            // 异步写入当前流
            await CurrentStream.WriteAsync(buffer, currentOffset, toWrite, cancellationToken);

            // 更新状态
            currentOffset += toWrite;
            bytesRemaining -= toWrite;
            _position += toWrite;
        }
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        return Position = origin switch
        {
            _ when !CanSeek => throw new NotSupportedException("流不可跳转"),
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => throw new NotSupportedException("不支持从末尾定位"),
            _ => throw new ArgumentException("无效的 SeekOrigin", nameof(origin)),
        };
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 注意：不关闭任何流，因为流是循环使用的，应该由调用方管理生命周期
        }
        base.Dispose(disposing);
    }
}