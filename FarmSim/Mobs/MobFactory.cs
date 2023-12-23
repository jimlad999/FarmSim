using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FarmSim.Mobs;

internal class MobFactory
{
    private readonly Dictionary<string, ConstructorInfo> _mobConstructors;

    public MobFactory(MobData[] mobData)
    {
        _mobConstructors = mobData
            .Select(d => d.Class)
            .Distinct()
            .ToDictionary(
                className => className,
                className => Type.GetType(className).GetConstructor(Array.Empty<Type>()));
    }

    public Mob Create(string className)
    {
        return (Mob)_mobConstructors[className].Invoke(Array.Empty<object>());
    }
}