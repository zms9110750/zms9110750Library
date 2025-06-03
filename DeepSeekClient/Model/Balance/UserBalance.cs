using System.Text.Json.Serialization;

namespace zms9110750.DeepSeekClient.Model.Balance;

/// <summary>
/// 余额详情
/// </summary>
/// <param name="Currency">货币，人民币或美元<br/>有效值只可能为[CNY, USD]</param>
/// <param name="TotalBalance">总的可用余额，包括赠金和充值余额</param>
/// <param name="GrantedBalance">未过期的赠金余额</param>
/// <param name="ToppedUpBalance">充值余额</param>
public record UserBalance(
	 Currency Currency,
	 [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double TotalBalance,
	 [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double GrantedBalance,
	 [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double ToppedUpBalance);
