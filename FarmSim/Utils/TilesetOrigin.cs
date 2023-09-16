using Microsoft.Xna.Framework;

namespace FarmSim.Utils;

class TilesetOrigin
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2 Convert()
    {
        return new Vector2(x: X, y: Y);
    }
}
