namespace zms9110750.DeepSeekClient.Model.Balance;
/// <summary>
/// 用户余额响应
/// </summary>
/// <param name="IsAvailable">当前账户是否有余额可供 API 调用</param>
/// <param name="BalanceInfos">余额详情</param>
public record UserResponse(
	  bool IsAvailable,
	  UserBalance[] BalanceInfos);
