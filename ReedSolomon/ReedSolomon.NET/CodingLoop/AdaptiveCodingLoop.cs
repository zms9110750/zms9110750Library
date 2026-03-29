using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using zms9110750.ReedSolomon.Matrixs;

namespace zms9110750.ReedSolomon.CodingLoop
{
    /// <summary>
    /// 确定性编码循环。始终选择累计执行时间最少的循环。
    /// 每次选择时立即对该循环增加保底时间，避免并发时全选同一个。
    /// </summary>
    /// <typeparam name="T">域元素类型（byte, ushort, uint 等）</typeparam>
    public class DeterministicCodingLoop<T> : ICodingLoop<T> where T : unmanaged
    {
        /// <summary>
        /// 每个循环的累计执行时间（毫秒）
        /// </summary>
        private readonly Dictionary<ICodingLoop<T>, TimeSpan> _totalTimes;

        /// <summary>
        /// 读写锁，保护统计数据的并发访问
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        /// <inheritdoc/>
        public T PrimitivePolynomial { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loops">候选编码循环列表</param>
        public DeterministicCodingLoop(params ICodingLoop<T>[] loops)
        {
            if (loops == null || loops.Length == 0)
            {
                throw new ArgumentException("至少需要一个编码循环", nameof(loops));
            }

            // 验证语义一致性
            var first = loops[0];
            PrimitivePolynomial = first.PrimitivePolynomial;
            for (int i = 1; i < loops.Length; i++)
            {
                if (loops[i].PrimitivePolynomial.ToString() != PrimitivePolynomial.ToString())
                {
                    throw new ArgumentException($"循环 {i} 的本原多项式不一致");
                }
            }

            _totalTimes = loops.ToDictionary(l => l, l => TimeSpan.Zero);
            _lock = new ReaderWriterLockSlim();
        }

        /// <inheritdoc/>
        public void CodeSomeShards(
             IMatrix<T> matrixRows,
            int startRow,
            int rowCount,
            IEnumerable<byte[]> inputs,
            IEnumerable<byte[]> outputs,
            int offset,
            int byteCount)
        {
            // 1. 选择并立即加保底时间
            var selected = SelectAndAddPenalty();

            // 2. 执行
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            selected.Key.CodeSomeShards(matrixRows, startRow, rowCount, inputs, outputs, offset, byteCount);
            stopwatch.Stop();

            // 3. 加上实际执行时间
            AddExecutionTime(selected.Key, stopwatch.Elapsed);
        }

        /// <summary>
        /// 选择累计时间最少的循环，并立即增加保底时间（1毫秒）
        /// </summary>
        private KeyValuePair<ICodingLoop<T>, TimeSpan> SelectAndAddPenalty()
        {
            _lock.EnterWriteLock();
            try
            {
                // 找到累计时间最少的
                var selected = _totalTimes.OrderBy(kv => kv.Value).First();

                // 立即加保底时间
                _totalTimes[selected.Key] = selected.Value + TimeSpan.FromMilliseconds(1);

                return selected;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 增加实际执行时间
        /// </summary>
        private void AddExecutionTime(ICodingLoop<T> loop, TimeSpan elapsedMs)
        {
            _lock.EnterWriteLock();
            try
            {
                _totalTimes[loop] += elapsedMs;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
