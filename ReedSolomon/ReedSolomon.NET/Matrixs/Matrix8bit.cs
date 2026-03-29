using System;
using zms9110750.ReedSolomon.Galos;

namespace zms9110750.ReedSolomon.Matrixs
{
    /// <summary>
    /// 伽罗瓦域上的矩阵实现（byte 版本）
    /// </summary>
    public class Matrix8bit : IMatrix<byte>
    {
        private readonly byte[] _data;

        /// <inheritdoc/>
        public int Rows { get; }

        /// <inheritdoc/>
        public int Columns { get; }

        /// <inheritdoc/>
        public IGaloisField<byte> GaloisField { get; }

        /// <summary>
        /// 构造函数：创建指定大小的零矩阵
        /// </summary>
        public Matrix8bit(int rows, int columns, IGaloisField<byte>? gf = null)
        {
            if (rows <= 0)
                throw new ArgumentException("行数必须为正数", nameof(rows));
            if (columns <= 0)
                throw new ArgumentException("列数必须为正数", nameof(columns));

            Rows = rows;
            Columns = columns;
            GaloisField = gf ?? new GaloisField8bit();
            _data = new byte[rows * columns];
        }

        /// <summary>
        /// 私有构造函数：直接使用一维数组初始化（用于内部操作）
        /// </summary>
        private Matrix8bit(IGaloisField<byte> gf, byte[] data, int rows, int columns)
        {
            GaloisField = gf;
            Rows = rows;
            Columns = columns;
            _data = data;
        }

        /// <inheritdoc/>
        public byte this[int row, int col]
        {
            get
            {
                if (row < 0 || row >= Rows)
                    throw new ArgumentOutOfRangeException(nameof(row));
                if (col < 0 || col >= Columns)
                    throw new ArgumentOutOfRangeException(nameof(col));
                return _data[row * Columns + col];
            }
            private set
            {
                if (row < 0 || row >= Rows)
                    throw new ArgumentOutOfRangeException(nameof(row));
                if (col < 0 || col >= Columns)
                    throw new ArgumentOutOfRangeException(nameof(col));
                _data[row * Columns + col] = value;
            }
        }

        /// <inheritdoc/> 
        public ReadOnlySpan<byte> GetRow(int row)
        {
            if (row < 0 || row >= Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            return new ReadOnlySpan<byte>(_data, row * Columns, Columns);
        }

        /// <inheritdoc/>
        public IMatrix<byte> Multiply(IMatrix<byte> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (Columns != other.Rows)
                throw new InvalidOperationException($"矩阵维度不匹配：左矩阵列数 {Columns}，右矩阵行数 {other.Rows}");
            if (GaloisField.PrimitivePolynomial != other.GaloisField.PrimitivePolynomial)
                throw new InvalidOperationException("矩阵使用的伽罗瓦域不一致");

            var result = new Matrix8bit(Rows, other.Columns, GaloisField);

            for (int i = 0; i < Rows; i++)
            {
                int iBase = i * Columns;
                for (int j = 0; j < other.Columns; j++)
                {
                    byte sum = 0;
                    for (int k = 0; k < Columns; k++)
                    {
                        sum ^= GaloisField.Multiply(_data[iBase + k], other[k, j]);
                    }
                    result._data[i * other.Columns + j] = sum;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public IMatrix<byte> InverseSubMatrix(int rowStart, int colStart, int size)
        {
            if (rowStart < 0 || rowStart + size > Rows)
                throw new ArgumentOutOfRangeException(nameof(rowStart));
            if (colStart < 0 || colStart + size > Columns)
                throw new ArgumentOutOfRangeException(nameof(colStart));
            if (size <= 0)
                throw new ArgumentException("size 必须为正数", nameof(size));


            // 直接创建增广矩阵 [A | I]，从原矩阵复制数据
            var augData = new byte[size * size * 2];
            for (int i = 0; i < size; i++)
            {
                int srcOffset = (rowStart + i) * Columns + colStart;
                int iAugBase = i * size * 2;

                // 复制 A 的行
                Array.Copy(_data, srcOffset, augData, iAugBase, size);

                // 设置 I
                augData[iAugBase + size + i] = 1;
            }
            Span<byte> temp = stackalloc byte[size * 2];

            // 高斯消元
            for (int col = 0; col < size; col++)
            {
                // 找主元
                int pivot = col;
                while (pivot < size && augData[pivot * size * 2 + col] == 0)
                {
                    pivot++;
                }
                if (pivot == size)
                {
                    throw new InvalidOperationException("矩阵不可逆");
                }

                // 交换行
                if (pivot != col)
                {
                    int curRowStart = col * size * 2;
                    int pivotRowStart = pivot * size * 2;

                    augData.AsSpan(curRowStart, size * 2).CopyTo(temp);
                    augData.AsSpan(pivotRowStart, size * 2).CopyTo(augData.AsSpan(curRowStart, size * 2));
                    temp.CopyTo(augData.AsSpan(pivotRowStart, size * 2));
                }

                // 归一化当前行
                int curRowBase = col * size * 2;
                byte inv = GaloisField.Inverse(augData[curRowBase + col]);
                for (int j = 0; j < size * 2; j++)
                {
                    augData[curRowBase + j] = GaloisField.Multiply(augData[curRowBase + j], inv);
                }

                // 消去其他行
                for (int row = 0; row < size; row++)
                {
                    if (row != col)
                    {
                        int rowBase = row * size * 2;
                        byte factor = augData[rowBase + col];
                        if (factor != 0)
                        {
                            for (int j = 0; j < size * 2; j++)
                            {
                                augData[rowBase + j] ^= GaloisField.Multiply(factor, augData[curRowBase + j]);
                            }
                        }
                    }
                }
            }

            // 提取逆矩阵
            var resultData = new byte[size * size];
            for (int i = 0; i < size; i++)
            {
                int iBase = i * size;
                int iAugBase = i * size * 2;
                Array.Copy(augData, iAugBase + size, resultData, iBase, size);
            }

            return new Matrix8bit(GaloisField, resultData, size, size);
        }

        /// <summary>
        /// 创建范德蒙德矩阵
        /// </summary>
        public static IMatrix<byte> Vandermonde(int rows, int columns, IGaloisField<byte>? gf = null)
        {
            gf ??= new GaloisField8bit();
            var data = new byte[rows * columns];

            for (int i = 0; i < rows; i++)
            {
                int iBase = i * columns;
                for (int j = 0; j < columns; j++)
                {
                    data[iBase + j] = gf.Power((byte)i, j);
                }
            }

            return new Matrix8bit(gf, data, rows, columns);
        }

        /// <inheritdoc/>
        public IMatrix<byte> InverseRows(ReadOnlySpan<int> rowIndices, int size)
        {
            if (rowIndices.Length != size)
            {
                throw new ArgumentException($"rowIndices 长度 {rowIndices.Length} 必须等于 size {size}");
            }

            var subData = new byte[size * size];
            for (var i = 0; i < size; i++)
            {
                var row = rowIndices[i];
                var srcOffset = row * Columns;
                var dstOffset = i * size;
                Array.Copy(_data, srcOffset, subData, dstOffset, size);
            }

            var subMatrix = new Matrix8bit(GaloisField, subData, size, size);
            return subMatrix.InverseSubMatrix(0, 0, size);
        }
    }
}
