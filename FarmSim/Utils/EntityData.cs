using System;
using System.Collections.Generic;

namespace FarmSim.Utils;

class EntityData : ISpriteData, IBuildableData
{
    public string Source { get; set; }
    public string DefaultAnimationKey { get; set; }
    public Dictionary<string, AnimationData> Animations { get; set; }
    // Some entities are deployable as buildings (e.g. plant monsters as crops)
    public Zoning[] Buildable { get; set; } = Array.Empty<Zoning>();
}
