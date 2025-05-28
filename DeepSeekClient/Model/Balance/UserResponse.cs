using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Balance;
/// <summary>
/// 用户余额详情
/// </summary>
/// <param name="IsAvailable">当前账户是否有余额可供 API 调用</param>
/// <param name="BalanceInfos">余额详情</param>
public record UserResponse(
	[property: JsonPropertyName("is_available"), JsonProperty("is_available")] bool IsAvailable,
	[property: JsonPropertyName("balance_infos"), JsonProperty("balance_infos")] UserBalance[] BalanceInfos);
