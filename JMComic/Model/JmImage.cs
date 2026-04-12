
namespace zms9110750.JMComic.Model;

/// <summary>
/// 表示一张具体的图片
/// </summary>
/// <param name="AlbumId">所属本子的ID</param>
/// <param name="ScrambleId">图片解密密钥</param>
/// <param name="ImageUrl">图片的完整下载URL</param>
/// <param name="FileName">图片的文件名（不含后缀）</param>
/// <param name="FileSuffix">图片的文件后缀，如".jpg"、".webp"</param>
/// <param name="Index">图片在章节中的序号，从1开始</param>
/// <remarks>
/// 图片（Image）是阅读的最小单位。禁漫天堂的图片通常需要解密才能正常显示。
/// 解密算法需要使用ScrambleId作为密钥。
/// 图片信息通常由章节详情派生而来，包含具体的下载URL和解密所需信息。
/// </remarks>
public record JmImage(
    string AlbumId,
    string ScrambleId,
    string ImageUrl,
    string FileName,
    string FileSuffix,
    int Index
);