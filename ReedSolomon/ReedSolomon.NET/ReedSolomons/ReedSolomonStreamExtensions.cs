using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace zms9110750.ReedSolomon.ReedSolomons
{
    /// <summary>
    /// 针对 IReedSolomon 的流式扩展方法，支持高效分片与内存复用。
    /// </summary>
    public static class ReedSolomonStreamExtensions
    {

        /// <summary>
        /// 从输入流读取数据，编码后将所有分片（数据+冗余）分别写入对应输出流。
        /// </summary>
        /// <typeparam name="T">域元素类型（byte, ushort, uint 等）</typeparam>
        /// <param name="rs">Reed-Solomon 编解码器</param>
        /// <param name="dataStream">输入数据流</param>
        /// <param name="totalLength">总数据长度（从外部获取，如 Content-Length）。</param>
        /// <param name="outputStreams">输出流集合，数量等于 TotalShardCount</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task EncodeParityAsync<T>(
            this IReedSolomon<T> rs,
            Stream dataStream,
            long totalLength,
            IReadOnlyList<Stream> outputStreams, 
            CancellationToken cancellationToken = default)
            where T : unmanaged
        {
            int k = rs.DataShardCount;
            int m = rs.ParityShardCount;
            int n = rs.TotalShardCount;

            if (outputStreams == null || outputStreams.Count != n)
            {
                throw new ArgumentException("outputStreams 数量必须等于 TotalShardCount");
            }

            if (totalLength == 0)
            {
                for (int i = 0; i < n; i++)
                {
                    await outputStreams[i].WriteAsync(Array.Empty<byte>(), 0, 0, cancellationToken).ConfigureAwait(false);
                }
                return;
            }

            int shardSize = (int)Math.Ceiling((double)totalLength / k);
            int blockSize = Math.Min(1024 * 1024, shardSize);
            int blocks = (shardSize + blockSize - 1) / blockSize;

            var pool = ArrayPool<byte>.Shared;
            var dataShards = new byte[k][];
            var parityShards = new byte[m][];

            try
            {
                for (int i = 0; i < k; i++)
                {
                    dataShards[i] = pool.Rent(shardSize);
                }
                for (int i = 0; i < m; i++)
                {
                    parityShards[i] = pool.Rent(shardSize);
                }

                byte[] readBuffer = pool.Rent(blockSize * k);

                try
                {
                    long bytesProcessed = 0;
                    int blockIndex = 0;

                    while (bytesProcessed < totalLength)
                    {
                        int blockOffset = blockIndex * blockSize;
                        int currentBlockSize = Math.Min(blockSize, shardSize - blockOffset);
                        if (currentBlockSize <= 0)
                        {
                            break;
                        }

                        int bytesToRead = (int)Math.Min(blockSize * k, totalLength - bytesProcessed);
                        int bytesRead = await dataStream.ReadAsync(readBuffer, 0, bytesToRead, cancellationToken).ConfigureAwait(false);
                        if (bytesRead <= 0)
                        {
                            break;
                        }

                        int maxBytesToFill = currentBlockSize * k;
                        int bytesToFill = Math.Min(bytesRead, maxBytesToFill);

                        for (int bytePos = 0; bytePos < bytesToFill; bytePos++)
                        {
                            int shardIndex = bytePos % k;
                            int shardOffset = blockOffset + (bytePos / k);
                            dataShards[shardIndex][shardOffset] = readBuffer[bytePos];
                        }

                        var dataShardsBlock = new List<byte[]>();
                        for (int i = 0; i < k; i++)
                        {
                            dataShardsBlock.Add(dataShards[i]);
                        }
                        var parityShardsBlock = new List<byte[]>();
                        for (int i = 0; i < m; i++)
                        {
                            parityShardsBlock.Add(parityShards[i]);
                        }

                        rs.EncodeParity(dataShardsBlock, parityShardsBlock, blockOffset, currentBlockSize);

                        bytesProcessed += bytesRead;
                        blockIndex++;
                    }

                    for (int i = 0; i < k; i++)
                    {
                        await outputStreams[i].WriteAsync(dataShards[i], 0, shardSize, cancellationToken).ConfigureAwait(false);
                    }
                    for (int i = 0; i < m; i++)
                    {
                        await outputStreams[k + i].WriteAsync(parityShards[i], 0, shardSize, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    pool.Return(readBuffer);
                }
            }
            finally
            {
                for (int i = 0; i < k; i++)
                {
                    if (dataShards[i] != null)
                    {
                        pool.Return(dataShards[i]);
                    }
                }
                for (int i = 0; i < m; i++)
                {
                    if (parityShards[i] != null)
                    {
                        pool.Return(parityShards[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 从输入流组读取所有分片，自动恢复缺失分片，写入数据分片到输出流。
        /// </summary>
        /// <typeparam name="T">域元素类型</typeparam>
        /// <param name="rs">Reed-Solomon 编解码器</param>
        /// <param name="inputStreams">输入流集合，数量等于 TotalShardCount，null 表示缺失</param>
        /// <param name="outputStream">输出数据流（只写数据分片）</param>
        /// <param name="totalLength">总数据长度（从外部获取，如 Content-Length）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DecodeMissingAsync<T>(
            this IReedSolomon<T> rs,
            IReadOnlyList<Stream?> inputStreams,
            Stream outputStream,
            long totalLength,
            CancellationToken cancellationToken = default)
            where T : unmanaged
        {
            int n = rs.TotalShardCount;
            int k = rs.DataShardCount;

            if (inputStreams == null || inputStreams.Count != n)
            {
                throw new ArgumentException($"inputStreams 数量必须等于 TotalShardCount，期望 {n}，实际 {inputStreams?.Count}");
            }

            if (totalLength == 0)
            {
                await outputStream.WriteAsync(Array.Empty<byte>(), 0, 0, cancellationToken).ConfigureAwait(false);
                return;
            }

            // 构建 shardPresent Span
            bool[] shardPresent = new bool[n];
            for (int i = 0; i < n; i++)
            {
                shardPresent[i] = inputStreams[i] != null;
            }

            int shardSize = (int)Math.Ceiling((double)totalLength / k);
            int blockSize = Math.Min(1024 * 1024, shardSize);
            int blocks = (shardSize + blockSize - 1) / blockSize;

            var pool = ArrayPool<byte>.Shared;
            var shards = new byte[n][];

            try
            {
                for (int i = 0; i < n; i++)
                {
                    shards[i] = pool.Rent(shardSize);
                }

                outputStream.SetLength(totalLength);
                outputStream.Position = 0;

                byte[] writeBuffer = pool.Rent(blockSize * k);

                try
                {
                    for (int block = 0; block < blocks; block++)
                    {
                        int offset = block * blockSize;
                        int byteCount = Math.Min(blockSize, shardSize - offset);

                        for (int i = 0; i < n; i++)
                        {
                            if (shardPresent[i])
                            {
                                inputStreams[i]!.Position = offset;
                                int read = 0;
                                while (read < byteCount)
                                {
                                    int r = await inputStreams[i]!.ReadAsync(shards[i], offset + read, byteCount - read, cancellationToken).ConfigureAwait(false);
                                    if (r == 0)
                                    {
                                        break;
                                    }
                                    read += r;
                                }
                                if (read < byteCount)
                                {
                                    Array.Clear(shards[i], offset + read, byteCount - read);
                                }
                            }
                            else
                            {
                                Array.Clear(shards[i], offset, byteCount);
                            }
                        }

                        rs.DecodeMissing(shards, shardPresent, offset, byteCount);

                        int bytesToWrite = (int)Math.Min(blockSize * k, totalLength - block * blockSize * k);
                        for (int bytePos = 0; bytePos < bytesToWrite; bytePos++)
                        {
                            int shardIndex = bytePos % k;
                            int shardOffset = offset + (bytePos / k);
                            writeBuffer[bytePos] = shards[shardIndex][shardOffset];
                        }

                        await outputStream.WriteAsync(writeBuffer, 0, bytesToWrite, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    pool.Return(writeBuffer);
                }
            }
            finally
            {
                for (int i = 0; i < n; i++)
                {
                    if (shards[i] != null)
                    {
                        pool.Return(shards[i]);
                    }
                }
            }
        }
    }
}
