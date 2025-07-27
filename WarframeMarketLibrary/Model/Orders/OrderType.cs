using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketLibrary.Model.Orders;
/// <summary>
/// 订单类型
/// </summary>
public enum OrderType
{
	/// <summary>
	/// 占位符
	/// </summary>
	None,
	/// <summary>
	/// 买单
	/// </summary>
	Buy,
	/// <summary>
	/// 卖单
	/// </summary>
	Sell,
}
