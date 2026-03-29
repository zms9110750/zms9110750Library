using Xunit.Abstractions;
using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.Matrixs;

namespace ReedSolomon.NET.Tests.Matrixs
{
    /// <summary>
    /// 测试 Matrix8bit 类自己的特性
    /// </summary>
    public class Matrix8bitTests
    {
        private readonly ITestOutputHelper _output;

        public Matrix8bitTests(ITestOutputHelper output)
        {
            _output = output;
        }
        /// <summary>
        /// 测试构造器参数非法时抛出异常
        /// </summary>
        [Fact]
        public void Constructor_InvalidDimensions_ShouldThrow()
        {
            var gf = new GaloisField8bit();

            Assert.Throws<ArgumentException>(() => new Matrix8bit(0, 5, gf));
            Assert.Throws<ArgumentException>(() => new Matrix8bit(5, 0, gf));
            Assert.Throws<ArgumentException>(() => new Matrix8bit(-1, 5, gf));
        }

        /// <summary>
        /// 测试索引器越界抛出异常
        /// </summary>
        [Fact]
        public void Indexer_OutOfRange_ShouldThrow()
        {
            var gf = new GaloisField8bit();
            var m = new Matrix8bit(3, 4, gf);

            Assert.Throws<ArgumentOutOfRangeException>(() => m[-1, 0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => m[0, -1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => m[3, 0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => m[0, 4]);
        }

        /// <summary>
        /// 测试矩阵乘法维度不匹配时抛出异常
        /// </summary>
        [Fact]
        public void Multiply_DimensionMismatch_ShouldThrow()
        {
            var gf = new GaloisField8bit();
            var m1 = new Matrix8bit(2, 3, gf);
            var m2 = new Matrix8bit(4, 2, gf);

            Assert.Throws<InvalidOperationException>(() => m1.Multiply(m2));
        }

        /// <summary>
        /// 测试不同 GF 的矩阵相乘时抛出异常
        /// </summary>
        [Fact]
        public void Multiply_DifferentGaloisField_ShouldThrow()
        {
            var gf29 = new GaloisField8bit(GaloisField8Poly.P29);
            var gf43 = new GaloisField8bit(GaloisField8Poly.P43);
            var m1 = new Matrix8bit(2, 2, gf29);
            var m2 = new Matrix8bit(2, 2, gf43);

            Assert.Throws<InvalidOperationException>(() => m1.Multiply(m2));
        }

        /// <summary>
        /// 测试裁剪不同位置的子矩阵并求逆
        /// </summary>
        [Fact]
        public void InverseSubMatrix_FromDifferentPosition_ShouldWork()
        {
            var gf = new GaloisField8bit();
            var v = Matrix8bit.Vandermonde(4, 4, gf);

            // 裁剪 2x2 子矩阵从 (1,1)
            var subInv = v.InverseSubMatrix(1, 1, 2);
            Assert.NotNull(subInv);
        }

        /// <summary>
        /// 测试非方阵求逆抛出异常
        /// </summary>
        [Fact]
        public void InverseSubMatrix_NonSquare_ShouldThrow()
        {
            var gf = new GaloisField8bit();
            var m = new Matrix8bit(2, 3, gf);

            Assert.Throws<ArgumentOutOfRangeException>(() => m.InverseSubMatrix(0, 0, 3));
        }

        /// <summary>
        /// 测试按行索引求逆时行索引长度不匹配抛出异常
        /// </summary>
        [Fact]
        public void InverseRows_WrongLength_ShouldThrow()
        {
            var gf = new GaloisField8bit();
            var v = Matrix8bit.Vandermonde(4, 4, gf);

            var rowIndices = new int[] { 0, 1 };
            Assert.Throws<ArgumentException>(() => v.InverseRows(rowIndices, 3));
        }

        /// <summary>
        /// 测试不同多项式的范德蒙德矩阵不同
        /// </summary>
        [Fact]
        public void Vandermonde_DifferentPolynomials_ShouldBeDifferent()
        {
            var gf29 = new GaloisField8bit(GaloisField8Poly.P29);
            var gf43 = new GaloisField8bit(GaloisField8Poly.P43);

            // 检查更高次幂
            for (int i = 2; i <= 5; i++)
            {
                for (int j = 3; j <= 5; j++)
                {
                    _output.WriteLine($"P29 {i}^{j} = {gf29.Power((byte)i, j)}");
                    _output.WriteLine($"P43 {i}^{j} = {gf43.Power((byte)i, j)}");
                    _output.WriteLine("---");
                }
            }

            // 检查更大的矩阵
            var v29 = Matrix8bit.Vandermonde(10, 5, gf29);
            var v43 = Matrix8bit.Vandermonde(10, 5, gf43);

            bool hasDifference = false;
            for (int i = 0; i < 10 && !hasDifference; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (v29[i, j] != v43[i, j])
                    {
                        hasDifference = true;
                        _output.WriteLine($"第一个不同位置: [{i},{j}] P29={v29[i, j]}, P43={v43[i, j]}");
                        break;
                    }
                }
            }

            Assert.True(hasDifference, "不同多项式的范德蒙德矩阵应该不同");
        }
    }
}
