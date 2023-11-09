using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Complete;

/// <summary>
/// 一个结构，用于二分查找
/// </summary>
/// <remarks>
/// 一个构造函数，用来初始化搜索范围
/// </remarks>
/// <param name="min">搜索范围的最小值</param>
/// <param name="max">搜索范围的最大值</param>
public struct BinarySearcher(int min, int max) : IEquatable<BinarySearcher>
{
	public override readonly string ToString() => (min, Value, max).ToString();
	/// <summary>
	/// 一个整数，表示搜索范围的最小值
	/// </summary>
	private int min = min;
	/// <summary>
	/// 一个整数，表示搜索范围的最大值
	/// </summary>
	private int max = max;
	/// <summary>
	/// 一个布尔值，表示是否已经判断过最小值
	/// </summary>
	private bool checkedMin = false;
	/// <summary>
	/// 一个布尔值，表示是否已经判断过最大值
	/// </summary>
	private bool checkedMax = false;

	/// <summary>
	/// 当前的搜索值
	/// </summary>
	public readonly int Value
	{
		get
		{
			//根据是否已经判断过最大值和最小值，返回不同的值
			return (checkedMax, checkedMin) switch
			{
				//如果没有判断过最大值，返回最大值
				(false, _) => max,
				//如果没有判断过最小值，返回最小值
				(_, false) => min,
				//否则，返回最小值和最大值的平均值
				_ => (min + max) / 2
			};
		}
	}
	/// <summary>
	/// 更新搜索范围
	/// </summary>
	/// <param name="number">是否需要往更大的方向搜索</param>
	/// <returns>一个布尔值，表示这次判断是否对下次返回的值造成影响</returns>
	public bool Update(bool isLarge)
	{
		//根据需要比较的数字和当前的搜索值的大小关系，以及是否已经判断过最大值和最小值，进行不同的操作
		switch (isLarge, checkedMax, checkedMin)
		{
			//已经无法进一步搜索了，返回false。
			case (_, _, _) when min == max:
				return false;
			//如果没有比较过最大值，却比最大值还大，返回false。
			case (true, false, _):
				return false;
			//如果没有比较过最大值，设置最大值已经比较过。
			case (false, false, _):
				checkedMax = true;
				return true;
			//如果没有比较过最小值却比最小值还小，返回false。
			case (false, true, false):
				return false;
			//如果没有比较过最小值，设置最小值已经比较过。
			case (true, true, false):
				checkedMin = true;
				return true;
			//如果最大值和最小值贴近且仍然较大。把最小值+1。
			case (true, true, true) when max - min == 1:
				min++;
				return true;
			//如果需要比较的数字大于当前的搜索值，把最小值设为最大值和最小值的平均数。
			case (true, true, true):
				min = (min + max) / 2;
				return true;
			//如果需要比较的数字小于当前的搜索值，把最大值设为最大值和最小值的平均数。
			case (false, true, true):
				max = (min + max) / 2;
				return true;
			default:
				throw new NotImplementedException("越界");
		}
	}
	/// <summary>
	/// 更新搜索范围
	/// </summary>
	/// <param name="number">一个整数，表示需要比较的数字</param>
	/// <returns>一个布尔值，表示这次判断是否对下次返回的值造成影响</returns>
	public bool Update(int number)
	{
		if (number == Value)
		{
			max = number;
			min = number;
			return false;
		}
		return Update(number > Value);
	}
	#region 重写运算符 
	public static bool operator ==(BinarySearcher left, BinarySearcher right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BinarySearcher left, BinarySearcher right)
	{
		return !(left == right);
	}

	public readonly bool Equals(BinarySearcher other)
	{
		return min == other.min && max == other.max && Value == other.Value;
	}

	public override readonly int GetHashCode() => HashCode.Combine(min, max, checkedMin, checkedMax, Value);

	public override readonly bool Equals(object? obj)
	{
		return obj is BinarySearcher searcher && Equals(searcher);
	}
	#endregion
}
