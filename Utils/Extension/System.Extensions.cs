// 自动生成的扩展方法
// 程序集：mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System 命名空间下的类型共 148 个

using System.Collections;
using System.Globalization;

namespace zms9110750.Utils.Extension
{
    /// <summary>
    /// AppDomain 类型的扩展方法
    /// </summary>
    public static class AppDomainExtensions
    {
        /// <inheritdoc cref="AppDomain.Unload(AppDomain)"/>
        public static void Unload(
            this AppDomain domain)
        {
            AppDomain.Unload(domain: domain);
        }
    }

    /// <summary>
    /// Array 类型的扩展方法
    /// </summary>
    public static class ArrayExtensions
    {
        /// <inheritdoc cref="Array.BinarySearch(Array, Object)"/>
        public static Int32 BinarySearch(
            this Array array, Object value)
        {
            return Array.BinarySearch(array: array, value: value);
        }

        /// <inheritdoc cref="Array.BinarySearch(Array, Int32, Int32, Object)"/>
        public static Int32 BinarySearch(
            this Array array, Int32 index, Int32 length, Object value)
        {
            return Array.BinarySearch(array: array, index: index, length: length, value: value);
        }

        /// <inheritdoc cref="Array.BinarySearch(Array, Object, IComparer)"/>
        public static Int32 BinarySearch(
            this Array array, Object value, IComparer comparer)
        {
            return Array.BinarySearch(array: array, value: value, comparer: comparer);
        }

        /// <inheritdoc cref="Array.BinarySearch(Array, Int32, Int32, Object, IComparer)"/>
        public static Int32 BinarySearch(
            this Array array, Int32 index, Int32 length, Object value, IComparer comparer)
        {
            return Array.BinarySearch(array: array, index: index, length: length, value: value, comparer: comparer);
        }

        /// <inheritdoc cref="Array.Clear(Array, Int32, Int32)"/>
        public static void Clear(
            this Array array, Int32 index, Int32 length)
        {
            Array.Clear(array: array, index: index, length: length);
        }

        /// <inheritdoc cref="Array.ConstrainedCopy(Array, Int32, Array, Int32, Int32)"/>
        public static void ConstrainedCopy(
            this Array sourceArray, Int32 sourceIndex, Array destinationArray, Int32 destinationIndex, Int32 length)
        {
            Array.ConstrainedCopy(sourceArray: sourceArray, sourceIndex: sourceIndex, destinationArray: destinationArray, destinationIndex: destinationIndex, length: length);
        }

        /// <inheritdoc cref="Array.Copy(Array, Array, Int32)"/>
        public static void Copy(
            this Array sourceArray, Array destinationArray, Int32 length)
        {
            Array.Copy(sourceArray: sourceArray, destinationArray: destinationArray, length: length);
        }

        /// <inheritdoc cref="Array.Copy(Array, Int32, Array, Int32, Int32)"/>
        public static void Copy(
            this Array sourceArray, Int32 sourceIndex, Array destinationArray, Int32 destinationIndex, Int32 length)
        {
            Array.Copy(sourceArray: sourceArray, sourceIndex: sourceIndex, destinationArray: destinationArray, destinationIndex: destinationIndex, length: length);
        }

        /// <inheritdoc cref="Array.Copy(Array, Array, Int64)"/>
        public static void Copy(
            this Array sourceArray, Array destinationArray, Int64 length)
        {
            Array.Copy(sourceArray: sourceArray, destinationArray: destinationArray, length: length);
        }

        /// <inheritdoc cref="Array.Copy(Array, Int64, Array, Int64, Int64)"/>
        public static void Copy(
            this Array sourceArray, Int64 sourceIndex, Array destinationArray, Int64 destinationIndex, Int64 length)
        {
            Array.Copy(sourceArray: sourceArray, sourceIndex: sourceIndex, destinationArray: destinationArray, destinationIndex: destinationIndex, length: length);
        }

        /// <inheritdoc cref="Array.IndexOf(Array, Object)"/>
        public static Int32 IndexOf(
            this Array array, Object value)
        {
            return Array.IndexOf(array: array, value: value);
        }

        /// <inheritdoc cref="Array.IndexOf(Array, Object, Int32)"/>
        public static Int32 IndexOf(
            this Array array, Object value, Int32 startIndex)
        {
            return Array.IndexOf(array: array, value: value, startIndex: startIndex);
        }

        /// <inheritdoc cref="Array.IndexOf(Array, Object, Int32, Int32)"/>
        public static Int32 IndexOf(
            this Array array, Object value, Int32 startIndex, Int32 count)
        {
            return Array.IndexOf(array: array, value: value, startIndex: startIndex, count: count);
        }

        /// <inheritdoc cref="Array.LastIndexOf(Array, Object)"/>
        public static Int32 LastIndexOf(
            this Array array, Object value)
        {
            return Array.LastIndexOf(array: array, value: value);
        }

        /// <inheritdoc cref="Array.LastIndexOf(Array, Object, Int32)"/>
        public static Int32 LastIndexOf(
            this Array array, Object value, Int32 startIndex)
        {
            return Array.LastIndexOf(array: array, value: value, startIndex: startIndex);
        }

        /// <inheritdoc cref="Array.LastIndexOf(Array, Object, Int32, Int32)"/>
        public static Int32 LastIndexOf(
            this Array array, Object value, Int32 startIndex, Int32 count)
        {
            return Array.LastIndexOf(array: array, value: value, startIndex: startIndex, count: count);
        }

        /// <inheritdoc cref="Array.Reverse(Array)"/>
        public static void Reverse(
            this Array array)
        {
            Array.Reverse(array: array);
        }

        /// <inheritdoc cref="Array.Reverse(Array, Int32, Int32)"/>
        public static void Reverse(
            this Array array, Int32 index, Int32 length)
        {
            Array.Reverse(array: array, index: index, length: length);
        }

        /// <inheritdoc cref="Array.Sort(Array)"/>
        public static void Sort(
            this Array array)
        {
            Array.Sort(array: array);
        }

        /// <inheritdoc cref="Array.Sort(Array, Array)"/>
        public static void Sort(
            this Array keys, Array items)
        {
            Array.Sort(keys: keys, items: items);
        }

        /// <inheritdoc cref="Array.Sort(Array, Int32, Int32)"/>
        public static void Sort(
            this Array array, Int32 index, Int32 length)
        {
            Array.Sort(array: array, index: index, length: length);
        }

        /// <inheritdoc cref="Array.Sort(Array, Array, Int32, Int32)"/>
        public static void Sort(
            this Array keys, Array items, Int32 index, Int32 length)
        {
            Array.Sort(keys: keys, items: items, index: index, length: length);
        }

        /// <inheritdoc cref="Array.Sort(Array, IComparer)"/>
        public static void Sort(
            this Array array, IComparer comparer)
        {
            Array.Sort(array: array, comparer: comparer);
        }

        /// <inheritdoc cref="Array.Sort(Array, Array, IComparer)"/>
        public static void Sort(
            this Array keys, Array items, IComparer comparer)
        {
            Array.Sort(keys: keys, items: items, comparer: comparer);
        }

        /// <inheritdoc cref="Array.Sort(Array, Int32, Int32, IComparer)"/>
        public static void Sort(
            this Array array, Int32 index, Int32 length, IComparer comparer)
        {
            Array.Sort(array: array, index: index, length: length, comparer: comparer);
        }

        /// <inheritdoc cref="Array.Sort(Array, Array, Int32, Int32, IComparer)"/>
        public static void Sort(
            this Array keys, Array items, Int32 index, Int32 length, IComparer comparer)
        {
            Array.Sort(keys: keys, items: items, index: index, length: length, comparer: comparer);
        }
    }

    /// <summary>
    /// Char 类型的扩展方法
    /// </summary>
    public static class CharExtensions
    {
        /// <inheritdoc cref="Char.ConvertToUtf32(Char, Char)"/>
        public static Int32 ConvertToUtf32(
            this Char highSurrogate, Char lowSurrogate)
        {
            return Char.ConvertToUtf32(highSurrogate: highSurrogate, lowSurrogate: lowSurrogate);
        }

        /// <inheritdoc cref="Char.GetNumericValue(Char)"/>
        public static Double GetNumericValue(
            this Char c)
        {
            return Char.GetNumericValue(c: c);
        }

        /// <inheritdoc cref="Char.GetUnicodeCategory(Char)"/>
        public static UnicodeCategory GetUnicodeCategory(
            this Char c)
        {
            return Char.GetUnicodeCategory(c: c);
        }

        /// <inheritdoc cref="Char.IsControl(Char)"/>
        public static Boolean IsControl(
            this Char c)
        {
            return Char.IsControl(c: c);
        }

        /// <inheritdoc cref="Char.IsDigit(Char)"/>
        public static Boolean IsDigit(
            this Char c)
        {
            return Char.IsDigit(c: c);
        }

        /// <inheritdoc cref="Char.IsHighSurrogate(Char)"/>
        public static Boolean IsHighSurrogate(
            this Char c)
        {
            return Char.IsHighSurrogate(c: c);
        }

        /// <inheritdoc cref="Char.IsLetter(Char)"/>
        public static Boolean IsLetter(
            this Char c)
        {
            return Char.IsLetter(c: c);
        }

        /// <inheritdoc cref="Char.IsLetterOrDigit(Char)"/>
        public static Boolean IsLetterOrDigit(
            this Char c)
        {
            return Char.IsLetterOrDigit(c: c);
        }

        /// <inheritdoc cref="Char.IsLower(Char)"/>
        public static Boolean IsLower(
            this Char c)
        {
            return Char.IsLower(c: c);
        }

        /// <inheritdoc cref="Char.IsLowSurrogate(Char)"/>
        public static Boolean IsLowSurrogate(
            this Char c)
        {
            return Char.IsLowSurrogate(c: c);
        }

        /// <inheritdoc cref="Char.IsNumber(Char)"/>
        public static Boolean IsNumber(
            this Char c)
        {
            return Char.IsNumber(c: c);
        }

        /// <inheritdoc cref="Char.IsPunctuation(Char)"/>
        public static Boolean IsPunctuation(
            this Char c)
        {
            return Char.IsPunctuation(c: c);
        }

        /// <inheritdoc cref="Char.IsSeparator(Char)"/>
        public static Boolean IsSeparator(
            this Char c)
        {
            return Char.IsSeparator(c: c);
        }

        /// <inheritdoc cref="Char.IsSurrogate(Char)"/>
        public static Boolean IsSurrogate(
            this Char c)
        {
            return Char.IsSurrogate(c: c);
        }

        /// <inheritdoc cref="Char.IsSurrogatePair(Char, Char)"/>
        public static Boolean IsSurrogatePair(
            this Char highSurrogate, Char lowSurrogate)
        {
            return Char.IsSurrogatePair(highSurrogate: highSurrogate, lowSurrogate: lowSurrogate);
        }

        /// <inheritdoc cref="Char.IsSymbol(Char)"/>
        public static Boolean IsSymbol(
            this Char c)
        {
            return Char.IsSymbol(c: c);
        }

        /// <inheritdoc cref="Char.IsUpper(Char)"/>
        public static Boolean IsUpper(
            this Char c)
        {
            return Char.IsUpper(c: c);
        }

        /// <inheritdoc cref="Char.IsWhiteSpace(Char)"/>
        public static Boolean IsWhiteSpace(
            this Char c)
        {
            return Char.IsWhiteSpace(c: c);
        }

        /// <inheritdoc cref="Char.ToLower(Char, CultureInfo)"/>
        public static Char ToLower(
            this Char c, CultureInfo culture)
        {
            return Char.ToLower(c: c, culture: culture);
        }

        /// <inheritdoc cref="Char.ToLower(Char)"/>
        public static Char ToLower(
            this Char c)
        {
            return Char.ToLower(c: c);
        }

        /// <inheritdoc cref="Char.ToLowerInvariant(Char)"/>
        public static Char ToLowerInvariant(
            this Char c)
        {
            return Char.ToLowerInvariant(c: c);
        }

        /// <inheritdoc cref="Char.ToString(Char)"/>
        public static String ToString(
            this Char c)
        {
            return Char.ToString(c: c);
        }

        /// <inheritdoc cref="Char.ToUpper(Char, CultureInfo)"/>
        public static Char ToUpper(
            this Char c, CultureInfo culture)
        {
            return Char.ToUpper(c: c, culture: culture);
        }

        /// <inheritdoc cref="Char.ToUpper(Char)"/>
        public static Char ToUpper(
            this Char c)
        {
            return Char.ToUpper(c: c);
        }

        /// <inheritdoc cref="Char.ToUpperInvariant(Char)"/>
        public static Char ToUpperInvariant(
            this Char c)
        {
            return Char.ToUpperInvariant(c: c);
        }
    }

    /// <summary>
    /// DateTime 类型的扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <inheritdoc cref="DateTime.Compare(DateTime, DateTime)"/>
        public static Int32 Compare(
            this DateTime t1, DateTime t2)
        {
            return DateTime.Compare(t1: t1, t2: t2);
        }

        /// <inheritdoc cref="DateTime.Equals(DateTime, DateTime)"/>
        public static Boolean Equals(
            this DateTime t1, DateTime t2)
        {
            return DateTime.Equals(t1: t1, t2: t2);
        }

        /// <inheritdoc cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/>
        public static DateTime SpecifyKind(
            this DateTime value, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(value: value, kind: kind);
        }
    }

    /// <summary>
    /// DateTimeOffset 类型的扩展方法
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <inheritdoc cref="DateTimeOffset.Compare(DateTimeOffset, DateTimeOffset)"/>
        public static Int32 Compare(
            this DateTimeOffset first, DateTimeOffset second)
        {
            return DateTimeOffset.Compare(first: first, second: second);
        }

        /// <inheritdoc cref="DateTimeOffset.Equals(DateTimeOffset, DateTimeOffset)"/>
        public static Boolean Equals(
            this DateTimeOffset first, DateTimeOffset second)
        {
            return DateTimeOffset.Equals(first: first, second: second);
        }
    }

    /// <summary>
    /// Decimal 类型的扩展方法
    /// </summary>
    public static class DecimalExtensions
    {
        /// <inheritdoc cref="Decimal.Add(Decimal, Decimal)"/>
        public static Decimal Add(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Add(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.Ceiling(Decimal)"/>
        public static Decimal Ceiling(
            this Decimal d)
        {
            return Decimal.Ceiling(d: d);
        }

        /// <inheritdoc cref="Decimal.Compare(Decimal, Decimal)"/>
        public static Int32 Compare(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Compare(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.Divide(Decimal, Decimal)"/>
        public static Decimal Divide(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Divide(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.Equals(Decimal, Decimal)"/>
        public static Boolean Equals(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Equals(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.Floor(Decimal)"/>
        public static Decimal Floor(
            this Decimal d)
        {
            return Decimal.Floor(d: d);
        }

        /// <inheritdoc cref="Decimal.GetBits(Decimal)"/>
        public static Int32[] GetBits(
            this Decimal d)
        {
            return Decimal.GetBits(d: d);
        }

        /// <inheritdoc cref="Decimal.Multiply(Decimal, Decimal)"/>
        public static Decimal Multiply(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Multiply(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.Negate(Decimal)"/>
        public static Decimal Negate(
            this Decimal d)
        {
            return Decimal.Negate(d: d);
        }

        /// <inheritdoc cref="Decimal.Remainder(Decimal, Decimal)"/>
        public static Decimal Remainder(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Remainder(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.Round(Decimal)"/>
        public static Decimal Round(
            this Decimal d)
        {
            return Decimal.Round(d: d);
        }

        /// <inheritdoc cref="Decimal.Round(Decimal, Int32)"/>
        public static Decimal Round(
            this Decimal d, Int32 decimals)
        {
            return Decimal.Round(d: d, decimals: decimals);
        }

        /// <inheritdoc cref="Decimal.Round(Decimal, MidpointRounding)"/>
        public static Decimal Round(
            this Decimal d, MidpointRounding mode)
        {
            return Decimal.Round(d: d, mode: mode);
        }

        /// <inheritdoc cref="Decimal.Round(Decimal, Int32, MidpointRounding)"/>
        public static Decimal Round(
            this Decimal d, Int32 decimals, MidpointRounding mode)
        {
            return Decimal.Round(d: d, decimals: decimals, mode: mode);
        }

        /// <inheritdoc cref="Decimal.Subtract(Decimal, Decimal)"/>
        public static Decimal Subtract(
            this Decimal d1, Decimal d2)
        {
            return Decimal.Subtract(d1: d1, d2: d2);
        }

        /// <inheritdoc cref="Decimal.ToByte(Decimal)"/>
        public static Byte ToByte(
            this Decimal value)
        {
            return Decimal.ToByte(value: value);
        }

        /// <inheritdoc cref="Decimal.ToDouble(Decimal)"/>
        public static Double ToDouble(
            this Decimal d)
        {
            return Decimal.ToDouble(d: d);
        }

        /// <inheritdoc cref="Decimal.ToInt16(Decimal)"/>
        public static Int16 ToInt16(
            this Decimal value)
        {
            return Decimal.ToInt16(value: value);
        }

        /// <inheritdoc cref="Decimal.ToInt32(Decimal)"/>
        public static Int32 ToInt32(
            this Decimal d)
        {
            return Decimal.ToInt32(d: d);
        }

        /// <inheritdoc cref="Decimal.ToInt64(Decimal)"/>
        public static Int64 ToInt64(
            this Decimal d)
        {
            return Decimal.ToInt64(d: d);
        }

        /// <inheritdoc cref="Decimal.ToOACurrency(Decimal)"/>
        public static Int64 ToOACurrency(
            this Decimal value)
        {
            return Decimal.ToOACurrency(value: value);
        }

        /// <inheritdoc cref="Decimal.ToSByte(Decimal)"/>
        public static SByte ToSByte(
            this Decimal value)
        {
            return Decimal.ToSByte(value: value);
        }

        /// <inheritdoc cref="Decimal.ToSingle(Decimal)"/>
        public static Single ToSingle(
            this Decimal d)
        {
            return Decimal.ToSingle(d: d);
        }

        /// <inheritdoc cref="Decimal.ToUInt16(Decimal)"/>
        public static UInt16 ToUInt16(
            this Decimal value)
        {
            return Decimal.ToUInt16(value: value);
        }

        /// <inheritdoc cref="Decimal.ToUInt32(Decimal)"/>
        public static UInt32 ToUInt32(
            this Decimal d)
        {
            return Decimal.ToUInt32(d: d);
        }

        /// <inheritdoc cref="Decimal.ToUInt64(Decimal)"/>
        public static UInt64 ToUInt64(
            this Decimal d)
        {
            return Decimal.ToUInt64(d: d);
        }

        /// <inheritdoc cref="Decimal.Truncate(Decimal)"/>
        public static Decimal Truncate(
            this Decimal d)
        {
            return Decimal.Truncate(d: d);
        }
    }

    /// <summary>
    /// Delegate 类型的扩展方法
    /// </summary>
    public static class DelegateExtensions
    {
        /// <inheritdoc cref="Delegate.Combine(Delegate, Delegate)"/>
        public static Delegate Combine(
            this Delegate a, Delegate b)
        {
            return Delegate.Combine(a: a, b: b);
        }

        /// <inheritdoc cref="Delegate.Remove(Delegate, Delegate)"/>
        public static Delegate Remove(
            this Delegate source, Delegate value)
        {
            return Delegate.Remove(source: source, value: value);
        }

        /// <inheritdoc cref="Delegate.RemoveAll(Delegate, Delegate)"/>
        public static Delegate RemoveAll(
            this Delegate source, Delegate value)
        {
            return Delegate.RemoveAll(source: source, value: value);
        }
    }

    /// <summary>
    /// Double 类型的扩展方法
    /// </summary>
    public static class DoubleExtensions
    {
        /// <inheritdoc cref="Double.IsInfinity(Double)"/>
        public static Boolean IsInfinity(
            this Double d)
        {
            return Double.IsInfinity(d: d);
        }

        /// <inheritdoc cref="Double.IsNaN(Double)"/>
        public static Boolean IsNaN(
            this Double d)
        {
            return Double.IsNaN(d: d);
        }

        /// <inheritdoc cref="Double.IsNegativeInfinity(Double)"/>
        public static Boolean IsNegativeInfinity(
            this Double d)
        {
            return Double.IsNegativeInfinity(d: d);
        }

        /// <inheritdoc cref="Double.IsPositiveInfinity(Double)"/>
        public static Boolean IsPositiveInfinity(
            this Double d)
        {
            return Double.IsPositiveInfinity(d: d);
        }
    }

    /// <summary>
    /// FormattableString 类型的扩展方法
    /// </summary>
    public static class FormattableStringExtensions
    {
        /// <inheritdoc cref="FormattableString.Invariant(FormattableString)"/>
        public static String Invariant(
            this FormattableString formattable)
        {
            return FormattableString.Invariant(formattable: formattable);
        }
    }

    /// <summary>
    /// IntPtr 类型的扩展方法
    /// </summary>
    public static class IntPtrExtensions
    {
        /// <inheritdoc cref="IntPtr.Add(IntPtr, Int32)"/>
        public static IntPtr Add(
            this IntPtr pointer, Int32 offset)
        {
            return IntPtr.Add(pointer: pointer, offset: offset);
        }

        /// <inheritdoc cref="IntPtr.Subtract(IntPtr, Int32)"/>
        public static IntPtr Subtract(
            this IntPtr pointer, Int32 offset)
        {
            return IntPtr.Subtract(pointer: pointer, offset: offset);
        }
    }

    /// <summary>
    /// Single 类型的扩展方法
    /// </summary>
    public static class SingleExtensions
    {
        /// <inheritdoc cref="Single.IsInfinity(Single)"/>
        public static Boolean IsInfinity(
            this Single f)
        {
            return Single.IsInfinity(f: f);
        }

        /// <inheritdoc cref="Single.IsNaN(Single)"/>
        public static Boolean IsNaN(
            this Single f)
        {
            return Single.IsNaN(f: f);
        }

        /// <inheritdoc cref="Single.IsNegativeInfinity(Single)"/>
        public static Boolean IsNegativeInfinity(
            this Single f)
        {
            return Single.IsNegativeInfinity(f: f);
        }

        /// <inheritdoc cref="Single.IsPositiveInfinity(Single)"/>
        public static Boolean IsPositiveInfinity(
            this Single f)
        {
            return Single.IsPositiveInfinity(f: f);
        }
    }

    /// <summary>
    /// String 类型的扩展方法
    /// </summary>
    public static class StringExtensions
    {
        /// <inheritdoc cref="String.Compare(String, String)"/>
        public static Int32 Compare(
            this String strA, String strB)
        {
            return String.Compare(strA: strA, strB: strB);
        }

        /// <inheritdoc cref="String.Compare(String, String, Boolean)"/>
        public static Int32 Compare(
            this String strA, String strB, Boolean ignoreCase)
        {
            return String.Compare(strA: strA, strB: strB, ignoreCase: ignoreCase);
        }

        /// <inheritdoc cref="String.Compare(String, String, StringComparison)"/>
        public static Int32 Compare(
            this String strA, String strB, StringComparison comparisonType)
        {
            return String.Compare(strA: strA, strB: strB, comparisonType: comparisonType);
        }

        /// <inheritdoc cref="String.Compare(String, String, CultureInfo, CompareOptions)"/>
        public static Int32 Compare(
            this String strA, String strB, CultureInfo culture, CompareOptions options)
        {
            return String.Compare(strA: strA, strB: strB, culture: culture, options: options);
        }

        /// <inheritdoc cref="String.Compare(String, String, Boolean, CultureInfo)"/>
        public static Int32 Compare(
            this String strA, String strB, Boolean ignoreCase, CultureInfo culture)
        {
            return String.Compare(strA: strA, strB: strB, ignoreCase: ignoreCase, culture: culture);
        }

        /// <inheritdoc cref="String.Compare(String, Int32, String, Int32, Int32)"/>
        public static Int32 Compare(
            this String strA, Int32 indexA, String strB, Int32 indexB, Int32 length)
        {
            return String.Compare(strA: strA, indexA: indexA, strB: strB, indexB: indexB, length: length);
        }

        /// <inheritdoc cref="String.Compare(String, Int32, String, Int32, Int32, Boolean)"/>
        public static Int32 Compare(
            this String strA, Int32 indexA, String strB, Int32 indexB, Int32 length, Boolean ignoreCase)
        {
            return String.Compare(strA: strA, indexA: indexA, strB: strB, indexB: indexB, length: length, ignoreCase: ignoreCase);
        }

        /// <inheritdoc cref="String.Compare(String, Int32, String, Int32, Int32, Boolean, CultureInfo)"/>
        public static Int32 Compare(
            this String strA, Int32 indexA, String strB, Int32 indexB, Int32 length, Boolean ignoreCase, CultureInfo culture)
        {
            return String.Compare(strA: strA, indexA: indexA, strB: strB, indexB: indexB, length: length, ignoreCase: ignoreCase, culture: culture);
        }

        /// <inheritdoc cref="String.Compare(String, Int32, String, Int32, Int32, CultureInfo, CompareOptions)"/>
        public static Int32 Compare(
            this String strA, Int32 indexA, String strB, Int32 indexB, Int32 length, CultureInfo culture, CompareOptions options)
        {
            return String.Compare(strA: strA, indexA: indexA, strB: strB, indexB: indexB, length: length, culture: culture, options: options);
        }

        /// <inheritdoc cref="String.Compare(String, Int32, String, Int32, Int32, StringComparison)"/>
        public static Int32 Compare(
            this String strA, Int32 indexA, String strB, Int32 indexB, Int32 length, StringComparison comparisonType)
        {
            return String.Compare(strA: strA, indexA: indexA, strB: strB, indexB: indexB, length: length, comparisonType: comparisonType);
        }

        /// <inheritdoc cref="String.CompareOrdinal(String, String)"/>
        public static Int32 CompareOrdinal(
            this String strA, String strB)
        {
            return String.CompareOrdinal(strA: strA, strB: strB);
        }

        /// <inheritdoc cref="String.CompareOrdinal(String, Int32, String, Int32, Int32)"/>
        public static Int32 CompareOrdinal(
            this String strA, Int32 indexA, String strB, Int32 indexB, Int32 length)
        {
            return String.CompareOrdinal(strA: strA, indexA: indexA, strB: strB, indexB: indexB, length: length);
        }

        /// <inheritdoc cref="String.Concat(String, String)"/>
        public static String Concat(
            this String str0, String str1)
        {
            return String.Concat(str0: str0, str1: str1);
        }

        /// <inheritdoc cref="String.Concat(String, String, String)"/>
        public static String Concat(
            this String str0, String str1, String str2)
        {
            return String.Concat(str0: str0, str1: str1, str2: str2);
        }

        /// <inheritdoc cref="String.Concat(String, String, String, String)"/>
        public static String Concat(
            this String str0, String str1, String str2, String str3)
        {
            return String.Concat(str0: str0, str1: str1, str2: str2, str3: str3);
        }

        /// <inheritdoc cref="String.Copy(String)"/>
        public static String Copy(
            this String str)
        {
            return String.Copy(str: str);
        }

        /// <inheritdoc cref="String.Equals(String, String)"/>
        public static Boolean Equals(
            this String a, String b)
        {
            return String.Equals(a: a, b: b);
        }

        /// <inheritdoc cref="String.Equals(String, String, StringComparison)"/>
        public static Boolean Equals(
            this String a, String b, StringComparison comparisonType)
        {
            return String.Equals(a: a, b: b, comparisonType: comparisonType);
        }

        /// <inheritdoc cref="String.Format(String, Object)"/>
        public static String Format(
            this String format, Object arg0)
        {
            return String.Format(format: format, arg0: arg0);
        }

        /// <inheritdoc cref="String.Format(String, Object, Object)"/>
        public static String Format(
            this String format, Object arg0, Object arg1)
        {
            return String.Format(format: format, arg0: arg0, arg1: arg1);
        }

        /// <inheritdoc cref="String.Format(String, Object, Object, Object)"/>
        public static String Format(
            this String format, Object arg0, Object arg1, Object arg2)
        {
            return String.Format(format: format, arg0: arg0, arg1: arg1, arg2: arg2);
        }

        /// <inheritdoc cref="String.Format(String, Object[])"/>
        public static String Format(
            this String format, Object[] args)
        {
            return String.Format(format: format, args: args);
        }

        /// <inheritdoc cref="String.Intern(String)"/>
        public static String Intern(
            this String str)
        {
            return String.Intern(str: str);
        }

        /// <inheritdoc cref="String.IsInterned(String)"/>
        public static String IsInterned(
            this String str)
        {
            return String.IsInterned(str: str);
        }

        /// <inheritdoc cref="String.IsNullOrEmpty(String)"/>
        public static Boolean IsNullOrEmpty(
            this String value)
        {
            return String.IsNullOrEmpty(value: value);
        }

        /// <inheritdoc cref="String.IsNullOrWhiteSpace(String)"/>
        public static Boolean IsNullOrWhiteSpace(
            this String value)
        {
            return String.IsNullOrWhiteSpace(value: value);
        }

        /// <inheritdoc cref="String.Join(String, String[])"/>
        public static String Join(
            this String separator, String[] value)
        {
            return String.Join(separator: separator, value: value);
        }

        /// <inheritdoc cref="String.Join(String, Object[])"/>
        public static String Join(
            this String separator, Object[] values)
        {
            return String.Join(separator: separator, values: values);
        }

        /// <inheritdoc cref="String.Join(String, IEnumerable<String>)"/>
        public static String Join(
            this String separator, IEnumerable<String> values)
        {
            return String.Join(separator: separator, values: values);
        }

        /// <inheritdoc cref="String.Join(String, String[], Int32, Int32)"/>
        public static String Join(
            this String separator, String[] value, Int32 startIndex, Int32 count)
        {
            return String.Join(separator: separator, value: value, startIndex: startIndex, count: count);
        }

        /// <inheritdoc cref="String.Join(String, IEnumerable<T>)"/>
        public static String Join<T>(
            this String separator, IEnumerable<T> values)
        {
            return String.Join(separator: separator, values: values);
        }
    }

    /// <summary>
    /// TimeSpan 类型的扩展方法
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <inheritdoc cref="TimeSpan.Compare(TimeSpan, TimeSpan)"/>
        public static Int32 Compare(
            this TimeSpan t1, TimeSpan t2)
        {
            return TimeSpan.Compare(t1: t1, t2: t2);
        }

        /// <inheritdoc cref="TimeSpan.Equals(TimeSpan, TimeSpan)"/>
        public static Boolean Equals(
            this TimeSpan t1, TimeSpan t2)
        {
            return TimeSpan.Equals(t1: t1, t2: t2);
        }
    }

    /// <summary>
    /// Type 类型的扩展方法
    /// </summary>
    public static class TypeExtensions
    {
        /// <inheritdoc cref="Type.GetTypeCode(Type)"/>
        public static TypeCode GetTypeCode(
            this Type type)
        {
            return Type.GetTypeCode(type: type);
        }
    }

    /// <summary>
    /// TypedReference 类型的扩展方法
    /// </summary>
    public static class TypedReferenceExtensions
    {
        /// <inheritdoc cref="TypedReference.GetTargetType(TypedReference)"/>
        public static Type GetTargetType(
            this TypedReference value)
        {
            return TypedReference.GetTargetType(value: value);
        }

        /// <inheritdoc cref="TypedReference.SetTypedReference(TypedReference, Object)"/>
        public static void SetTypedReference(
            this TypedReference target, Object value)
        {
            TypedReference.SetTypedReference(target: target, value: value);
        }

        /// <inheritdoc cref="TypedReference.TargetTypeToken(TypedReference)"/>
        public static RuntimeTypeHandle TargetTypeToken(
            this TypedReference value)
        {
            return TypedReference.TargetTypeToken(value: value);
        }

        /// <inheritdoc cref="TypedReference.ToObject(TypedReference)"/>
        public static Object ToObject(
            this TypedReference value)
        {
            return TypedReference.ToObject(value: value);
        }
    }

    /// <summary>
    /// UIntPtr 类型的扩展方法
    /// </summary>
    public static class UIntPtrExtensions
    {
        /// <inheritdoc cref="UIntPtr.Add(UIntPtr, Int32)"/>
        public static UIntPtr Add(
            this UIntPtr pointer, Int32 offset)
        {
            return UIntPtr.Add(pointer: pointer, offset: offset);
        }

        /// <inheritdoc cref="UIntPtr.Subtract(UIntPtr, Int32)"/>
        public static UIntPtr Subtract(
            this UIntPtr pointer, Int32 offset)
        {
            return UIntPtr.Subtract(pointer: pointer, offset: offset);
        }
    }

}
