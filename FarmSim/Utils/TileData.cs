using System;
using System.Collections.Generic;

namespace FarmSim.Utils;

class TileData : ISpriteData, IBuildableData
{
    public string Source { get; set; }
    public string DefaultAnimationKey { get; set; }
    public Dictionary<string, AnimationData> Animations { get; set; }
    // What is buildable on this tile
    public Zoning[] Buildable { get; set; } = Array.Empty<Zoning>();
}
