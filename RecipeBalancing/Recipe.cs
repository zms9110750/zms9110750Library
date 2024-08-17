using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zms9110750Library.RecipeBalancing;

/// <summary>
/// 配方
/// </summary>
/// <param name="Name">配方名</param>
/// <param name="Time">消耗时间(秒)</param>
public sealed record Recipe(string Name, double Time)
{
	/// <summary>
	/// 生产某个产品所需要的时间
	/// </summary>
	/// <param name="products">产品名</param>
	/// <returns>平均产出每个产品所需时间</returns>
	/// <remarks>如无法生产则返回<see cref="Timeout.InfiniteTimeSpan"/></remarks>
	public double this[string products] => !Outputs.TryGetValue(products, out var value)
		? double.PositiveInfinity
		: value <= 0 ? throw new ArgumentException("该配方产出有0或负值")
		: Time / value;

	/// <summary>
	/// 材料
	/// </summary>
	public Dictionary<string, double>? Inputs { get; init; }

	/// <summary>
	/// 产品
	/// </summary>
	public Dictionary<string, double> Outputs { get; init; } = [];

	/// <summary>
	/// 催化剂或工厂
	/// </summary>
	public Dictionary<string, double>? Catalysts { get; init; }

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder(Name);
		sb.Append(':', 2);
		sb.Append('[');
		sb.Append(TimeSpan.FromSeconds(Time));
		sb.Append(']');
		sb.Append('+');
		sb.Append('[');
		sb.AppendJoin(',', Inputs?.Select(ToString) ?? []);
		sb.Append(']');

		sb.Append('=', 2);
		sb.Append('(');
		sb.AppendJoin(',', Catalysts?.Select(ToString) ?? []);
		sb.Append(')');
		sb.Append('=', 2);
		sb.Append('>');

		sb.Append('[');
		sb.AppendJoin(',', Outputs?.Select(ToString) ?? []);
		sb.Append(']');

		return sb.ToString();
	}

	public static string ToString(KeyValuePair<string, double> pair)
	{
		return pair.Value == 1 ? pair.Key : $"{pair.Value:0.##}*{pair.Key}";
	}
}
