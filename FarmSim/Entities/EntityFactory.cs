﻿using FarmSim.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FarmSim.Entities;

class EntityFactory<T, TData>
    where TData : IClassData
{
    private readonly Dictionary<string, ConstructorInfo> _constructors;

    public EntityFactory(TData[] data)
    {
        _constructors = data
            .Select(d => d.Class)
            .Distinct()
            .ToDictionary(
                className => className,
                className => Type.GetType(className).GetConstructor(Array.Empty<Type>()));
    }

    public T Create(string className)
    {
        return (T)_constructors[className].Invoke(Array.Empty<object>());
    }
}