using System.Runtime.InteropServices;

namespace zms9110750.TreeCollection.Trie;

/// <summary>
/// 负责储存字符的字典树子节点
/// </summary>
/// <param name="parent">父节点</param>
public class TrieNode(TrieBase parent) : TrieBase(parent)
{

	readonly Dictionary<int, HashSet<int>> _token = new Dictionary<int, HashSet<int>>();

	/// <summary>
	/// 储存的字符。若字典树尚未添加此节点应该存在的文本。则为null。
	/// </summary>
	public string? Word { get; private set; }

	/// <inheritdoc/>
	/// <remarks>获取根节点的分隔符</remarks>
	internal override IReadOnlySet<char> Separator => Root.Separator;

	/// <inheritdoc/>
	public override void Add(string word)
	{
		var index = Depth - 1;
		if (index >= word.Length)
		{
			Word = word;
			return;
		} 
		base[word[index]].Add(word);
	} 

	internal IEnumerable<string> Search(string s, int index, int tokenIndex)
	{
		ref var set = ref CollectionsMarshal.GetValueRefOrAddDefault(_token, tokenIndex, out _);
		set ??= [];
		if (!set.Add(index))
		{
			yield break;
		}

		if (index >= s.Length)
		{
			if (Word != null)
				yield return Word;
			foreach (var child in Children.Values)
			{
				foreach (var result in child.Search(s, index, tokenIndex))
				{
					yield return result;
				}
			}
			yield break;
		}
		var currentChar = s[index];
		if (Separator.Contains(currentChar))
		{
			if (Children.TryGetValue(currentChar, out var childNode))
			{
				foreach (var match in childNode.Search(s, index + 1, tokenIndex))
					yield return match;
			}
			foreach (var item in Children.Values)
			{
				foreach (var match in item.Search(s, index, tokenIndex))
					yield return match;
			}
		}
		else
		{
			if (Children.TryGetValue(currentChar, out var childNode))
			{
				foreach (var match in childNode.Search(s, index + 1, tokenIndex))
					yield return match;
			}
		}
	}
	internal override void ReleaseToken(int tokenIndex)
	{
		base.ReleaseToken(tokenIndex);
		if (_token.TryGetValue(tokenIndex, out var set))
		{
			set.Clear();
		}
	}
}