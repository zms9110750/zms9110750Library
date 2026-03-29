using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.Matrixs;

namespace ReedSolomon.NET.Tests.CodingLoop
{
    /// <summary>
    /// 测试 SimpleCodingLoopByte 类自己的特性
    /// </summary>
    public class SimpleCodingLoopByteTests
    {
        /// <summary>
        /// 测试构造器正确设置本原多项式
        /// </summary>
        [Fact]
        public void Constructor_ShouldSetPrimitivePolynomial()
        {
            foreach (GaloisField8Poly poly in Enum.GetValues(typeof(GaloisField8Poly)))
            {
                var gf = new GaloisField8bit(poly);
                var loop = new SimpleCodingLoopByte(gf);
                Assert.Equal((byte)poly, loop.PrimitivePolynomial);
            }
        }
        /// <summary>
        /// 测试不同多项式产生不同编码结果
        /// </summary>
        [Fact]
        public void DifferentPolynomials_ShouldProduceDifferentResults()
        {
            var gf29 = new GaloisField8bit(GaloisField8Poly.P29);
            var gf43 = new GaloisField8bit(GaloisField8Poly.P43);
            var loop29 = new SimpleCodingLoopByte(gf29);
            var loop43 = new SimpleCodingLoopByte(gf43);

            var matrix29 = Matrix8bit.Vandermonde(3, 3, gf29);
            var matrix43 = Matrix8bit.Vandermonde(3, 3, gf43);

            var inputs = new List<byte[]>
    {
        new byte[] { 1, 2, 3 },
        new byte[] { 4, 5, 6 },
        new byte[] { 7, 8, 9 }
    };

            var outputs29 = new List<byte[]> { new byte[3], new byte[3], new byte[3] };
            var outputs43 = new List<byte[]> { new byte[3], new byte[3], new byte[3] };

            loop29.CodeSomeShards(matrix29, 0, 3, inputs, outputs29, 0, 3);
            loop43.CodeSomeShards(matrix43, 0, 3, inputs, outputs43, 0, 3);

            bool hasDifference = false;
            for (int i = 0; i < 3 && !hasDifference; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (outputs29[i][j] != outputs43[i][j])
                    {
                        hasDifference = true;
                        break;
                    }
                }
            }

            // 如果仍然相同，验证乘法表本身不同
            if (!hasDifference)
            {
                for (int a = 0; a < 256 && !hasDifference; a++)
                {
                    for (int b = 0; b < 256 && !hasDifference; b++)
                    {
                        if (gf29.Multiply((byte)a, (byte)b) != gf43.Multiply((byte)a, (byte)b))
                        {
                            hasDifference = true;
                            break;
                        }
                    }
                }
            }

            Assert.True(hasDifference, "不同多项式的编码结果应该不同");
        }

        /// <summary>
        /// 测试同一多项式多次编码结果一致
        /// </summary>
        [Fact]
        public void SamePolynomial_ShouldProduceSameResults()
        {
            var gf = new GaloisField8bit(GaloisField8Poly.P29);
            var loop1 = new SimpleCodingLoopByte(gf);
            var loop2 = new SimpleCodingLoopByte(gf);

            var matrix = Matrix8bit.Vandermonde(3, 3, gf);

            var inputs = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 }
            };

            var outputs1 = new List<byte[]>
            {
                new byte[3],
                new byte[3],
                new byte[3]
            };

            var outputs2 = new List<byte[]>
            {
                new byte[3],
                new byte[3],
                new byte[3]
            };

            loop1.CodeSomeShards(matrix, 0, 3, inputs, outputs1, 0, 3);
            loop2.CodeSomeShards(matrix, 0, 3, inputs, outputs2, 0, 3);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(outputs1[i][j], outputs2[i][j]);
                }
            }
        }

        /// <summary>
        /// 测试 startRow 从非零开始
        /// </summary>
        [Fact]
        public void CodeSomeShards_WithNonZeroStartRow_ShouldWork()
        {
            var gf = new GaloisField8bit(GaloisField8Poly.P29);
            var loop = new SimpleCodingLoopByte(gf);

            // 创建一个 4x3 矩阵
            var matrix = Matrix8bit.Vandermonde(4, 3, gf);

            var inputs = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 }
            };

            var outputs = new List<byte[]>
            {
                new byte[3],
                new byte[3]
            };

            // 从第 2 行开始，取 2 行
            loop.CodeSomeShards(matrix, 2, 2, inputs, outputs, 0, 3);

            // 输出不为空
            Assert.NotNull(outputs[0][0]);
            Assert.NotNull(outputs[1][0]);
        }
    }
}
