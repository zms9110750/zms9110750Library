using System;
using zms9110750.ReedSolomon.Galos;

namespace zms9110750.ReedSolomon.Matrixs
{
    /// <summary>
    /// 伽罗瓦域上的矩阵接口
    /// </summary>
    /// <typeparam name="T">域元素类型（byte, ushort, uint 等）</typeparam>
    public interface IMatrix<T> where T : unmanaged
    {
        /// <summary>
        /// 获取矩阵的行数
        /// </summary>
        int Rows { get; }

        /// <summary>
        /// 获取矩阵的列数
        /// </summary>
        int Columns { get; }

        /// <summary>
        /// 获取矩阵使用的伽罗瓦域
        /// </summary>
        IGaloisField<T> GaloisField { get; }

        /// <summary>
        /// 访问矩阵元素（只读）
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        T this[int row, int col] { get; }

        /// <summary>
        /// 获取矩阵的指定行（只读视图）
        /// </summary>
        /// <param name="row">行索引</param>
        /// <returns>该行的只读视图</returns>
        ReadOnlySpan<T> GetRow(int row);

        /// <summary>
        /// 矩阵乘法。返回 this × other
        /// </summary>
        /// <param name="other">右边的乘数矩阵</param>
        /// <returns>乘积矩阵</returns>
        /// <exception cref="InvalidOperationException">当 Columns != other.Rows 时抛出</exception>
        /// <exception cref="ArgumentNullException">当 other 为 null 时抛出</exception>
        IMatrix<T> Multiply(IMatrix<T> other);

        /// <summary>
        /// 从当前矩阵中裁剪出一个子方阵并求逆。
        /// 等价于 SubMatrix(rowStart, colStart, rowStart + size, colStart + size).Inverse()
        /// </summary>
        /// <param name="rowStart">子矩阵起始行索引</param>
        /// <param name="colStart">子矩阵起始列索引</param>
        /// <param name="size">子矩阵大小</param>
        /// <returns>子方阵的逆矩阵</returns>
        IMatrix<T> InverseSubMatrix(int rowStart, int colStart, int size);

        /// <summary>
        /// 根据指定的行索引构建子矩阵并求逆
        /// </summary>
        IMatrix<T> InverseRows(ReadOnlySpan<int> rowIndices, int size);
    }
}
