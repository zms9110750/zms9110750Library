using System;
using System.Collections.Generic;
using System.Linq;

namespace zms9110750.ReedSolomon.ReedSolomons
{
    /// <summary>
    /// Reed-Solomon 编解码器扩展方法
    /// </summary>
    public static class IReedSolomonExtensions
    {
        /// <summary>
        /// 编码：为数据分片生成冗余分片（全量版本）
        /// </summary>
        public static void EncodeParity(
            this IReedSolomon rs,
            IEnumerable<byte[]> dataShards,
            IEnumerable<byte[]> parityShards)
        {
            int byteCount = dataShards.First().Length;
            rs.EncodeParity(dataShards, parityShards, 0, byteCount);
        }

        /// <summary>
        /// 解码：恢复缺失的分片（全量版本）
        /// </summary>
        public static void DecodeMissing(
            this IReedSolomon rs,
            IEnumerable<byte[]> shards,
            ReadOnlySpan<bool> shardPresent)
        {
            int byteCount = shards.First().Length;
            rs.DecodeMissing(shards, shardPresent, 0, byteCount);
        }

        /// <summary>
        /// 校验冗余分片是否正确
        /// </summary>
        public static bool IsParityCorrect(
            this IReedSolomon rs,
            IEnumerable<byte[]> dataShards,
            IEnumerable<byte[]> parityShards)
        {
            var dataList = dataShards as IReadOnlyList<byte[]> ?? dataShards.ToArray();
            var parityList = parityShards as IReadOnlyList<byte[]> ?? parityShards.ToArray();

            if (dataList.Count != rs.DataShardCount)
            {
                throw new ArgumentException($"数据分片数量错误，期望 {rs.DataShardCount}，实际 {dataList.Count}");
            }
            if (parityList.Count != rs.ParityShardCount)
            {
                throw new ArgumentException($"冗余分片数量错误，期望 {rs.ParityShardCount}，实际 {parityList.Count}");
            }

            int shardSize = dataList[0].Length;
            const int blockSize = 1024 * 1024;
            int blocks = (shardSize + blockSize - 1) / blockSize;

            byte[][] tempOutputs = System.Buffers.ArrayPool<byte[]>.Shared.Rent(rs.ParityShardCount);
            try
            {
                for (int i = 0; i < rs.ParityShardCount; i++)
                {
                    tempOutputs[i] = System.Buffers.ArrayPool<byte>.Shared.Rent(blockSize);
                }

                for (int block = 0; block < blocks; block++)
                {
                    int offset = block * blockSize;
                    int byteCount = Math.Min(blockSize, shardSize - offset);

                    rs.EncodeParity(dataList, tempOutputs, offset, byteCount);

                    for (int i = 0; i < rs.ParityShardCount; i++)
                    {
                        for (int j = 0; j < byteCount; j++)
                        {
                            if (tempOutputs[i][j] != parityList[i][offset + j])
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            finally
            {
                for (int i = 0; i < rs.ParityShardCount; i++)
                {
                    if (tempOutputs[i] != null)
                    {
                        System.Buffers.ArrayPool<byte>.Shared.Return(tempOutputs[i]);
                    }
                }
                System.Buffers.ArrayPool<byte[]>.Shared.Return(tempOutputs);
            }
        }
    }
}
