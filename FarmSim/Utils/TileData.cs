using System;
using Utils.Data;

namespace FarmSim.Utils;

class TileData : ISpriteData, IBuildableData
{
    public string Source { get; set; }
    // "origin" is from top left corner of the tile
    public OriginData Origin { get; set; }
    // What is buildable on this tile
    public Zoning[] Buildable { get; set; } = Array.Empty<Zoning>();
}
