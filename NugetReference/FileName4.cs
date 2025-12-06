using System.Diagnostics.CodeAnalysis;
namespace NugetReference;

public readonly record struct NoneOr<T>
{
	public T Value => HasValue ? field : throw new InvalidOperationException("Cannot convert an unsuccessful NoneOr to a value.");
	[MemberNotNullWhen(true, nameof(Value))]
	public bool HasValue { get; }
	public NoneOr()
	{
		Value = default!;
		HasValue = false;
	}
	public NoneOr(T? value)
	{
		Value = value!;
		HasValue = value != null;
	}

	[Obsolete("use default(NoneOr<T>)")]
	public static NoneOr<T> None => default;

	public static implicit operator NoneOr<T>(T? value) => new NoneOr<T>(value);


}
public readonly record struct ErrorOr<T>
{
	public T? Value => IsSuccess ? field : throw Error;
	public Exception? Error => IsInitialized ? field : throw new InvalidOperationException("Cannot convert an uninit ErrorOr to a value.");
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => Error is null;
	private bool IsInitialized { get; }

	[Obsolete("don't use this", true)]
	public ErrorOr()
	{
		//什么也不做，令其相当于default。
		//IsInitialized会是false。任何属性访问都会报错。
	}
	public ErrorOr(T value)
	{
		Value = value;
		IsInitialized = true;
	}
	public ErrorOr(Exception error)
	{
		ArgumentNullException.ThrowIfNull(error);
		Error = error;
		IsInitialized = true;
	}
	public static implicit operator ErrorOr<T>(T value) => new ErrorOr<T>(value: value);
	public static implicit operator ErrorOr<T>(Exception error) => new ErrorOr<T>(error: error);


}

public static class NoneOrExtend
{
	public static T? ToNullable<T>(this NoneOr<T> noneOr) where T : struct
	{
		return noneOr.TryGetValue(out var resert) ? resert : null;
	}

	[return: NotNullIfNotNull(nameof(value))]
	public static T GetValueOr<T>(this NoneOr<T> noneOr, T value)
	{
		return noneOr.HasValue ? noneOr.Value : value;
	}
	public static bool TryGetValue<T>(this NoneOr<T> noneOr, out T? resert)
	{
		resert = default;
		if (noneOr.HasValue)
		{
			resert = noneOr.Value;
		}
		return noneOr.HasValue;
	}
	public static void IfNotValue<T>(this NoneOr<T> noneOr, Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		if (!noneOr.HasValue)
		{
			action();
		}
	}
	public static void IfHasValue<T>(this NoneOr<T> noneOr, Action<T> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		if (noneOr.HasValue)
		{
			action(noneOr.Value);
		}
	}
	public static NoneOr<TResult> IfHasValue<T, TResult>(this NoneOr<T> noneOr, Func<T, TResult> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		return noneOr.HasValue ? action(noneOr.Value) : default;
	}
	public static void Switch<T>(this NoneOr<T> noneOr, Action<T> notNone, Action none)
	{
		ArgumentNullException.ThrowIfNull(notNone);
		ArgumentNullException.ThrowIfNull(none);
		if (noneOr.HasValue)
		{
			notNone(noneOr.Value);
		}
		else
		{
			none();
		}
	}
	public static NoneOr<TResult> Switch<T, TResult>(this NoneOr<T> noneOr, Func<T, TResult> notNone, Func<TResult> none)
	{
		ArgumentNullException.ThrowIfNull(notNone);
		ArgumentNullException.ThrowIfNull(none);
		return noneOr.HasValue ? notNone(noneOr.Value) : none();
	}
}
public static class ErrorOrExtend
{
	public static T? GetValueOr<T>(ErrorOr<T> errorOr, T? value)
	{
		return errorOr.IsSuccess ? errorOr.Value : value;
	}
}