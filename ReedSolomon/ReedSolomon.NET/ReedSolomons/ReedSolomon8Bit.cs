/*
namespace zms9110750.ReedSolomon.ReedSolomons
{

    /// <summary>
    /// Reed-Solomon 编解码器实现（byte 版本）
    /// </summary>
    public class ReedSolomon8Bit : IReedSolomon<byte>
    {
        private readonly IGaloisField<byte> _gf;
        private readonly IMatrix<byte> _encodingMatrix;
        private readonly ICodingLoop<byte> _codingLoop;

        /// <inheritdoc/>
        public int DataShardCount { get; }

        /// <inheritdoc/>
        public int ParityShardCount { get; }

        /// <inheritdoc/>
        public byte PrimitivePolynomial => _gf.PrimitivePolynomial;

        /// <inheritdoc/> 
        public int TotalShardCount => DataShardCount + ParityShardCount;

        /// <summary>
        /// 构造函数。8bit的实现，总分片数不能超过256（2^8）。
        /// </summary>
        /// <param name="dataShards">数据分片数</param>
        /// <param name="parityShards">冗余分片数</param>
        /// <param name="gf">可选的 Galois 字段实现</param>
        /// <param name="codingLoop">可选的编码循环实现</param>
        public ReedSolomon8Bit(int dataShards, int parityShards, IGaloisField<byte>? gf = null, ICodingLoop<byte>? codingLoop = null)
        {
            if (dataShards <= 0)
            {
                throw new ArgumentException("数据分片数必须为正数", nameof(dataShards));
            }
            if (parityShards <= 0)
            {
                throw new ArgumentException("冗余分片数必须为正数", nameof(parityShards));
            }
            if (dataShards + parityShards > 256)
            {
                throw new ArgumentException($"总分片数不能超过 256，当前 {dataShards + parityShards}");
            }

            _gf = gf ?? new GaloisField8bit();
            DataShardCount = dataShards;
            ParityShardCount = parityShards;

            _encodingMatrix = BuildMatrix(dataShards, dataShards + parityShards, _gf);
            _codingLoop = codingLoop ?? new SimpleCodingLoopByte(_gf);

            static IMatrix<byte> BuildMatrix(int dataShards, int totalShards, IGaloisField<byte> gf)
            {
                var vandermonde = Matrix8bit.Vandermonde(totalShards, dataShards, gf);
                var topInverse = vandermonde.InverseSubMatrix(0, 0, dataShards);
                return vandermonde.Multiply(topInverse);
            }
        }

        /// <inheritdoc/>
        public void EncodeParity(IEnumerable<byte[]> dataShards, IEnumerable<byte[]> parityShards, int offset, int byteCount)
        {
            _codingLoop.CodeSomeShards(
                _encodingMatrix,
                DataShardCount,
                ParityShardCount,
                dataShards,
                parityShards,
                offset,
                byteCount);
        }
        /// <inheritdoc/>
        public void RecoverDataShards(IEnumerable<byte[]> shards, ReadOnlySpan<bool> shardPresent, int offset, int byteCount)
        {
            IList<byte[]> shardList = shards as IList<byte[]> ?? shards.ToArray();

            if (shardList.Count != TotalShardCount)
            {
                throw new ArgumentException($"分片数量错误，期望 {TotalShardCount}，实际 {shardList.Count}");
            }

            // 一次迭代：统计可用数量 + 收集可用分片 + 收集缺失的冗余分片
            int presentCount = 0;
            Span<int> rowIndices = stackalloc int[DataShardCount];

            // 从 ArrayPool 租用 subShards
            byte[][] subShards = System.Buffers.ArrayPool<byte[]>.Shared.Rent(DataShardCount);
            try
            {
                var missingParityRows = new List<byte[]>();
                var missingParityOutputs = new List<byte[]>();

                for (int i = 0, indices = 0; i < TotalShardCount; i++)
                {
                    if (shardPresent[i])
                    {
                        presentCount++;
                        if (indices < DataShardCount)
                        {
                            rowIndices[indices] = i;
                            subShards[indices] = shardList[i];
                            indices++;
                        }
                    }
                    else if (i >= DataShardCount)
                    {
                        var rowCoeffs = _encodingMatrix.GetRow(i).ToArray();
                        missingParityRows.Add(rowCoeffs);
                        missingParityOutputs.Add(shardList[i]);
                    }
                }

                if (presentCount == TotalShardCount)
                {
                    return;
                }
                if (presentCount < DataShardCount)
                {
                    throw new InvalidOperationException($"可用分片不足，需要至少 {DataShardCount} 个，实际 {presentCount}");
                }

                // 根据行索引构建子矩阵并求逆
                var inverse = _encodingMatrix.InverseRows(rowIndices, DataShardCount);

                // 使用逆矩阵的行直接恢复缺失的数据分片
                for (int i = 0; i < DataShardCount; i++)
                {
                    if (shardPresent[i])
                    {
                        continue;
                    }

                    var output = shardList[i];
                    for (int b = offset; b < offset + byteCount; b++)
                    {
                        byte sum = 0;
                        for (int j = 0; j < DataShardCount; j++)
                        {
                            sum ^= _gf.Multiply(inverse[i, j], subShards[j][b]);
                        }
                        output[b] = sum;
                    }
                }

                // 恢复缺失的冗余分片
                if (missingParityRows.Count > 0)
                {
                    var dataShards = shardList.Take(DataShardCount);
                    _codingLoop.CodeSomeShards(
                        _encodingMatrix,
                        DataShardCount,
                        missingParityRows.Count,
                        dataShards,
                        missingParityOutputs,
                        offset,
                        byteCount);
                }
            }
            finally
            {
                // 归还 subShards 数组（只归还外层引用数组，内层数据是 shardList 的引用，不归还）
                System.Buffers.ArrayPool<byte[]>.Shared.Return(subShards);
            }
        }
    }
}
*/