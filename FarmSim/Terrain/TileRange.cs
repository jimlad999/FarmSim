using System.Linq;
using FarmSim.Utils;
using System.Collections.Generic;

namespace FarmSim.Terrain;

readonly struct TileRange
{
    public List<Tile> Tiles { get; init; }
    public int TopLeftX { get; init; }
    public int TopLeftY { get; init; }
    public int BottomRightX { get; init; }
    public int BottomRightY { get; init; }

    public TileRange(
        List<Tile> tiles,
        int topLeftX,
        int topLeftY,
        int bottomRightX,
        int bottomRightY)
    {
        Tiles = tiles;
        TopLeftX = topLeftX;
        TopLeftY = topLeftY;
        BottomRightX = bottomRightX;
        BottomRightY = bottomRightY;
    }

    public readonly bool AllTilesAreBuildable(
        ICollection<Zoning> buildable,
        Tileset tileset)
    {
        return Tiles
            .SelectMany(BuildingExtensions.YieldTilesets)
            .Distinct()
            .All(key => tileset[key].IsBuildable(buildable));
    }
}
