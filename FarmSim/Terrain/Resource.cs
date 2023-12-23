using FarmSim.Entities;
using FarmSim.Rendering;

namespace FarmSim.Terrain;

class Resource : Entity
{
    public Resource(
        string tilesetKey,
        int tileX,
        int tileY)
    {
        EntitySpriteKey = tilesetKey;
        TileX = tileX;
        XInt = (tileX * Renderer.TileSize) + Renderer.TileSizeHalf;
        X = XInt;
        TileY = tileY;
        YInt = (tileY * Renderer.TileSize) + Renderer.TileSizeHalf;
        Y = YInt;
    }
}
