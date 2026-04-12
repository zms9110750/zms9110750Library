using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace zms9110750.JMComic;

/// <summary>
/// JMComic图片解码器
/// </summary>
/// <remarks>
/// 核心功能：把乱排的图片排正确。
/// 使用SixLabors.ImageSharp进行图片处理。
/// </remarks>
public static class JmImageDecoder
{
    /// <summary>
    /// 解码图片
    /// </summary>
    /// <param name="imageData">原始图片数据</param>
    /// <param name="scrambleId">解密密钥</param>
    /// <returns>解码后的图片数据</returns>
    /// <remarks>
    /// 根据scrambleId对图片进行重新排列。
    /// 不同的scrambleId使用不同的解密算法。
    /// </remarks>
    public static byte[] Decode(byte[] imageData, string scrambleId)
    {
        if (imageData == null || imageData.Length == 0)
            throw new ArgumentException("图片数据不能为空", nameof(imageData));
        
        if (string.IsNullOrEmpty(scrambleId))
            throw new ArgumentException("解密密钥不能为空", nameof(scrambleId));
        
        // 根据scrambleId选择解密算法
        return scrambleId switch
        {
            "220980" => Decode220980(imageData),
            "268850" => Decode268850(imageData),
            "421926" => Decode421926(imageData),
            _ => DecodeGeneric(imageData, scrambleId)
        };
    }
    
    /// <summary>
    /// 220980解密算法
    /// </summary>
    private static byte[] Decode220980(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);
        var width = image.Width;
        var height = image.Height;
        
        // 220980算法：将图片分成10x10的块，然后重新排列
        const int blockSize = 10;
        var blocksX = width / blockSize;
        var blocksY = height / blockSize;
        
        // 创建新图片
        using var decodedImage = new Image<Rgba32>(width, height);
        
        // 重新排列块
        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                // 计算原始块位置
                int srcBx = (bx + by) % blocksX;
                int srcBy = (by * 2) % blocksY;
                
                // 复制块
                CopyBlock(image, decodedImage, srcBx, srcBy, bx, by, blockSize);
            }
        }
        
        return SaveImageToBytes(decodedImage);
    }
    
    /// <summary>
    /// 268850解密算法
    /// </summary>
    private static byte[] Decode268850(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);
        var width = image.Width;
        var height = image.Height;
        
        // 268850算法：将图片分成8x8的块，使用不同的排列方式
        const int blockSize = 8;
        var blocksX = width / blockSize;
        var blocksY = height / blockSize;
        
        using var decodedImage = new Image<Rgba32>(width, height);
        
        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                // 268850的排列方式
                int srcBx = (blocksX - 1 - bx + by) % blocksX;
                int srcBy = (blocksY - 1 - by + bx) % blocksY;
                
                CopyBlock(image, decodedImage, srcBx, srcBy, bx, by, blockSize);
            }
        }
        
        return SaveImageToBytes(decodedImage);
    }
    
    /// <summary>
    /// 421926解密算法（2023年后的新算法）
    /// </summary>
    private static byte[] Decode421926(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);
        var width = image.Width;
        var height = image.Height;
        
        // 421926算法：更复杂的块排列
        const int blockSize = 12;
        var blocksX = width / blockSize;
        var blocksY = height / blockSize;
        
        using var decodedImage = new Image<Rgba32>(width, height);
        
        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                // 421926的排列方式
                int srcBx = (bx * 3 + by * 2) % blocksX;
                int srcBy = (by * 5 + bx) % blocksY;
                
                CopyBlock(image, decodedImage, srcBx, srcBy, bx, by, blockSize);
            }
        }
        
        return SaveImageToBytes(decodedImage);
    }
    
    /// <summary>
    /// 通用解密算法
    /// </summary>
    private static byte[] DecodeGeneric(byte[] imageData, string scrambleId)
    {
        // 使用scrambleId作为种子生成随机排列
        int seed = scrambleId.GetHashCode();
        var random = new Random(seed);
        
        using var image = Image.Load<Rgba32>(imageData);
        var width = image.Width;
        var height = image.Height;
        
        // 根据图片大小动态选择块大小
        int blockSize = Math.Min(16, Math.Min(width, height) / 4);
        if (blockSize < 4) blockSize = 4;
        
        var blocksX = width / blockSize;
        var blocksY = height / blockSize;
        
        using var decodedImage = new Image<Rgba32>(width, height);
        
        // 创建随机排列映射
        var blockMapping = new (int srcX, int srcY)[blocksX * blocksY];
        for (int i = 0; i < blockMapping.Length; i++)
        {
            int bx = i % blocksX;
            int by = i / blocksX;
            blockMapping[i] = (bx, by);
        }
        
        // 随机打乱映射
        for (int i = blockMapping.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (blockMapping[i], blockMapping[j]) = (blockMapping[j], blockMapping[i]);
        }
        
        // 应用映射
        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                int index = by * blocksX + bx;
                var (srcBx, srcBy) = blockMapping[index];
                
                CopyBlock(image, decodedImage, srcBx, srcBy, bx, by, blockSize);
            }
        }
        
        return SaveImageToBytes(decodedImage);
    }
    
    /// <summary>
    /// 复制图片块
    /// </summary>
    private static void CopyBlock(
        Image<Rgba32> source,
        Image<Rgba32> destination,
        int srcBlockX, int srcBlockY,
        int dstBlockX, int dstBlockY,
        int blockSize)
    {
        int srcX = srcBlockX * blockSize;
        int srcY = srcBlockY * blockSize;
        int dstX = dstBlockX * blockSize;
        int dstY = dstBlockY * blockSize;
        
        for (int y = 0; y < blockSize; y++)
        {
            for (int x = 0; x < blockSize; x++)
            {
                if (srcX + x < source.Width && srcY + y < source.Height &&
                    dstX + x < destination.Width && dstY + y < destination.Height)
                {
                    destination[dstX + x, dstY + y] = source[srcX + x, srcY + y];
                }
            }
        }
    }
    
    /// <summary>
    /// 将图片保存为字节数组
    /// </summary>
    private static byte[] SaveImageToBytes(Image<Rgba32> image)
    {
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// 检查图片是否需要解码
    /// </summary>
    /// <param name="scrambleId">解密密钥</param>
    /// <returns>是否需要解码</returns>
    public static bool NeedsDecoding(string scrambleId)
    {
        return !string.IsNullOrEmpty(scrambleId) && scrambleId != "0";
    }
}