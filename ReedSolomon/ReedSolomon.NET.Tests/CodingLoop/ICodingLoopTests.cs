using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.Matrixs;

namespace ReedSolomon.NET.Tests.CodingLoop
{
    /// <summary>
    /// 测试 ICodingLoop&lt;byte&gt; 接口的行为
    /// </summary>
    public class ICodingLoopTests
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

        private static IMatrix<byte> CreateIdentity(IGaloisField<byte> gf, int size)
        {
            var v = Matrix8bit.Vandermonde(size, size, gf);
            var inv = v.InverseSubMatrix(0, 0, size);
            return v.Multiply(inv);
        }

        /// <summary>
        /// 测试单位矩阵编码：输出应该等于输入
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void CodeSomeShards_WithIdentityMatrix_ShouldOutputEqualsInput(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var loop = new SimpleCodingLoopByte(gf);

            int size = 3;
            var identity = CreateIdentity(gf, size);

            var inputs = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 }
            };

            var outputs = new List<byte[]>
            {
                new byte[3],
                new byte[3],
                new byte[3]
            };

            loop.CodeSomeShards(
                identity, 0, size,
                inputs, outputs,
                0, 3);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(inputs[i][j], outputs[i][j]);
                }
            }
        }

        /// <summary>
        /// 测试范德蒙德矩阵编码：输出不为空
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void CodeSomeShards_WithVandermondeMatrix_ShouldProduceNonZeroOutput(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var loop = new SimpleCodingLoopByte(gf);

            var matrix = Matrix8bit.Vandermonde(3, 3, gf);

            var inputs = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 }
            };

            var outputs = new List<byte[]>
            {
                new byte[3],
                new byte[3],
                new byte[3]
            };

            loop.CodeSomeShards(
                matrix, 0, 3,
                inputs, outputs,
                0, 3);

            bool hasNonZero = false;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (outputs[i][j] != 0)
                    {
                        hasNonZero = true;
                        break;
                    }
                }
            }
            Assert.True(hasNonZero);
        }

        /// <summary>
        /// 测试偏移量：只处理指定范围的字节
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void CodeSomeShards_WithOffset_ShouldOnlyProcessRange(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var loop = new SimpleCodingLoopByte(gf);

            int size = 3;
            var identity = CreateIdentity(gf, size);

            var inputs = new List<byte[]>
            {
                new byte[] { 1, 2, 3, 4, 5, 6 },
                new byte[] { 7, 8, 9, 10, 11, 12 },
                new byte[] { 13, 14, 15, 16, 17, 18 }
            };

            var outputs = new List<byte[]>
            {
                new byte[6],
                new byte[6],
                new byte[6]
            };

            loop.CodeSomeShards(
                identity, 0, size,
                inputs, outputs,
                3, 3);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(0, outputs[i][j]);
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 3; j < 6; j++)
                {
                    Assert.Equal(inputs[i][j], outputs[i][j]);
                }
            }
        }

        /// <summary>
        /// 测试 byteCount=0：不处理任何字节
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void CodeSomeShards_WithZeroByteCount_ShouldNotModifyOutputs(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var loop = new SimpleCodingLoopByte(gf);

            int size = 3;
            var identity = CreateIdentity(gf, size);

            var inputs = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 }
            };

            var outputs = new List<byte[]>
            {
                new byte[3],
                new byte[3],
                new byte[3]
            };

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    outputs[i][j] = 0xFF;
                }
            }

            loop.CodeSomeShards(
                identity, 0, size,
                inputs, outputs,
                0, 0);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(0xFF, outputs[i][j]);
                }
            }
        }
    }
}
namespace ReedSolomon.NET.Tests.CodingLoop
{
}
