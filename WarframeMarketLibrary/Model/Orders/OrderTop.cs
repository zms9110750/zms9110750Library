﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketLibrary.Model.Orders;
/// <summary>
/// 在线玩家买卖订单的前五
/// </summary>
/// <param name="Buy">买单列表</param>
/// <param name="Sell">卖单列表</param>
public record OrderTop(
	Order[] Buy,
	Order[] Sell);