using Xunit;
using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.Matrixs;

namespace ReedSolomon.NET.Tests.Matrixs
{
    /// <summary>
    /// 测试 IMatrix&lt;byte&gt; 接口的行为
    /// </summary>
    public class IMatrixTests
    {
        public static TheoryData<GaloisField8Poly> AllPolynomials()
        {
            var data = new TheoryData<GaloisField8Poly>();
            foreach (GaloisField8Poly poly in Enum.GetValues(typeof(GaloisField8Poly)))
            {
                data.Add(poly);
            }
            return data;
        }

        /// <summary>
        /// 测试所有多项式下，创建零矩阵
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void Create_ZeroMatrix_ShouldBeZero(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IMatrix<byte> m = new Matrix8bit(3, 4, gf);

            Assert.Equal(3, m.Rows);
            Assert.Equal(4, m.Columns);
            Assert.Equal(gf, m.GaloisField);

            for (int i = 0; i < m.Rows; i++)
            {
                for (int j = 0; j < m.Columns; j++)
                {
                    Assert.Equal(0, m[i, j]);
                }
            }
        }

        /// <summary>
        /// 测试所有多项式下，范德蒙德矩阵正确
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void Vandermonde_ShouldBeCorrect(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IMatrix<byte> v = Matrix8bit.Vandermonde(4, 3, gf);

            Assert.Equal(4, v.Rows);
            Assert.Equal(3, v.Columns);
            Assert.Equal(gf, v.GaloisField);

            // 第0行: [1, 0, 0] (因为 0^0=1, 0^1=0, 0^2=0)
            Assert.Equal(1, v[0, 0]);
            Assert.Equal(0, v[0, 1]);
            Assert.Equal(0, v[0, 2]);

            // 第1行: [1, 1, 1] (1^0=1, 1^1=1, 1^2=1)
            Assert.Equal(1, v[1, 0]);
            Assert.Equal(1, v[1, 1]);
            Assert.Equal(1, v[1, 2]);

            // 第2行: [1, 2, 2^2]
            Assert.Equal(1, v[2, 0]);
            Assert.Equal(2, v[2, 1]);
            Assert.Equal(gf.Power(2, 2), v[2, 2]);

            // 第3行: [1, 3, 3^2]
            Assert.Equal(1, v[3, 0]);
            Assert.Equal(3, v[3, 1]);
            Assert.Equal(gf.Power(3, 2), v[3, 2]);
        }

        /// <summary>
        /// 测试所有多项式下，矩阵乘法正确性
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void Multiply_ShouldBeCorrect(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IMatrix<byte> v = Matrix8bit.Vandermonde(2, 2, gf);
            // v = [1, 0; 1, 1]

            IMatrix<byte> result = v.Multiply(v);

            // [1,0; 1,1] * [1,0; 1,1] = [1,0; 1+1=0, 1]
            Assert.Equal(1, result[0, 0]);
            Assert.Equal(0, result[0, 1]);
            Assert.Equal(gf.Add(1, 1), result[1, 0]);
            Assert.Equal(1, result[1, 1]);
        }

        /// <summary>
        /// 测试所有多项式下，求逆正确性
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void InverseSubMatrix_ShouldBeCorrect(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IMatrix<byte> v = Matrix8bit.Vandermonde(2, 2, gf);
            // v = [1, 0; 1, 1]

            IMatrix<byte> inv = v.InverseSubMatrix(0, 0, 2);
            IMatrix<byte> product = v.Multiply(inv);

            Assert.Equal(1, product[0, 0]);
            Assert.Equal(0, product[0, 1]);
            Assert.Equal(0, product[1, 0]);
            Assert.Equal(1, product[1, 1]);
        }

        /// <summary>
        /// 测试所有多项式下，按行索引求逆
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void InverseRows_ShouldBeCorrect(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IMatrix<byte> v = Matrix8bit.Vandermonde(4, 4, gf);

            var rowIndices = new int[] { 0, 1, 2, 3 };
            IMatrix<byte> inv = v.InverseRows(rowIndices, 4);
            IMatrix<byte> product = v.Multiply(inv);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == j)
                    {
                        Assert.Equal(1, product[i, j]);
                    }
                    else
                    {
                        Assert.Equal(0, product[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// 测试所有多项式下，获取行
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void GetRow_ShouldReturnCorrectSpan(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IMatrix<byte> v = Matrix8bit.Vandermonde(4, 3, gf);

            var row0 = v.GetRow(0);
            Assert.Equal(3, row0.Length);
            Assert.Equal(1, row0[0]);
            Assert.Equal(0, row0[1]);
            Assert.Equal(0, row0[2]);

            var row1 = v.GetRow(1);
            Assert.Equal(1, row1[0]);
            Assert.Equal(1, row1[1]);
            Assert.Equal(1, row1[2]);

            Assert.Throws<ArgumentOutOfRangeException>(() => v.GetRow(4));
        }
    }
} 
namespace ReedSolomon.NET.Tests.Matrixs
{
}
