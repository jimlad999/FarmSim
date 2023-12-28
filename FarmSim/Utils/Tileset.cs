using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FarmSim.Utils;

class Tileset : SpriteSheet<TileData>
{
    public readonly Dictionary<string, TileData> Data;

    public Tileset(SpriteBatch spriteBatch, TilesetData tilesetData)
    {
        Data = tilesetData.Data;
        ProcessData(spriteBatch, tilesetData);
    }
}
