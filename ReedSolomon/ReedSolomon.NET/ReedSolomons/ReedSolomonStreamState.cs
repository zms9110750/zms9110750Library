namespace zms9110750.ReedSolomon.ReedSolomons
{
    /// <summary>
    /// Reed-Solomon 流式编码的状态机
    /// </summary>
    public struct ReedSolomonStreamState
    {
        /// <summary>
        /// 初始化状态机
        /// </summary>
        /// <param name="dataShardCount">数据分片数量（K）</param>
        /// <param name="parityShardCount">冗余分片数量（M）</param>
        /// <param name="totalLength">原始数据总长度（字节）</param>
        /// <param name="blockSize">每个块的大小（处理单元）。注意：实际块大小会取该值与分片大小的较小值</param> 
        public ReedSolomonStreamState(int dataShardCount, int parityShardCount, long totalLength, int blockSize)
        {
            DataShardCount = dataShardCount;
            ParityShardCount = parityShardCount;
            TotalLength = totalLength;
            BlockSize = Math.Min(ShardSize, blockSize);
        }

        /// <summary>
        /// 初始化状态机
        /// </summary>
        /// <param name="reedSolomon">Reed-Solomon 编解码器实例，用于获取数据分片数和冗余分片数</param>
        /// <param name="totalLength">原始数据总长度（字节）</param>
        /// <param name="blockSize">每个块的大小（处理单元）。注意：实际块大小会取该值与分片大小的较小值</param>
        public ReedSolomonStreamState(IReedSolomon reedSolomon, long totalLength, int blockSize) : this(reedSolomon.DataShardCount, reedSolomon.ParityShardCount, totalLength, blockSize)
        {
        }

        /// <summary>数据分片数量（K）</summary>
        public int DataShardCount { get; }

        /// <summary>冗余分片数量（M）</summary>
        public int ParityShardCount { get; }

        /// <summary>原始数据总长度（字节）</summary>
        public long TotalLength { get; }

        /// <summary>每个块的大小（处理单元）</summary>
        public int BlockSize { get; }

        /// <summary>当前已处理的原始数据字节数</summary>
        public long BytesProcessed { get; private set; }

        /// <summary>总分片数量（N = K + M）</summary>
        public int TotalShardCount => DataShardCount + ParityShardCount;

        /// <summary>每个分片的大小（向上取整）</summary>
        public int ShardSize => (int)((TotalLength + DataShardCount - 1) / DataShardCount);

        /// <summary>当前块在分片内的偏移位置</summary>
        public int BlockOffset => (int)(BytesProcessed / DataShardCount);

        /// <summary>当前块的实际字节数</summary>
        public int CurrentBlockSize => Math.Min(BlockSize, ShardSize - BlockOffset);

        /// <summary>当前块需要读取的原始数据字节数</summary>
        public int CurrentReadSize => (int)Math.Min(CurrentBlockSize * DataShardCount, TotalLength - BytesProcessed);

        /// <summary>是否还有数据需要处理</summary>
        public bool HasRemaining => BytesProcessed < TotalLength;

        /// <summary>总块数</summary>
        public int TotalBlocks => (ShardSize + BlockSize - 1) / BlockSize;

        /// <summary>当前块索引（第几个块）</summary>
        public int CurrentBlockIndex => BlockOffset / BlockSize;

        /// <summary>
        /// 更新已处理的字节数
        /// </summary>
        /// <param name="bytesRead">本次读取的字节数</param>
        public void UpdateProgress(int bytesRead)
        {
            BytesProcessed += bytesRead;
        }
    }
}