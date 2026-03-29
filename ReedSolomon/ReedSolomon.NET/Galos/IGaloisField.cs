namespace zms9110750.ReedSolomon.Galos
{
    /// <summary>
    /// 伽罗瓦域接口。定义有限域上的基本运算。
    /// </summary>
    /// <typeparam name="T">域元素的类型（byte, ushort, uint 等）</typeparam>
    public interface IGaloisField<T> where T : unmanaged
    {
        /// <summary>
        /// 每个元素占用的位数 m。
        /// </summary>
        int Bits { get; }

        /// <summary>
        /// 本原多项式（低 m 位系数）。
        /// </summary>
        T PrimitivePolynomial { get; }

        /// <summary>
        /// 加法。在 GF(2^m) 中，加法等同于按位异或。
        /// </summary>
        T Add(T a, T b);

        /// <summary>
        /// 减法。在 GF(2^m) 中，减法等同于加法（异或）。
        /// </summary>
        T Subtract(T a, T b);

        /// <summary>
        /// 乘法。使用本原多项式定义的伽罗瓦域乘法。
        /// </summary>
        T Multiply(T a, T b);

        /// <summary>
        /// 除法。返回 a × b⁻¹。
        /// </summary>
        T Divide(T a, T b);

        /// <summary>
        /// 幂运算。返回 aⁿ。
        /// </summary>
        T Power(T a, int exponent);

        /// <summary>
        /// 求逆元。返回 a⁻¹，使得 a × a⁻¹ = 1。
        /// </summary>
        T Inverse(T a);

        /// <summary>
        /// 判断元素是否为零。
        /// </summary>
        bool IsZero(T value);

        /// <summary>
        /// 判断元素是否为一。
        /// </summary>
        bool IsOne(T value);
    }
}
