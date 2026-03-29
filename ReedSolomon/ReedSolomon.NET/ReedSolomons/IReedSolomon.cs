namespace zms9110750.ReedSolomon.ReedSolomons
{
    /// <summary>
    /// Reed-Solomon 编解码器接口（非泛型，用于扩展方法）
    /// </summary>
    public interface IReedSolomon
    {
        /// <summary>
        /// 数据分片数量 k
        /// </summary>
        int DataShardCount { get; }

        /// <summary>
        /// 冗余分片数量 m
        /// </summary>
        int ParityShardCount { get; }

        /// <summary>
        /// 总分片数量 n = k + m
        /// </summary>
        int TotalShardCount { get; }

        /// <summary>
        /// 编码：为数据分片生成冗余分片（分片处理版本）
        /// </summary>
        /// <param name="dataShards">数据分片，k 个，每个是 byte[]，只读</param>
        /// <param name="parityShards">冗余分片，m 个，每个是 byte[]，会被写入</param>
        /// <param name="offset">每个分片的起始字节索引</param>
        /// <param name="byteCount">要处理的字节数</param>
        void EncodeParity(IEnumerable<byte[]> dataShards, IEnumerable<byte[]> parityShards, int offset, int byteCount);

        /// <summary>
        /// 解码：恢复缺失的分片（分片处理版本）
        /// </summary>
        /// <param name="shards">所有分片，n 个，每个是 byte[]，缺失的分片会被填充</param>
        /// <param name="shardPresent">标记哪些分片存在，长度 n</param>
        /// <param name="offset">每个分片的起始字节索引</param>
        /// <param name="byteCount">要处理的字节数</param>
        void DecodeMissing(IEnumerable<byte[]> shards, ReadOnlySpan<bool> shardPresent, int offset, int byteCount);
    }
    /// <summary>
    /// Reed-Solomon 编解码器接口（泛型）
    /// </summary>
    /// <typeparam name="T">域元素类型（byte, ushort, uint 等）</typeparam>
    public interface IReedSolomon<T> : IReedSolomon where T : unmanaged
    {
        /// <summary>
        /// 本原多项式（低 m 位系数）
        /// </summary>
        T PrimitivePolynomial { get; }
    }
}
