using System;
using System.Collections.Generic;

namespace FarmSim.Utils;

static partial class CollectionExtensions
{
    public static bool Contains<T>(this IEnumerable<T> enumerable, T value1, T value2)
    {
        var equality = default(EqualityComparer<T>);
        foreach (var value in enumerable)
        {
            if (equality.Equals(value, value1)
                || equality.Equals(value, value2))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Contains<T>(this IEnumerable<T> enumerable, T value1, T value2, T value3)
    {
        var equality = default(EqualityComparer<T>);
        foreach (var value in enumerable)
        {
            if (equality.Equals(value, value1)
                || equality.Equals(value, value2)
                || equality.Equals(value, value3))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Contains<T>(this IEnumerable<T> enumerable, T value1, T value2, T value3, T value4)
    {
        var equality = default(EqualityComparer<T>);
        foreach (var value in enumerable)
        {
            if (equality.Equals(value, value1)
                || equality.Equals(value, value2)
                || equality.Equals(value, value3)
                || equality.Equals(value, value4))
            {
                return true;
            }
        }
        return false;
    }
}
