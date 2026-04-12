using System.Buffers;
using System.IO.Pipelines;

namespace zms9110750.ReedSolomon.Streams;

/// <summary>
/// Reed-Solomon 编码流：将写入的数据编码后分散到多个冗余分片流
/// </summary>
public sealed class ReedSolomonEncodeStream : Stream
{
    private readonly IMatrix _encodingMatrix;
    private readonly Stream _hubStream;
    private readonly Pipe[] _pipes;
    private readonly Task _encodeTask;
    private readonly CancellationTokenSource _cts;
    private readonly List<Stream> _outputStreams;
    private readonly int _blockSize;
    private readonly int _dataShardCount;
    private readonly int _parityShardCount;
    private bool _disposed;
    private long _position;

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// 初始化编码流
    /// </summary>
    /// <param name="encodingMatrix">编码矩阵（非方阵）</param>
    /// <param name="outputStreams">冗余分片输出流集合，数量为 M</param>
    /// <param name="blockSize">每个分片的字节数</param>
    public ReedSolomonEncodeStream(IMatrix encodingMatrix, IEnumerable<Stream> outputStreams, int blockSize)
    {
        if (encodingMatrix == null)
            throw new ArgumentNullException(nameof(encodingMatrix));
        if (outputStreams == null)
            throw new ArgumentNullException(nameof(outputStreams));
        if (blockSize <= 0)
            throw new ArgumentException("blockSize 必须大于 0", nameof(blockSize));

        if (encodingMatrix.IsSquare)
        {
            throw new ArgumentException("编码矩阵不能是方阵，请使用非方阵的编码矩阵", nameof(encodingMatrix));
        }

        _encodingMatrix = encodingMatrix;
        _blockSize = blockSize;
        _dataShardCount = encodingMatrix.Columns;
        _parityShardCount = encodingMatrix.Rows - encodingMatrix.Columns;
        _outputStreams = outputStreams.ToList();
        _cts = new CancellationTokenSource();
        _position = 0;

        if (_outputStreams.Count != _parityShardCount)
        {
            throw new ArgumentException($"输出流数量应为 {_parityShardCount}，实际 {_outputStreams.Count}", nameof(outputStreams));
        }

        // 1. 创建 K 个 Pipe（对应 K 个数据分片）
        _pipes = new Pipe[_dataShardCount];
        var pipeStreams = new Stream[_dataShardCount];
        for (int i = 0; i < _dataShardCount; i++)
        {
            _pipes[i] = new Pipe();
            pipeStreams[i] = _pipes[i].Writer.AsStream();
        }

        // 2. 创建集线器流，每个数据分片写满 blockSize 后自动切换
        _hubStream = new StreamRoundRobin(pipeStreams, blockSize);

        // 3. 启动后台编码任务
        _encodeTask = Task.Run(() => EncodeLoopAsync(_cts.Token));
    }

    /// <summary>
    /// 后台编码循环：从 Pipe 读取数据，编码后写入输出流
    /// </summary>
    private async Task EncodeLoopAsync(CancellationToken ct)
    {
        var readers = _pipes.Select(p => p.Reader).ToArray();
        int blockBytes = _dataShardCount * _blockSize;
        var inputBuffer = new byte[blockBytes];
        var outputBuffer = new byte[_parityShardCount * _blockSize];

        try
        {
            while (!ct.IsCancellationRequested)
            {
                bool anyData = false;

                // 从每个数据分片的 Pipe 读取一个 block 的数据
                for (int col = 0; col < _dataShardCount; col++)
                {
                    int offset = col * _blockSize;
                    int read = 0;

                    while (read < _blockSize)
                    {
                        var result = await readers[col].ReadAsync(ct);
                        if (result.IsCompleted && result.Buffer.IsEmpty)
                        {
                            break;
                        }

                        anyData = true;
                        int toCopy = Math.Min(_blockSize - read, (int)result.Buffer.Length);
                        result.Buffer.Slice(0, toCopy).CopyTo(inputBuffer.AsSpan(offset + read, toCopy));
                        read += toCopy;
                        readers[col].AdvanceTo(result.Buffer.Slice(toCopy).Start);
                    }

                    // 如果读取不足，补零
                    if (read < _blockSize)
                    {
                        Array.Clear(inputBuffer, offset + read, _blockSize - read);
                    }
                }

                // 没有更多数据，退出循环
                if (!anyData)
                {
                    break;
                }

                // 执行 RS 编码
                _encodingMatrix.CodeShards(inputBuffer, outputBuffer, _blockSize);

                // 将编码结果写入各个冗余分片流
                for (int i = 0; i < _parityShardCount; i++)
                {
                    await _outputStreams[i].WriteAsync(outputBuffer.AsMemory(i * _blockSize, _blockSize).ToArray(), 0, _blockSize, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消，忽略
        }
        catch (Exception ex)
        {
            // 发生异常时，通知所有 Pipe 写入失败
            foreach (var pipe in _pipes)
            {
                pipe.Writer.Complete(ex);
            }
            throw;
        }
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ReedSolomonEncodeStream));

        await _hubStream.WriteAsync(buffer, offset, count, cancellationToken);
        _position += count;
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ReedSolomonEncodeStream));

        await _hubStream.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        FlushAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 完成编码，等待所有数据编码完成
    /// </summary>
    public async Task CompleteAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ReedSolomonEncodeStream));

        // 完成集线器流写入
        await _hubStream.FlushAsync();

        // 完成所有 Pipe 的写入
        foreach (var pipe in _pipes)
        {
            pipe.Writer.Complete();
        }

        // 等待编码任务完成
        await _encodeTask;

        // 刷新所有输出流
        foreach (var stream in _outputStreams)
        {
            await stream.FlushAsync();
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _cts.Cancel();
            _hubStream.Dispose();

            foreach (var pipe in _pipes)
            {
                pipe.Writer.Complete();
            }

            try
            {
                _encodeTask.Wait();
            }
            catch (AggregateException)
            {
                // 忽略取消异常
            }

            foreach (var stream in _outputStreams)
            {
                stream.Dispose();
            }

            _cts.Dispose();
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value)
        => throw new NotSupportedException();
}