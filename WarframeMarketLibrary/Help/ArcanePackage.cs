
using System.Collections;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using WarframeMarketLibrary.Model.Item;

namespace WarframeMarketLibrary.Help;
/// <summary>
/// 赋能包
/// </summary>
/// <param name="packageName"></param>
public class ArcanePackage(string packageName) : ILookup<ItemSubtypes, string>
{
	/// <summary>
	/// 包名
	/// </summary>
	public string Name { get; } = packageName;
	/// <summary>
	/// 这个品质下的道具
	/// </summary>
	/// <param name="key">品质</param>
	/// <returns></returns>
	public IEnumerable<string> this[ItemSubtypes key] => QualityToItems.GetValueOrDefault(key) ?? [];
	Dictionary<ItemSubtypes, HashSet<string>> QualityToItems { get; } = [];
	Dictionary<ItemSubtypes, double> Quality { get; } = [];
	Dictionary<string, ItemSubtypes> QualityByItems { get; } = [];
	/// <inheritdoc/>
	public int Count => QualityToItems.Count;
	/// <summary>
	/// 添加一个稀有度的赋能列表
	/// </summary>
	/// <param name="subtype">品级</param>
	/// <param name="quality">稀有度</param>
	/// <param name="strings">赋能列表</param>
	public void Add(ItemSubtypes subtype, double quality, IEnumerable<string> strings)
	{
		QualityToItems.Add(subtype, [.. strings]);
		Quality.Add(subtype, quality);
		foreach (var item in strings)
		{
			QualityByItems.Add(item, subtype);
		}
	}
	/// <summary>
	/// 获取一个品质的概率综合
	/// </summary>
	/// <param name="subtype">品质</param>
	/// <returns>品质出现概率</returns>
	public double GetProbability(ItemSubtypes subtype)
	{
		return Quality.GetValueOrDefault(subtype);
	}
	/// <summary>
	/// 获取一个道具在这个包中的出现率
	/// </summary>
	/// <param name="itemName">道具名</param>
	/// <returns>道具出现概率</returns>
	public double GetProbability(string itemName)
	{
		var subtype = QualityByItems.GetValueOrDefault(itemName);
		return subtype == default ? 0 : GetProbability(subtype) / QualityToItems[subtype].Count;
	}
	/// <inheritdoc/>
	public IEnumerator<IGrouping<ItemSubtypes, string>> GetEnumerator()
	{
		return QualityToItems.Select(s => (IGrouping<ItemSubtypes, string>)new Group(s.Key, s.Value)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	/// <inheritdoc/>
	public bool Contains(ItemSubtypes key)
	{
		return QualityToItems.ContainsKey(key);
	}
	class Group(ItemSubtypes key, IEnumerable<string> strings) : IGrouping<ItemSubtypes, string>
	{
		public ItemSubtypes Key { get; } = key;
		IEnumerator<string> Enumerator { get; } = strings.GetEnumerator();

		public IEnumerator<string> GetEnumerator()
		{
			return Enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	/// <summary>
	/// 从资源文件中加载赋能包
	/// </summary>
	/// <returns></returns>
	public static ArcanePackage[] Create()
	{
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WarframeMarketLibrary.Help.赋能包.xml");
		using StreamReader reader = new StreamReader(stream!, Encoding.UTF8);
		var xel = XElement.Parse(reader.ReadToEnd());
		return xel.Elements().Select(Parse).ToArray();

		static ArcanePackage Parse(XElement source)
		{
			var pack = new ArcanePackage(source.Name.ToString());
			foreach (var item in source.Elements())
			{ 
				pack.Add(Enum.Parse<ItemSubtypes>(item.Name.ToString()), (double)item.Attribute("Quality")!, item.Elements().Select(s => s.Name.ToString()));
			}
			return pack;
		}
	}
}
