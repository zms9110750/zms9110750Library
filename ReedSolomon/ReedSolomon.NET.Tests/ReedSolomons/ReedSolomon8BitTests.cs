
using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.ReedSolomons;

namespace ReedSolomon.NET.Tests.ReedSolomons
{
    /// <summary>
    /// 测试 ReedSolomon8Bit 类自己的特性
    /// </summary>
    public class ReedSolomon8BitTests
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
        /// 测试构造器参数校验
        /// </summary>
        [Fact]
        public void Constructor_InvalidParameters_ShouldThrow()
        {
            var gf = new GaloisField8bit();

            Assert.Throws<ArgumentException>(() => new ReedSolomon8Bit(0, 4, gf));
            Assert.Throws<ArgumentException>(() => new ReedSolomon8Bit(16, 0, gf));
            Assert.Throws<ArgumentException>(() => new ReedSolomon8Bit(-1, 4, gf));
            Assert.Throws<ArgumentException>(() => new ReedSolomon8Bit(200, 100, gf));
        }

        /// <summary>
        /// 测试不同多项式构造的实例，PrimitivePolynomial 不同
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void Constructor_DifferentPolynomials_ShouldHaveDifferentPrimitivePolynomial(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var rs = new ReedSolomon8Bit(16, 4, gf);

            Assert.Equal((byte)poly, rs.PrimitivePolynomial);
        }

        /// <summary>
        /// 测试同一多项式多次构造，属性相同
        /// </summary>
        [Fact]
        public void Constructor_SamePolynomial_ShouldHaveSameProperties()
        {
            var gf = new GaloisField8bit(GaloisField8Poly.P29);
            var rs1 = new ReedSolomon8Bit(16, 4, gf);
            var rs2 = new ReedSolomon8Bit(16, 4, gf);

            Assert.Equal(rs1.DataShardCount, rs2.DataShardCount);
            Assert.Equal(rs1.ParityShardCount, rs2.ParityShardCount);
            Assert.Equal(rs1.TotalShardCount, rs2.TotalShardCount);
            Assert.Equal(rs1.PrimitivePolynomial, rs2.PrimitivePolynomial);
        }

        /// <summary>
        /// 测试默认构造器使用 P29
        /// </summary>
        [Fact]
        public void Constructor_Default_ShouldUseP29()
        {
            var rs = new ReedSolomon8Bit(16, 4);
            Assert.Equal(29, rs.PrimitivePolynomial);
        }

        /// <summary>
        /// 测试不同多项式编码结果不同
        /// </summary>
        [Fact]
        public void DifferentPolynomials_ShouldProduceDifferentParity()
        {
            var gf29 = new GaloisField8bit(GaloisField8Poly.P29);
            var gf43 = new GaloisField8bit(GaloisField8Poly.P43);
            var rs29 = new ReedSolomon8Bit(3, 2, gf29);
            var rs43 = new ReedSolomon8Bit(3, 2, gf43);

            // 使用能产生差异的数据
            // 根据之前的发现，4^4 在不同多项式下不同
            // 构造数据使编码结果产生差异
            var dataShards = new List<byte[]>
    {
        new byte[] { 4, 0, 0 },
        new byte[] { 4, 0, 0 },
        new byte[] { 4, 0, 0 }
    };

            var parity29 = new List<byte[]> { new byte[3], new byte[3] };
            var parity43 = new List<byte[]> { new byte[3], new byte[3] };

            rs29.EncodeParity(dataShards, parity29);
            rs43.EncodeParity(dataShards, parity43);

            bool hasDifference = false;
            for (int i = 0; i < 2 && !hasDifference; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (parity29[i][j] != parity43[i][j])
                    {
                        hasDifference = true;
                        break;
                    }
                }
            }

            // 如果还相同，用随机数据多次尝试
            if (!hasDifference)
            {
                var random = new Random(42);
                for (int attempt = 0; attempt < 100 && !hasDifference; attempt++)
                {
                    var testData = new List<byte[]>
            {
                new byte[] { (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256) },
                new byte[] { (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256) },
                new byte[] { (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256) }
            };

                    var testParity29 = new List<byte[]> { new byte[3], new byte[3] };
                    var testParity43 = new List<byte[]> { new byte[3], new byte[3] };

                    rs29.EncodeParity(testData, testParity29);
                    rs43.EncodeParity(testData, testParity43);

                    for (int i = 0; i < 2 && !hasDifference; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (testParity29[i][j] != testParity43[i][j])
                            {
                                hasDifference = true;
                                break;
                            }
                        }
                    }
                }
            }

            Assert.True(hasDifference, "不同多项式的编码结果应该不同");
        }
    }
}
