using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketLibrary.Model.Item;
using zms9110750.TreeCollection.Trie;

namespace WarframeMarketLibrary.Help;
/// <summary>
/// WarframeMarket的物品信息的只读缓存。可以模糊搜索。
/// </summary>
public class ItemCache : IReadOnlyDictionary<string, ItemShort>
{
	Dictionary<string, ItemShort> KeyOfItem { get; } = new Dictionary<string, ItemShort>();
	Trie Trie { get; } = new Trie([' ', '·', '_']);

	/// <inheritdoc/> 
	public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem).Keys;

	/// <inheritdoc/> 
	public IEnumerable<ItemShort> Values => ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem).Values;

	/// <inheritdoc/> 
	public int Count => ((IReadOnlyCollection<KeyValuePair<string, ItemShort>>)KeyOfItem).Count;

	/// <inheritdoc/> 
	public ItemShort this[string key] => ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem)[key];
	/// <summary>
	/// 用一个Items对象构造缓存
	/// </summary>
	/// <param name="items"></param>
	public ItemCache(Model.Item.ItemList items) : this(items.Data) { }

	/// <summary>
	/// 用一个<see cref="ItemShort"/>序列构造缓存
	/// </summary>
	/// <param name="items"></param>
	public ItemCache(IEnumerable<ItemShort> items)
	{
		foreach (var item in items)
		{
			KeyOfItem.TryAdd(item.Id, item);
			KeyOfItem.TryAdd(item.Slug, item);
			Trie.Add(item.Id);
			Trie.Add(item.Slug);
			foreach (var i18n in item.I18n)
			{
				KeyOfItem.TryAdd(i18n.Value.Name, item);
				Trie.Add(i18n.Value.Name);
			}
		}
	}

	/// <inheritdoc/> 
	public bool ContainsKey(string key)
	{
		return ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem).ContainsKey(key);
	}

	/// <inheritdoc/> 
	public bool TryGetValue(string key, [MaybeNullWhen(false)] out ItemShort value)
	{
		return ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem).TryGetValue(key, out value);
	}

	/// <inheritdoc/> 
	public IEnumerator<KeyValuePair<string, ItemShort>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<string, ItemShort>>)KeyOfItem).GetEnumerator();
	}

	/// <inheritdoc/> 
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)KeyOfItem).GetEnumerator();
	}

	/// <summary>
	/// 用字典树搜索，查询指定前缀的物品
	/// </summary>
	/// <param name="itemNamePart">物品名字部分</param>
	/// <returns></returns>
	/// <remarks>空格和中文的·会作为分隔符来拆分单词为节。搜索时会匹配节的开头按字符逐字匹配。分隔符可以忽略本节剩余内容和之后的任意节。</remarks>
	public IEnumerable<string> Search(string itemNamePart)
	{
		return Trie.Search(itemNamePart);
	}

	/// <summary>
	/// 用字典树搜索，查询指定前缀的物品
	/// </summary>
	/// <param name="itemNamePart">物品名字部分</param>
	/// <returns></returns>
	/// <remarks>空格和中文的·会作为分隔符来拆分单词为节。搜索时会匹配节的开头按字符逐字匹配。分隔符可以忽略本节剩余内容和之后的任意节。</remarks>
	public IEnumerable<ItemShort> SearchItems(string itemNamePart)
	{
		return Trie.Search(itemNamePart).Select(s => this[s]!).Distinct();
	}
}
