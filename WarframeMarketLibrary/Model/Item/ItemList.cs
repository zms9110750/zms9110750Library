 
namespace WarframeMarketLibrary.Model.Item;
/// <summary>
/// 从服务器得到的物品简略信息。用于缓存
/// </summary>
/// <param name="ApiVersion">版本信息</param>
/// <param name="Data">数据</param>
/// <param name="Error">错误</param>
public record ItemList (string ApiVersion, ItemShort[] Data, string? Error): Response<ItemShort[]>(ApiVersion, Data, Error);
