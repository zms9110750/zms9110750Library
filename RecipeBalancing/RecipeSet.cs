using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace zms9110750Library.RecipeBalancing;

/// <summary>
/// 配方集合
/// </summary>
public sealed record RecipeSet : ICollection<Recipe>, ILookup<string, Recipe>
{
	HashSet<Recipe> _recipes = [];
	Lazy<ILookup<string, Recipe>> _lookup;
	 
	public RecipeSet() 
	{
		UpdateLookup();
	}
	[MemberNotNull(nameof(_lookup))]
	private void UpdateLookup()
	{
		if (_lookup is { IsValueCreated: false })
		{
			return;
		}
		_lookup = new Lazy<ILookup<string, Recipe>>(() => _recipes
			.SelectMany(s => s.Outputs, (a, b) => (a, b))
			.ToLookup(a => a.b.Key, b => b.a)
		);
	}

	public override string ToString()
	{
		return string.Join('\n', _recipes);
	}
	public Recipe Balancer(IEnumerable<KeyValuePair<string, double>> finalProducts)
	{
		double Creat(string k, double arg)
		{
			return arg;
		}
		double Update(string k, double v, double arg)
		{
			return v + arg;
		}

		ArgumentNullException.ThrowIfNull(finalProducts);

		var invalid = _lookup;

		if (finalProducts is not IReadOnlyDictionary<string, double> products)
		{
			products = finalProducts.ToDictionary();
		}


		ConcurrentDictionary<string, double> 配方 = [];
		ConcurrentDictionary<string, double> 所需 = [];
		ConcurrentDictionary<string, double> 盈亏 = [];

		double reference = products.Join((ILookup<string, Recipe>)this
							   , s => s.Key
							   , s => s.Key
							   , (a, b) => MinTimeRecipe(b.Key)!.Time).Max();

		foreach (var (product, quantity) in products)
		{
			if (MinTimeRecipe(product) is { } recipe)
			{
				var scale = reference / recipe[product];
				var parallel = double.Ceiling(quantity / scale);
				配方.AddOrUpdate(recipe.Name, Creat, Update, parallel);
				foreach (var item in recipe.Inputs ?? [])
				{
					盈亏.AddOrUpdate(item.Key, Creat, Update, -item.Value * scale * parallel);
				}
				foreach (var item in recipe.Outputs ?? [])
				{
					盈亏.AddOrUpdate(item.Key, Creat, Update, item.Value * scale * parallel);
				}
			}
			else
			{
				盈亏.AddOrUpdate(product, Creat, Update, -quantity);
			}
		}
		do
		{
			所需.Clear();
			foreach (var (product, quantity) in 盈亏)
			{
				if (quantity < 0 && Contains(product))
				{
					所需.AddOrUpdate(product, Creat, Update, products.GetValueOrDefault(product) - quantity);
				}
			}
			foreach (var (product, quantity) in 所需)
			{
				if (MinTimeRecipe(product) is { } recipe)
				{
					配方.AddOrUpdate(recipe.Name, Creat, Update, quantity * recipe[product] / reference);
					foreach (var item in recipe.Inputs ?? [])
					{
						盈亏.AddOrUpdate(item.Key, Creat, Update, -item.Value * quantity);
					}
					foreach (var item in recipe.Outputs ?? [])
					{
						盈亏.AddOrUpdate(item.Key, Creat, Update, item.Value * quantity);
					}
				}
			}
		} while (!所需.IsEmpty);

		return _lookup != invalid
			? throw new InvalidOperationException(" Collection was modified; enumeration operation may not execute.")
			: new Recipe("[" + string.Join(',', finalProducts.Select(Recipe.ToString)) + "]", reference)
			{
				Inputs = new Dictionary<string, double>(盈亏.OrderBy(b => b.Value).TakeWhile(s => s.Value < 0).Select(s => KeyValuePair.Create(s.Key, -s.Value))),
				Outputs = new Dictionary<string, double>(盈亏.OrderByDescending(b => b.Value).TakeWhile(s => s.Value > 0)),
				Catalysts = new Dictionary<string, double>(配方.OrderByDescending(b => b.Value))
			};
	}

	/// <summary>
	/// 找到产出该产品的最短时间配方
	/// </summary>
	/// <param name="product">产品名</param>
	/// <returns>配方</returns>
	/// <remarks>若无记录能产出该产品的订单，则为null</remarks>
	public Recipe? MinTimeRecipe(string product)
	{
		if (!Contains(product))
		{
			return null;
		}
		var a = this[product].MinBy(s => s[product]);
		return double.IsPositiveInfinity(a?[product] ?? double.PositiveInfinity) ? null : a;
	}

	#region 接口 
	public int Count => ((ICollection<Recipe>)_recipes).Count;
	int ILookup<string, Recipe>.Count { get; }
	bool ICollection<Recipe>.IsReadOnly => false;
	public IEnumerable<Recipe> this[string key] => _lookup.Value[key];
	public void Add(Recipe item)
	{
		((ICollection<Recipe>)_recipes).Add(item);
		UpdateLookup();
	}
	public void Clear()
	{
		((ICollection<Recipe>)_recipes).Clear();
		UpdateLookup();
	}
	public bool Contains(Recipe item) => ((ICollection<Recipe>)_recipes).Contains(item);
	public void CopyTo(Recipe[] array, int arrayIndex) => ((ICollection<Recipe>)_recipes).CopyTo(array, arrayIndex);
	public IEnumerator<Recipe> GetEnumerator() => ((IEnumerable<Recipe>)_recipes).GetEnumerator();
	public bool Remove(Recipe item)
	{
		if (((ICollection<Recipe>)_recipes).Remove(item))
		{
			UpdateLookup();
			return true;
		}
		return false;
	}
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_recipes).GetEnumerator();
	public bool Contains(string key) => _lookup.Value.Contains(key);
	IEnumerator<IGrouping<string, Recipe>> IEnumerable<IGrouping<string, Recipe>>.GetEnumerator() => _lookup.Value.GetEnumerator();
	#endregion
}