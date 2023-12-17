using Utils.Data;

namespace FarmSim.Utils;

class TileData
{
    public string Source { get; set; }
    // "origin" is from top left corner of the tile
    public OriginData Origin { get; set; }
    // What is buildable on this tile
    public Zoning[] Buildable { get; set; }
}
