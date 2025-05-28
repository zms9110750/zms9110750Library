using System.Runtime.InteropServices;

namespace zms9110750.TreeCollection.Trie;
public class TrieNode(Trie parent) : TrieBase(parent)
{
	Dictionary<int, HashSet<int>> _token = new Dictionary<int, HashSet<int>>();
	public string? Word { get; private set; }
	internal override IReadOnlySet<char> Separator => Root.Separator;
	public void Add(string word, int index)
	{
		if (index >= word.Length)
		{
			Word = word;
			return;
		}
		this[word[index]].Add(word, index + 1);
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