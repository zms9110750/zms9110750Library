namespace zms9110750.DeepSeekClient.Model;
/// <summary>
/// 用户余额响应
/// </summary>
/// <param name="IsAvailable">当前账户是否有余额可供 API 调用</param>
/// <param name="BalanceInfos">余额详情</param>
public record BalanceResponse(
	  bool IsAvailable,
	  Balance[] BalanceInfos);

/// <summary>
/// 余额详情
/// </summary>
/// <param name="Currency">货币，人民币或美元<br/>有效值只可能为[CNY, USD]</param>
/// <param name="TotalBalance">总的可用余额，包括赠金和充值余额</param>
/// <param name="GrantedBalance">未过期的赠金余额</param>
/// <param name="ToppedUpBalance">充值余额</param>
public record Balance(
	 Currency Currency,
	 [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double TotalBalance,
	 [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double GrantedBalance,
	 [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double ToppedUpBalance);

/// <summary>
/// 货币单位
/// </summary>
public enum Currency
{
	/// <summary>
	/// 空占位
	/// </summary>
	Nond,

	/// <summary>
	/// 人民币
	/// </summary>
	CNY,

	/// <summary>
	/// 美元
	/// </summary>
	USD
}