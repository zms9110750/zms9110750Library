using System;
using System.Collections.Generic;
using System.Linq;
using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.Matrixs;

namespace zms9110750.ReedSolomon.CodingLoop
{
    /// <summary>
    /// 简单编码循环实现
    /// </summary>
    public class SimpleCodingLoopByte : ICodingLoop<byte>
    {
        /// <inheritdoc/>
        public byte PrimitivePolynomial { get; }

        public SimpleCodingLoopByte(IGaloisField<byte> gf)
        {
            PrimitivePolynomial = gf.PrimitivePolynomial;
        }

        /// <inheritdoc/>
        public void CodeSomeShards(
            IMatrix<byte> matrixRows,
            int startRow,
            int rowCount,
            IEnumerable<byte[]> inputs,
            IEnumerable<byte[]> outputs,
            int offset,
            int byteCount)
        {
            var gf = matrixRows.GaloisField;
            var inputList = inputs.ToList();
            var outputList = outputs.ToList();
            int inputCount = inputList.Count;

            Span<byte> sums = stackalloc byte[rowCount];
            for (int iByte = offset; iByte < offset + byteCount; iByte++)
            {
                sums.Clear();
                for (int row = 0; row < rowCount; row++)
                {
                    ref var sum = ref sums[row];
                    for (int col = 0; col < inputCount; col++)
                    {
                        sum ^= gf.Multiply(matrixRows[startRow + row, col], inputList[col][iByte]);
                    }
                    outputList[row][iByte] = sum;
                }
            }
        }
    }
}
