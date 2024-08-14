using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zms9110750Library.RecipeBalancing;

/// <summary>
/// 配方
/// </summary>
/// <param name="name">配方名</param>
/// <param name="time">消耗时间</param>
public record Recipe(string Name, TimeSpan Time)
{
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
		sb.Append(Time);
		sb.Append(']');
		sb.Append('+');
		sb.Append('[');
		sb.AppendJoin(',', Inputs?.Select(ToString)!);
		sb.Append(']');

		sb.Append('=', 2);
		sb.Append('(');
		sb.AppendJoin(',', Catalysts?.Select(ToString)!);
		sb.Append(')');
		sb.Append('=', 2);
		sb.Append('>');

		sb.Append('[');
		sb.AppendJoin(',', Outputs?.Select(ToString)!);
		sb.Append(']');

		return sb.ToString();
	}

	public static Recipe CraftingCalculator(IEnumerable<Recipe> recipes, Dictionary<string, double> finalProducts)
	{
		double Creat(string k, double arg)
		{
			return arg;
		}
		double Update(string k, double v, double arg)
		{
			return v + arg;
		}

		ArgumentNullException.ThrowIfNull(recipes);
		ArgumentNullException.ThrowIfNull(finalProducts);

		var recipeMap = recipes
	   .SelectMany(recipe => recipe.Outputs, (recipe, output) => (output.Key, recipe))
	   .GroupBy(x => x.Key, b => b.recipe)
	   .ToDictionary(group => group.Key, group => group.First());


		var time = recipeMap.Join(finalProducts, s => s.Key, v => v.Key
		, (a, b) => a.Value.Time
		).Max();

		ConcurrentDictionary<string, double> 配方 = [];
		ConcurrentDictionary<string, double> 所需 = new ConcurrentDictionary<string, double>(finalProducts);
		ConcurrentDictionary<string, double> 亏欠 = [];
		ConcurrentDictionary<string, double> 原料 = [];
		ConcurrentDictionary<string, double> 多余 = new ConcurrentDictionary<string, double>(finalProducts);
		while (!所需.IsEmpty)
		{
			foreach (var (k, v) in 所需)
			{
				if (recipeMap.TryGetValue(k, out var recipe) && recipe.Outputs.TryGetValue(k, out var output))
				{
					var scale = v / output;//计算配方产物数量与所需数量的比值。
					配方.AddOrUpdate(recipe.Name, Creat, Update, recipe.Time / time * scale);
					foreach (var item in recipe.Inputs ?? [])
					{
						亏欠.AddOrUpdate(item.Key, Creat, Update, scale * item.Value);
					}
					foreach (var item in recipe.Outputs)
					{
						if (item.Key != k)
						{
							多余.AddOrUpdate(item.Key, Creat, Update, scale * item.Value);
						}
					}
				}
				else
				{
					原料.AddOrUpdate(k, Creat, Update, v);
				}
			}
			所需.Clear();
			foreach (var (k, v) in 亏欠)
			{
				多余.Remove(k, out var more);
				more -= v;
				if (more < 0)
				{
					所需.AddOrUpdate(k, Creat, Update, -more);
				}
				else if (more > 0)
				{
					多余.AddOrUpdate(k, Creat, Update, more);
				}
			}
			亏欠.Clear();
		}

		var name = $"[]";

		var factory = new Recipe(name, time)
		{
			Inputs = new Dictionary<string, double>(原料),
			Outputs = new Dictionary<string, double>(多余),
			Catalysts = new Dictionary<string, double>(配方)
		};
		return factory;
	}

	static string ToString(KeyValuePair<string, double> pair)
	{
		return pair.Value == 1 ? pair.Key : $"{pair.Value:0.##}*{pair.Key}";
	}
}
