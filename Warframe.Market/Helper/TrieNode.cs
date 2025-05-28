using System;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace Warframe.Market.Helper;




public class TrieNode
{
	public string? Word { get; private set; } // 存储完整单词
	private Dictionary<char, TrieNode> Children { get; } = [];

	TrieNode this[char c]
	{
		get
		{
			ref var childNode = ref CollectionsMarshal.GetValueRefOrAddDefault(Children, c, out _);
			if (childNode == null)
			{
				childNode = new TrieNode();
			}
			return childNode;
		}
	}

	// 对外暴露的 Add 方法，只接受 string
	public void Add(string word)
	{
		ArgumentException.ThrowIfNullOrEmpty(word);
		AddInternal(word.AsSpan(), word);
	}

	// 内部使用的 Add 方法，接受 ReadOnlySpan<char> 和 string
	private void AddInternal(ReadOnlySpan<char> chars, string word)
	{
		switch (chars)
		{
			// 如果字符为空，存储完整单词
			case []:
				Word = word;
				return;
			// 如果字符是空格，则压缩连续的空格
			case [' ', ..]:
				// 跳过后续的空格
				var trimmedChars = chars.TrimStart(' ');
				if (trimmedChars.Length > 0)
				{
					this[' '].AddInternal(trimmedChars, word);
				}
				return;
			// 处理非空格字符
			case [var firstChar, ..]:
				// 递归插入剩余字符
				this[firstChar].AddInternal(chars.Slice(1), word);
				return;
		}
	}
	public IEnumerable<string> Search(ReadOnlyMemory<char> chars)
	{
		switch (chars.Span)
		{
			// 如果字符为空
			case []:
				if (Word != null)
				{
					yield return Word;
				}
				foreach (var child in Children.Values)
				{
					foreach (var item in child.Search(chars))
					{
						yield return item;
					}
				}
				yield break;
			// 如果字符以空格开头
			case [' ', ..]:
				// 截断空格并递归匹配剩余字符 
				if (Children.TryGetValue(' ', out var spaceNode))
				{
					foreach (var item in spaceNode.Search(chars.TrimStart(' ')))
					{
						yield return item;
					}
				}
				foreach (var child in Children.Values)
				{
					foreach (var item in child.Search(chars))
					{
						yield return item;
					}
				}
				yield break;

			// 如果字符以非空格开头
			case [var firstChar, ..] when Children.TryGetValue(firstChar, out var childNode):
				foreach (var item in childNode.Search(chars.Slice(1)))
				{
					yield return item;
				}
				yield break;
		}
	}
}
