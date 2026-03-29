using System.Collections.Generic;
using zms9110750.ReedSolomon.Matrixs;

namespace zms9110750.ReedSolomon.CodingLoop
{
    /// <summary>
    /// 编码循环接口。定义了如何遍历数据并执行 RS 编码。
    /// </summary>
    public interface ICodingLoop<T> where T : unmanaged
    {
        /// <summary>
        /// 本原多项式（低 m 位系数）。
        /// </summary>
        T PrimitivePolynomial { get; }

        /// <summary>
        /// 执行编码核心运算：output = matrixRows × inputs
        /// </summary>
        /// <param name="matrixRows">编码矩阵（m × k）</param>
        /// <param name="startRow">从矩阵的第几行开始取</param>
        /// <param name="rowCount">取多少行（即 m）</param>
        /// <param name="inputs">输入分片（数据分片），每个分片是 byte[]</param>
        /// <param name="outputs">输出分片（冗余分片），每个分片是 byte[]，会被修改</param>
        /// <param name="offset">每个分片的起始字节索引</param>
        /// <param name="byteCount">要处理的字节数</param>
        void CodeSomeShards(
            IMatrix<T> matrixRows,
            int startRow,
            int rowCount,
            IEnumerable<byte[]> inputs,
            IEnumerable<byte[]> outputs,
            int offset,
            int byteCount);
    }
}
