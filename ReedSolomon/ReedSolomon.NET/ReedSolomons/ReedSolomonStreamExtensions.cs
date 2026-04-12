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
        /// 优化版本：流式处理，内存中只保留当前块的数据。
        /// </summary>
        /// <param name="rs">Reed-Solomon 编解码器</param>
        /// <param name="dataStream">输入数据流</param>
        /// <param name="totalLength">总数据长度（从外部获取，如 Content-Length）</param>
        /// <param name="outputStreams">输出流集合，数量等于 TotalShardCount</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task EncodeParityAsync(
            this IReedSolomon rs,
            Stream dataStream,
            long totalLength,
            IReadOnlyList<Stream> outputStreams,
            CancellationToken cancellationToken = default)
        {
            // 验证输出流数量是否与总分片数匹配
            if (outputStreams == null || outputStreams.Count != rs.TotalShardCount)
            {
                throw new ArgumentException("outputStreams 数量必须等于 TotalShardCount");
            }


            // 空文件处理：直接返回，不进行任何编码操作
            if (totalLength == 0)
            {
                return;
            }
            // 初始化状态机
            var state = new ReedSolomonStreamState(
                reedSolomon: rs,
                totalLength: totalLength,
                blockSize: 4 * 1024 * 1024 / rs.DataShardCount
            );


            // 读取缓冲区：从输入流读取原始数据的临时缓冲区
            byte[] readBuffer = ArrayPool<byte>.Shared.Rent(state.BlockSize * state.DataShardCount);

            // 为数据分片申请内存：每个数据分片只需要保存一个块的数据
            byte[][] dataShards = new byte[state.DataShardCount][];
            for (int i = 0; i < state.DataShardCount; i++)
            {
                dataShards[i] = ArrayPool<byte>.Shared.Rent(state.BlockSize);
            }

            // 为冗余分片申请内存
            byte[][] parityShards = new byte[state.ParityShardCount][];
            for (int i = 0; i < state.ParityShardCount; i++)
            {
                parityShards[i] = ArrayPool<byte>.Shared.Rent(state.BlockSize);
            }

            try
            {
                // 使用状态机驱动循环
                while (state.HasRemaining)
                {
                    // 循环读取直到读满所需字节数 
                    for (int totalBytesRead = 0, bytesRead; totalBytesRead < state.CurrentReadSize; totalBytesRead += bytesRead)
                    {
                        bytesRead = await dataStream.ReadAsync(
                                        readBuffer, totalBytesRead, state.CurrentReadSize - totalBytesRead, cancellationToken
                                    ).ConfigureAwait(false);

                        if (bytesRead == 0)
                        {
                            throw new EndOfStreamException(
                                $"流意外结束。预期读取 {state.CurrentReadSize} 字节，实际只读取到 {totalBytesRead} 字节。"
                                + $"当前总进度：{state.BytesProcessed + totalBytesRead}/{totalLength} 字节"
                            );
                        }
                    }


                    // 数据交织：将读取的原始数据按列优先顺序填入各数据分片 
                    for (int bytePos = 0; bytePos < state.CurrentReadSize; bytePos++)
                    {
                        int shardOffset = Math.DivRem(bytePos, state.DataShardCount, out int shardIndex);
                        dataShards[shardIndex][shardOffset] = readBuffer[bytePos];
                    }

                    // 如果实际读取的字节数小于预期，将未填充的位置补零 
                    int filledRows = Math.DivRem(state.CurrentReadSize, state.DataShardCount, out int lastRowFilled);

                    if (lastRowFilled > 0)
                    {
                        for (int shardIndex = lastRowFilled; shardIndex < state.DataShardCount; shardIndex++)
                        {
                            dataShards[shardIndex][filledRows] = 0;
                        }
                    }

                    // 执行 RS 编码
                    rs.EncodeParity(dataShards, parityShards, 0, state.CurrentBlockSize);

                    // 将所有数据分片的当前块写入对应的输出流
                    for (int i = 0; i < state.DataShardCount; i++)
                    {
                        await outputStreams[i].WriteAsync(
                            dataShards[i], 0, state.CurrentBlockSize, cancellationToken
                        ).ConfigureAwait(false);
                    }

                    // 将所有冗余分片的当前块写入对应的输出流
                    for (int i = 0; i < state.ParityShardCount; i++)
                    {
                        await outputStreams[state.DataShardCount + i].WriteAsync(
                            parityShards[i], 0, state.CurrentBlockSize, cancellationToken
                        ).ConfigureAwait(false);
                    }

                    // 更新状态机进度
                    state.UpdateProgress(state.CurrentReadSize);
                }
            }
            finally
            {
                // 归还所有缓冲区
                ArrayPool<byte>.Shared.Return(readBuffer);

                for (int i = 0; i < state.DataShardCount; i++)
                {
                    if (dataShards[i] != null)
                    {
                        ArrayPool<byte>.Shared.Return(dataShards[i]);
                    }
                }

                for (int i = 0; i < state.ParityShardCount; i++)
                {
                    if (parityShards[i] != null)
                    {
                        ArrayPool<byte>.Shared.Return(parityShards[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 从输入流组读取所有分片，自动恢复缺失分片，写入数据分片到输出流。
        /// </summary>
        /// <param name="rs">Reed-Solomon 编解码器</param>
        /// <param name="inputStreams">输入流集合，数量等于 TotalShardCount，null 表示缺失</param>
        /// <param name="outputStream">输出数据流（只写数据分片）</param>
        /// <param name="totalLength">总数据长度（从外部获取，如 Content-Length）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DecodeMissingAsync(
            this IReedSolomon rs,
            IReadOnlyList<Stream?> inputStreams,
            Stream outputStream,
            long totalLength,
            CancellationToken cancellationToken = default)
        {
            // 总分片数（N）：包含数据和冗余的所有分片
            int totalShardCount = rs.TotalShardCount;

            // 数据分片数（K）：需要恢复的原始数据分片数量
            int dataShardCount = rs.DataShardCount;

            // 验证输入流数量是否与总分片数匹配
            if (inputStreams == null || inputStreams.Count != totalShardCount)
            {
                throw new ArgumentException($"inputStreams 数量必须等于 TotalShardCount，期望 {totalShardCount}，实际 {inputStreams?.Count}");
            }

            // 空文件处理：写入空数据后返回
            if (totalLength == 0)
            {
                await outputStream.WriteAsync(Array.Empty<byte>(), 0, 0, cancellationToken).ConfigureAwait(false);
                return;
            }

            // 分片存在标志数组：标记每个分片是否可用（true=存在，false=缺失）
            bool[] shardPresent = new bool[totalShardCount];
            for (int i = 0; i < totalShardCount; i++)
            {
                shardPresent[i] = inputStreams[i] != null;
            }

            // 分片大小：每个分片包含的字节数（向上取整）
            int shardSize = (int)Math.Ceiling((double)totalLength / dataShardCount);

            // 块大小：每次处理的数据块大小（1MB 或分片大小，取较小值）
            int blockSize = Math.Min(1024 * 1024, shardSize);

            // 块数量：每个分片需要分成多少块来处理
            int blocks = (shardSize + blockSize - 1) / blockSize;

            // 共享数组池：用于复用字节数组
            var pool = ArrayPool<byte>.Shared;

            // 所有分片的缓冲区数组：存储所有分片（包括数据和冗余）的字节数组
            var shards = new byte[totalShardCount][];

            try
            {
                // 为所有分片从池中分配内存
                for (int i = 0; i < totalShardCount; i++)
                {
                    shards[i] = pool.Rent(shardSize);
                }

                // 设置输出流的长度（预分配文件空间）
                outputStream.SetLength(totalLength);

                // 重置输出流的位置到开头
                outputStream.Position = 0;

                // 写入缓冲区：用于重组原始数据
                // 大小 = 块大小 × 数据分片数
                byte[] writeBuffer = pool.Rent(blockSize * dataShardCount);

                try
                {
                    // 逐块处理：对每个块进行解码
                    for (int block = 0; block < blocks; block++)
                    {
                        // 当前块的偏移量
                        int offset = block * blockSize;

                        // 当前块的字节数（最后一块可能小于 blockSize）
                        int byteCount = Math.Min(blockSize, shardSize - offset);

                        // 读取所有可用的分片数据
                        for (int i = 0; i < totalShardCount; i++)
                        {
                            if (shardPresent[i])
                            {
                                // 定位到当前块的起始位置
                                inputStreams[i]!.Position = offset;

                                // 已读取的字节数
                                int read = 0;

                                // 循环读取直到读完当前块的所有字节
                                while (read < byteCount)
                                {
                                    // 单次读取的字节数
                                    int r = await inputStreams[i]!.ReadAsync(shards[i], offset + read, byteCount - read, cancellationToken).ConfigureAwait(false);
                                    if (r == 0)
                                    {
                                        break;
                                    }
                                    read += r;
                                }

                                // 如果读取的字节数不足，用0填充剩余部分
                                if (read < byteCount)
                                {
                                    Array.Clear(shards[i], offset + read, byteCount - read);
                                }
                            }
                            else
                            {
                                // 缺失的分片用0填充（占位符）
                                Array.Clear(shards[i], offset, byteCount);
                            }
                        }

                        // 执行RS解码：恢复缺失的分片数据
                        rs.DecodeMissing(shards, shardPresent, offset, byteCount);

                        // 需要写入的字节数（不超过总长度）
                        int bytesToWrite = (int)Math.Min(blockSize * dataShardCount, totalLength - block * blockSize * dataShardCount);

                        // 数据重交织：从各分片中提取原始数据顺序
                        for (int bytePos = 0; bytePos < bytesToWrite; bytePos++)
                        {
                            // 分片索引：当前字节来自哪个数据分片
                            int shardIndex = bytePos % dataShardCount;

                            // 分片内偏移：在当前分片中的位置
                            int shardOffset = offset + (bytePos / dataShardCount);

                            // 将字节复制到写入缓冲区
                            writeBuffer[bytePos] = shards[shardIndex][shardOffset];
                        }

                        // 将恢复的原始数据写入输出流
                        await outputStream.WriteAsync(writeBuffer, 0, bytesToWrite, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    // 归还写入缓冲区到数组池
                    pool.Return(writeBuffer);
                }
            }
            finally
            {
                // 归还所有分片缓冲区
                for (int i = 0; i < totalShardCount; i++)
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