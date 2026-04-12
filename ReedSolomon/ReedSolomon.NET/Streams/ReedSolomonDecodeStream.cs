namespace zms9110750.ReedSolomon.Streams;

/// <summary>
/// Reed-Solomon 解码流：从多个分片流读取数据，解码后输出恢复的原始数据
/// </summary>
public sealed class ReedSolomonDecodeStream : Stream
{
    private readonly IMatrix _encodingMatrix;
    private readonly IReadOnlyList<Stream> _shardStreams;
    private readonly int _blockSize;
    private readonly int _dataShardCount;
    private readonly byte[] _inputBuffer;
    private readonly byte[] _outputBuffer;
    private readonly bool[] _shardPresent;
    private int _outputBufferFilled;
    private int _outputBufferReadPos;
    private long _position;
    private long _totalLength;
    private bool _isFinished;

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override long Length => _totalLength;

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// 初始化解码流
    /// </summary>
    /// <param name="encodingMatrix">编码矩阵</param>
    /// <param name="shardStreams">分片输入流集合，数量为 Rows（总分片数），缺失的流用 null 表示</param>
    /// <param name="blockSize">每个分片的字节数</param>
    /// <param name="totalLength">原始数据总长度</param>
    public ReedSolomonDecodeStream(IMatrix encodingMatrix, IReadOnlyList<Stream> shardStreams, int blockSize, long totalLength)
    {
        if (encodingMatrix == null)
            throw new ArgumentNullException(nameof(encodingMatrix));
        if (shardStreams == null)
            throw new ArgumentNullException(nameof(shardStreams));
        if (blockSize <= 0)
            throw new ArgumentException("blockSize 必须大于 0", nameof(blockSize));
        if (totalLength < 0)
            throw new ArgumentException("totalLength 不能为负数", nameof(totalLength));

        int totalShards = encodingMatrix.Rows;
        if (shardStreams.Count != totalShards)
        {
            throw new ArgumentException($"分片流数量应为 {totalShards}，实际 {shardStreams.Count}", nameof(shardStreams));
        }

        _encodingMatrix = encodingMatrix;
        _shardStreams = shardStreams;
        _blockSize = blockSize;
        _dataShardCount = encodingMatrix.Columns;
        _totalLength = totalLength;
        _inputBuffer = new byte[totalShards * blockSize];
        _outputBuffer = new byte[_dataShardCount * blockSize];
        _shardPresent = new bool[totalShards];
        _outputBufferFilled = 0;
        _outputBufferReadPos = 0;
        _position = 0;
        _isFinished = false;

        // 标记哪些分片存在
        for (int i = 0; i < totalShards; i++)
        {
            _shardPresent[i] = shardStreams[i] != null;
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_isFinished)
            return 0;

        int totalBytesRead = 0;
        int currentOffset = offset;
        int bytesRemaining = count;

        while (bytesRemaining > 0 && !_isFinished)
        {
            // 如果输出缓冲区有数据，直接读取
            if (_outputBufferReadPos < _outputBufferFilled)
            {
                int canRead = _outputBufferFilled - _outputBufferReadPos;
                int toRead = Math.Min(bytesRemaining, canRead);
                Buffer.BlockCopy(_outputBuffer, _outputBufferReadPos, buffer, currentOffset, toRead);
                _outputBufferReadPos += toRead;
                totalBytesRead += toRead;
                currentOffset += toRead;
                bytesRemaining -= toRead;
                _position += toRead;
                continue;
            }

            // 输出缓冲区已空，尝试解码下一个块
            bool hasMore = await DecodeNextBlockAsync(cancellationToken);
            if (!hasMore)
            {
                _isFinished = true;
                break;
            }
        }

        return totalBytesRead;
    }

    private async Task<bool> DecodeNextBlockAsync(CancellationToken cancellationToken)
    {
        // 检查是否还有数据需要处理
        if (_position >= _totalLength)
        {
            return false;
        }

        // 当前块的大小（最后一个块可能不完整）
        int currentBlockSize = (int)Math.Min(_blockSize, _totalLength - _position);

        // 从所有存在的分片流中读取当前块的数据
        for (int i = 0; i < _shardStreams.Count; i++)
        {
            if (_shardPresent[i])
            {
                var stream = _shardStreams[i];
                int offset = i * _blockSize;
                int bytesRead = 0;
                while (bytesRead < currentBlockSize)
                {
                    int read = await stream!.ReadAsync(_inputBuffer, offset + bytesRead, currentBlockSize - bytesRead, cancellationToken);
                    if (read == 0)
                        break;
                    bytesRead += read;
                }
                // 如果读取不足，补零
                if (bytesRead < currentBlockSize)
                {
                    Array.Clear(_inputBuffer, offset + bytesRead, currentBlockSize - bytesRead);
                }
            }
            else
            {
                // 缺失的分片，用零填充
                int offset = i * _blockSize;
                Array.Clear(_inputBuffer, offset, currentBlockSize);
            }
        }

        // 收集可用分片的行索引和数据
        var availableIndices = new List<int>();
        var availableData = new List<byte[]>();

        for (int i = 0; i < _shardStreams.Count && availableIndices.Count < _dataShardCount; i++)
        {
            if (_shardPresent[i])
            {
                availableIndices.Add(i);
                var data = new byte[currentBlockSize];
                Buffer.BlockCopy(_inputBuffer, i * _blockSize, data, 0, currentBlockSize);
                availableData.Add(data);
            }
        }

        if (availableIndices.Count < _dataShardCount)
        {
            throw new InvalidOperationException($"可用分片不足，需要 {_dataShardCount} 个，实际 {availableIndices.Count}");
        }

        // 构建恢复矩阵并解码
        var inverse = _encodingMatrix.InverseRows(availableIndices.ToArray(), _dataShardCount);

        // 准备连续内存输入
        byte[] availableBuffer = new byte[_dataShardCount * currentBlockSize];
        for (int i = 0; i < _dataShardCount; i++)
        {
            Buffer.BlockCopy(availableData[i], 0, availableBuffer, i * currentBlockSize, currentBlockSize);
        }

        // 解码
        byte[] recoveredBuffer = new byte[_dataShardCount * currentBlockSize];
        inverse.CodeShards(availableBuffer, recoveredBuffer, currentBlockSize);

        // 提取实际有效数据（只取到 totalLength 为止）
        int bytesToCopy = (int)Math.Min(_totalLength - _position, _dataShardCount * currentBlockSize);
        Buffer.BlockCopy(recoveredBuffer, 0, _outputBuffer, 0, bytesToCopy);
        _outputBufferFilled = bytesToCopy;
        _outputBufferReadPos = 0;

        return true;
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Flush() { }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();
}