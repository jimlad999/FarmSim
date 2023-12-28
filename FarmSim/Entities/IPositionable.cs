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
        positionable.TileX = positionable.XInt / Renderer.TileSize;
        if (positionable.XInt < 0) --positionable.TileX;
        positionable.TileY = positionable.YInt / Renderer.TileSize;
        if (positionable.YInt < 0) --positionable.TileY;
    }
}
