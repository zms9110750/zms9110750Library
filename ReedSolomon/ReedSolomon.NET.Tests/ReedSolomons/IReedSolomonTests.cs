using Xunit;
using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.ReedSolomons;

namespace ReedSolomon.NET.Tests.ReedSolomons
{
    /// <summary>
    /// 测试 IReedSolomon&lt;byte&gt; 接口的功能正确性
    /// </summary>
    public class IReedSolomonTests
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

        private static List<byte[]> CreateTestData(int k, int shardSize, int seed)
        {
            var data = new List<byte[]>();
            var random = new Random(seed);
            for (int i = 0; i < k; i++)
            {
                var shard = new byte[shardSize];
                random.NextBytes(shard);
                data.Add(shard);
            }
            return data;
        }

        /// <summary>
        /// 测试编码后冗余分片不为全零
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void EncodeParity_ShouldProduceNonZeroParity(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 }
            };

            var parityShards = new List<byte[]> { new byte[3], new byte[3] };

            rs.EncodeParity(dataShards, parityShards);

            bool hasNonZero = false;
            for (int i = 0; i < parityShards.Count; i++)
            {
                for (int j = 0; j < parityShards[i].Length; j++)
                {
                    if (parityShards[i][j] != 0)
                    {
                        hasNonZero = true;
                        break;
                    }
                }
            }
            Assert.True(hasNonZero);
        }

        /// <summary>
        /// 测试无缺失时解码不修改数据
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_AllShardsPresent_ShouldNotModify(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            var backup = allShards.Select(s => s.ToArray()).ToList();
            var present = Enumerable.Repeat(true, 5).ToArray();

            rs.DecodeMissing(allShards, present);

            for (int i = 0; i < allShards.Count; i++)
            {
                for (int j = 0; j < allShards[i].Length; j++)
                {
                    Assert.Equal(backup[i][j], allShards[i][j]);
                }
            }
        }

        /// <summary>
        /// 测试缺失一个数据分片
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_OneMissingDataShard_ShouldRecover(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            var backup0 = allShards[0].ToArray();
            allShards[0] = new byte[10];

            var present = Enumerable.Repeat(true, 5).ToArray();
            present[0] = false;

            rs.DecodeMissing(allShards, present);

            Assert.Equal(backup0, allShards[0]);
        }

        /// <summary>
        /// 测试缺失一个冗余分片
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_OneMissingParityShard_ShouldRecover(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var backupParity0 = parityShards[0].ToArray();
            parityShards[0] = new byte[10];

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            var present = Enumerable.Repeat(true, 5).ToArray();
            present[3] = false;

            rs.DecodeMissing(allShards, present);

            Assert.Equal(backupParity0, allShards[3]);
        }

        /// <summary>
        /// 测试缺失两个数据分片
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_TwoMissingDataShards_ShouldRecover(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(4, 2, gf);

            var dataShards = CreateTestData(4, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            var backup0 = allShards[0].ToArray();
            var backup1 = allShards[1].ToArray();
            allShards[0] = new byte[10];
            allShards[1] = new byte[10];

            var present = Enumerable.Repeat(true, 6).ToArray();
            present[0] = false;
            present[1] = false;

            rs.DecodeMissing(allShards, present);

            Assert.Equal(backup0, allShards[0]);
            Assert.Equal(backup1, allShards[1]);
        }

        /// <summary>
        /// 测试缺失混合分片（数据+冗余）
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_MixedMissingShards_ShouldRecover(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(4, 2, gf);

            var dataShards = CreateTestData(4, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            var backup0 = allShards[0].ToArray();
            var backupParity0 = allShards[4].ToArray();
            allShards[0] = new byte[10];
            allShards[4] = new byte[10];

            var present = Enumerable.Repeat(true, 6).ToArray();
            present[0] = false;
            present[4] = false;

            rs.DecodeMissing(allShards, present);

            Assert.Equal(backup0, allShards[0]);
            Assert.Equal(backupParity0, allShards[4]);
        }

        /// <summary>
        /// 测试可用分片刚好等于 k
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_ExactlyKShardsAvailable_ShouldRecover(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            var backup = allShards.Select(s => s.ToArray()).ToList();

            // 只保留前 3 个分片（刚好 k 个）
            var present = new bool[5];
            present[0] = true;
            present[1] = true;
            present[2] = true;
            present[3] = false;
            present[4] = false;

            rs.DecodeMissing(allShards, present);

            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(backup[i], allShards[i]);
            }
        }

        /// <summary>
        /// 测试可用分片不足 k 时抛出异常
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void DecodeMissing_NotEnoughShards_ShouldThrow(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            var allShards = new List<byte[]>();
            allShards.AddRange(dataShards);
            allShards.AddRange(parityShards);

            // 只有 2 个可用分片，需要 3 个
            var present = new bool[5];
            present[0] = true;
            present[1] = true;
            present[2] = false;
            present[3] = false;
            present[4] = false;

            Assert.Throws<InvalidOperationException>(() => rs.DecodeMissing(allShards, present));
        }

        /// <summary>
        /// 测试校验正确性
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void IsParityCorrect_WithCorrectParity_ShouldReturnTrue(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var parityShards = new List<byte[]> { new byte[10], new byte[10] };

            rs.EncodeParity(dataShards, parityShards);

            bool result = rs.IsParityCorrect(dataShards, parityShards);

            Assert.True(result);
        }

        /// <summary>
        /// 测试校验错误冗余分片返回 false
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public void IsParityCorrect_WithWrongParity_ShouldReturnFalse(GaloisField8Poly poly)
        {
            IGaloisField<byte> gf = new GaloisField8bit(poly);
            IReedSolomon<byte> rs = new ReedSolomon8Bit(3, 2, gf);

            var dataShards = CreateTestData(3, 10, 42);
            var wrongParity = new List<byte[]> { new byte[10], new byte[10] };

            bool result = rs.IsParityCorrect(dataShards, wrongParity);

            Assert.False(result);
        }
    }
}
