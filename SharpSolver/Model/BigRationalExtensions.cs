using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Numerics;

public static class BigRationalExtensions
{
    public static BigRational Sum(this IEnumerable<BigRational> enumerable)
    {
        return enumerable.Aggregate<BigRational, BigRational>(0, (current, bigRational) => current + bigRational);
    }

    public static string ToFractionString(this BigRational bigRational)
    {
        if (bigRational.Denominator == 1)
            return bigRational.Numerator.ToString("R", CultureInfo.InvariantCulture);
        else
            return bigRational.ToString();
    }
} // BigRationalExtensions