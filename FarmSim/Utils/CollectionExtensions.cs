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

    public static void Match<T>(this IEnumerable<T> enumerable, Dictionary<T, Action> actions)
    {
        foreach (var value in enumerable)
        {
            if (actions.TryGetValue(value, out var action))
            {
                action();
                return;
            }
        }
    }
}
