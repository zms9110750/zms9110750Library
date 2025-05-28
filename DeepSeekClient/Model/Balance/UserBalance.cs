using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeepSeekClient.Model.Balance;

/// <summary>
/// 余额详情
/// </summary>
/// <param name="Currency">货币，人民币或美元<br/>有效值只可能为[CNY, USD]</param>
/// <param name="TotalBalance">总的可用余额，包括赠金和充值余额</param>
/// <param name="GrantedBalance">未过期的赠金余额</param>
/// <param name="ToppedUpBalance">充值余额</param>
public record UserBalance(
	[property: JsonPropertyName("currency"), JsonProperty("currency")] string Currency,
	[property: JsonPropertyName("total_balance"), JsonProperty("total_balance")] string TotalBalance,
	[property: JsonPropertyName("granted_balance"), JsonProperty("granted_balance")] string GrantedBalance,
	[property: JsonPropertyName("topped_up_balance"), JsonProperty("topped_up_balance")] string ToppedUpBalance);