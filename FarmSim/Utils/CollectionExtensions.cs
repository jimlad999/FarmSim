using System;
using System.Collections.Generic;

namespace FarmSim.Utils;

static class CollectionExtensions
{
    private static readonly Random Rand = new();

    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            var k = Rand.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static T PickRandom<T>(this IList<T> list)
    {
        return list[Rand.Next(list.Count)];
    }

    public static TOut Match<TIn, TOut>(this IEnumerable<TIn> enumerable, Dictionary<TIn, Func<TOut>> returns, TOut defaultValue = default)
    {
        foreach (var value in enumerable)
        {
            if (returns.TryGetValue(value, out var returnFunc))
            {
                return returnFunc();
            }
        }
        return defaultValue;
    }
}
