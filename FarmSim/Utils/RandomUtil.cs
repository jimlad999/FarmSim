using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Utils;

static class RandomUtil
{
    [ThreadStatic]
    private static Random _rand;
    public static Random Rand
    {
        get { return _rand ??= new Random(); }
    }

    public static Vector2 RandomNormalizedDirection()
    {
        var vector = new Vector2(
            x: (float)Rand.NextDouble() * 2 - 1,
            y: (float)Rand.NextDouble() * 2 - 1);
        vector.Normalize();
        return vector;
    }
}
