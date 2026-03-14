using Canalot.Wrappers;
using System;
using System.Collections.Generic;

namespace Canalot.Extensions;

public static partial class CanalotExtensions
{

    // ========== Replace 无条件替换返回值 ==========

    /// <summary>
    /// 无条件替换为新的值
    /// </summary>
    public static TNew Replace<T, TNew>(this T value, TNew newValue)
    {
        return newValue;
    }

    /// <summary>
    /// 通过函数生成新值
    /// </summary>
    public static TNew Replace<T, TNew>(this T value, Func<T, TNew> func)
    {
        return func(value);
    }
    /// <summary>
    /// 开始一个 if 链
    /// </summary>
    public static IfElse<T> ToIf<T>(this T value, bool condition)
    {
        return new IfElse<T>(value, condition);
    }
}
