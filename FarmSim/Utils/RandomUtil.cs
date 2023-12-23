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
}
