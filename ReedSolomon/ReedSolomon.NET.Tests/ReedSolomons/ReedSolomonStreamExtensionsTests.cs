
using Xunit.Abstractions;
using zms9110750.ReedSolomon.Galos;
using zms9110750.ReedSolomon.ReedSolomons;

namespace ReedSolomon.NET.Tests.ReedSolomons
{
    /// <summary>
    /// 测试 ReedSolomon 扩展方法
    /// </summary>
    public class ReedSolomonStreamExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public ReedSolomonStreamExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static TheoryData<GaloisField8Poly> AllPolynomials()
        {
            var data = new TheoryData<GaloisField8Poly>();
            foreach (GaloisField8Poly poly in Enum.GetValues(typeof(GaloisField8Poly)))
            {
                data.Add(poly);
            }
            return data;
        }

        private static byte[] CreateRandomData(int size, int seed)
        {
            var data = new byte[size];
            var random = new Random(seed);
            random.NextBytes(data);
            return data;
        }

        /// <summary>
        /// 测试编码解码流，所有分片存在
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public async Task EncodeAndDecodeStream_AllShardsPresent_ShouldMatch(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var rs = new ReedSolomon8Bit(3, 2, gf);

            var originalData = CreateRandomData(1024, 42);
            var outputStreams = new List<MemoryStream>();

            for (int i = 0; i < rs.TotalShardCount; i++)
            {
                outputStreams.Add(new MemoryStream());
            }

            // 编码
            using (var inputStream = new MemoryStream(originalData))
            {
                await rs.EncodeParityAsync(inputStream, originalData.Length, outputStreams);
            }
             

            // 解码
            var inputStreams = outputStreams.Select(s => new MemoryStream(s.ToArray())).Cast<Stream>().ToList();

            using (var outputStream = new MemoryStream())
            {
                await rs.DecodeMissingAsync(inputStreams, outputStream, originalData.Length);

                var recoveredData = outputStream.ToArray();
                Assert.Equal(originalData.Length, recoveredData.Length);
                Assert.Equal(originalData, recoveredData);
            }
        }

        /// <summary>
        /// 测试编码解码流，缺失部分分片
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public async Task EncodeAndDecodeStream_WithMissingShards_ShouldRecover(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var rs = new ReedSolomon8Bit(3, 2, gf);

            var originalData = CreateRandomData(1024, 42);
            var outputStreams = new List<MemoryStream>();

            for (int i = 0; i < rs.TotalShardCount; i++)
            {
                outputStreams.Add(new MemoryStream());
            }

            // 编码
            using (var inputStream = new MemoryStream(originalData))
            {
                await rs.EncodeParityAsync(inputStream, originalData.Length, outputStreams);
            }

            // 模拟丢失分片 0 和 3
            var shardPresent = Enumerable.Repeat(true, rs.TotalShardCount).ToArray();
            shardPresent[0] = false;
            shardPresent[3] = false;

            // 解码
            var inputStreams = new List<Stream?>();
            for (int i = 0; i < rs.TotalShardCount; i++)
            {
                if (shardPresent[i])
                {
                    inputStreams.Add(new MemoryStream(outputStreams[i].ToArray()));
                }
                else
                {
                    inputStreams.Add(null);
                }
            }

            using (var outputStream = new MemoryStream())
            {
                await rs.DecodeMissingAsync(inputStreams, outputStream, originalData.Length);

                var recoveredData = outputStream.ToArray();
                Assert.Equal(originalData.Length, recoveredData.Length);
                Assert.Equal(originalData, recoveredData);
            }
        }


        /// <summary>
        /// 测试大文件分块处理
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public async Task EncodeAndDecodeStream_LargeFile_ShouldWork(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var rs = new ReedSolomon8Bit(4, 2, gf);

            // 5MB 数据，大于 blockSize (1MB)
            int fileSize = 5 * 1024 * 1024;
            var originalData = CreateRandomData(fileSize, 42);
            var outputStreams = new List<MemoryStream>();

            for (int i = 0; i < rs.TotalShardCount; i++)
            {
                outputStreams.Add(new MemoryStream());
            }

            // 编码
            using (var inputStream = new MemoryStream(originalData))
            {
                await rs.EncodeParityAsync(inputStream, originalData.Length, outputStreams);
            }

            // 解码
            var inputStreams = outputStreams.Select(s => new MemoryStream(s.ToArray())).Cast<Stream>().ToList();

            using (var outputStream = new MemoryStream())
            {
                await rs.DecodeMissingAsync(inputStreams, outputStream, originalData.Length);

                var recoveredData = outputStream.ToArray();
                Assert.Equal(originalData.Length, recoveredData.Length);
                Assert.Equal(originalData, recoveredData);
            }
        }

        /// <summary>
        /// 测试空文件
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPolynomials))]
        public async Task EncodeAndDecodeStream_EmptyFile_ShouldWork(GaloisField8Poly poly)
        {
            var gf = new GaloisField8bit(poly);
            var rs = new ReedSolomon8Bit(3, 2, gf);

            var originalData = Array.Empty<byte>();
            var outputStreams = new List<MemoryStream>();

            for (int i = 0; i < rs.TotalShardCount; i++)
            {
                outputStreams.Add(new MemoryStream());
            }

            // 编码
            using (var inputStream = new MemoryStream(originalData))
            {
                await rs.EncodeParityAsync(inputStream, originalData.Length, outputStreams);
            }

            var inputStreams = outputStreams.Select(s => new MemoryStream(s.ToArray())).Cast<Stream>().ToList();

            using (var outputStream = new MemoryStream())
            {
                await rs.DecodeMissingAsync(inputStreams, outputStream, originalData.Length);

                var recoveredData = outputStream.ToArray();
                Assert.Equal(originalData.Length, recoveredData.Length);
            }
        }

        /// <summary>
        /// 测试不同多项式都能正常工作
        /// </summary>
        [Fact]
        public async Task AllPolynomials_EncodeAndDecode_ShouldWork()
        {
            foreach (GaloisField8Poly poly in Enum.GetValues(typeof(GaloisField8Poly)))
            {
                var gf = new GaloisField8bit(poly);
                var rs = new ReedSolomon8Bit(3, 2, gf);

                var originalData = CreateRandomData(1024, 42);
                var outputStreams = new List<MemoryStream>();

                for (int i = 0; i < rs.TotalShardCount; i++)
                {
                    outputStreams.Add(new MemoryStream());
                }

                using (var inputStream = new MemoryStream(originalData))
                {
                    await rs.EncodeParityAsync(inputStream, originalData.Length, outputStreams);
                }

                var inputStreams = outputStreams.Select(s => new MemoryStream(s.ToArray())).Cast<Stream>().ToList();

                using (var outputStream = new MemoryStream())
                {
                    await rs.DecodeMissingAsync(inputStreams, outputStream, originalData.Length);

                    var recoveredData = outputStream.ToArray();
                    Assert.Equal(originalData.Length, recoveredData.Length);
                    Assert.Equal(originalData, recoveredData);
                }
            }
        }
    }
}
