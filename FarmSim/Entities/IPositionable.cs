using FarmSim.Rendering;

namespace FarmSim.Entities;

interface IPositionable
{
    // world position
    double X { get; set; }
    double Y { get; set; }
    int XInt { get; set; }
    int YInt { get; set; }
    // tile index
    int TileX { get; set; }
    int TileY { get; set; }
}

static class PositionableExtentions
{
    public static void UpdateTileIndex(this IPositionable positionable)
    {
        positionable.TileX = positionable.XInt.ToTileIndex();
        positionable.TileY = positionable.YInt.ToTileIndex();
    }

    public static int ToTileIndex(this int value)
    {
        var tileIndex = value / Renderer.TileSize;
        if (value < 0) --tileIndex;
        return tileIndex;
    }
}
